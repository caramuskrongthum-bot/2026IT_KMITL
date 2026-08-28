using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhoneSensorSender : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void InitPeerServer(string gameId, string controllerUrl);

    public enum AxisSource { Zero, RotX, RotY, RotZ }

    // ================== SINGLETON ==================
    public static PhoneSensorSender Instance { get; private set; }

    [Header("Scene Re-binding")]
    [Tooltip("Tag ของ Transform ไม้/ดาบ ที่ต้องมีอยู่ใน Scene ปัจจุบัน (ปล่อยว่างได้ถ้า Scene นั้นไม่มี)")]
    public string racketTag = "ItemTarget";
    [Tooltip("Tag ของ Transform ตัวละคร ที่ต้องมีอยู่ใน Scene ปัจจุบัน (ปล่อยว่างได้ถ้า Scene นั้นไม่มี)")]
    public string playerTag = "PlayerTarget";

    [Header("References (จะถูกหาใหม่อัตโนมัติทุกครั้งที่เปลี่ยน Scene)")]
    public Transform racketTransform;
    public Transform playerTransform;

    [Header("Player Movement Settings (Joystick)")]
    public float moveSpeed = 5.0f;
    public float turnSpeed = 10.0f;

    [Header("Global Sensitivity")]
    public float sensitivity = 2.0f;

    [Header("Per-Axis Multipliers (ความแรงแยกแกน)")]
    [Range(0f, 5f)] public float xAxisMultiplier = 1.0f;
    [Range(0f, 5f)] public float yAxisMultiplier = 1.0f;
    [Range(0f, 5f)] public float zAxisMultiplier = 1.0f;

    [Header("Drift Correction & Smoothing (แก้แกนเบี้ยว)")]
    public bool autoCenter = true;
    [Range(0.1f, 5.0f)] public float autoCenterSpeed = 1.5f;
    [Range(5f, 30f)] public float smoothingSpeed = 15.0f;

    // ================== NEW: POSITION TILT SETTINGS ==================
    [Header("Item Position Sliding (เอียงแล้วเลื่อน ซ้าย-ขวา)")]
    [Tooltip("เปิดใช้งานการเลื่อนตำแหน่ง X ของ ItemTarget ตามการเอียงมือถือ")]
    public bool enablePositionSlide = true;
    [Tooltip("แกน Gyro ที่จะใช้อ้างอิงการเอียงซ้าย-ขวา (ปกติจะใช้ RotZ หรือ RotY)")]
    public AxisSource tiltAxisSource = AxisSource.RotZ;
    [Tooltip("ความไวในการเลื่อนตำแหน่ง")]
    public float positionSensitivity = 0.05f;
    [Tooltip("ระยะการเลื่อนซ้าย-ขวาสุดจากจุดเริ่มต้น (Local Space)")]
    public float maxPositionOffset = 1.5f;
    [Tooltip("การกลับทิศการเลื่อน (-1 หรือ 1)")]
    public float invertPosition = -1f;

    [Header("Controller & Server Settings")]
    public string controllerBaseUrl = "https://your-domain.com/controller.html";
    public string controllerHtmlFileName = "controller.html";
    public int standaloneServerPort = 7777;

    [Header("Dynamic Axis Selection")]
    public AxisSource xAxisSource = AxisSource.RotX;
    public AxisSource yAxisSource = AxisSource.Zero;
    public AxisSource zAxisSource = AxisSource.RotY;
    public float invertX = -1f;
    public float invertY = 1f;
    public float invertZ = -1f;

    // --- Rotation variables ---
    private Quaternion initialRacketRotation;
    private Quaternion targetRacketRotation;

    // --- NEW: Position variables ---
    private Vector3 initialRacketPosition;
    private Vector3 targetRacketPosition;

    // Vector2 สำหรับเก็บทิศทางการเดินจาก Joystick
    private Vector2 currentJoystickInput = Vector2.zero;
    private LocalPhoneServer _localServer;

    public bool IsServerReady { get; private set; }
    public string UniqueGameId { get; private set; }
    public string FullControllerUrl { get; private set; }

    public GameObject ScanUi;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        UniqueGameId = "TENNIS-" + Random.Range(1000, 9999);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindTargets();
    }

    public void RebindTargets()
    {
        racketTransform = null;
        playerTransform = null;

        if (!string.IsNullOrEmpty(racketTag))
        {
            GameObject racketObj = GameObject.FindGameObjectWithTag(racketTag);
            if (racketObj != null) racketTransform = racketObj.transform;
        }

        if (!string.IsNullOrEmpty(playerTag))
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        if (racketTransform != null)
        {
            initialRacketRotation = racketTransform.localRotation;
            targetRacketRotation = initialRacketRotation;

            // NEW: บันทึกตำแหน่งเริ่มต้นในระดับ Local Space
            initialRacketPosition = racketTransform.localPosition;
            targetRacketPosition = initialRacketPosition;
        }
    }

    void Start()
    {
        if (IsServerReady) return;

        if (racketTransform != null)
        {
            initialRacketRotation = racketTransform.localRotation;
            targetRacketRotation = initialRacketRotation;

            initialRacketPosition = racketTransform.localPosition;
            targetRacketPosition = initialRacketPosition;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        FullControllerUrl = controllerBaseUrl + "?gameId=" + UniqueGameId;
        InitPeerServer(UniqueGameId, FullControllerUrl);
        IsServerReady = true;
#else
        StartStandaloneServer();
#endif
    }

    void StartStandaloneServer()
    {
        string htmlPath = Path.Combine(Application.streamingAssetsPath, controllerHtmlFileName);
        string html = File.Exists(htmlPath) ? File.ReadAllText(htmlPath) : "Fallback Page";

        string localIp = LocalPhoneServer.GetLocalIPAddress();
        _localServer = new LocalPhoneServer(html, standaloneServerPort);
        try
        {
            _localServer.Start();
            IsServerReady = true;
        }
        catch
        {
            IsServerReady = false;
        }

        FullControllerUrl = $"http://{localIp}:{standaloneServerPort}/?gameId={UniqueGameId}";
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (_localServer != null)
        {
            while (_localServer.TryDequeue(out string msg))
            {
                OnReceiveMotionData(msg);
            }
        }
#endif

        // --- 1. ระบบควบคุมการหมุน และการเลื่อนตำแหน่งไม้/ดาบ ---
        if (racketTransform != null)
        {
            if (autoCenter)
            {
                // Auto-center ทั้งการหมุนและตำแหน่ง
                targetRacketRotation = Quaternion.Slerp(targetRacketRotation, initialRacketRotation, Time.deltaTime * autoCenterSpeed);
                targetRacketPosition = Vector3.Lerp(targetRacketPosition, initialRacketPosition, Time.deltaTime * autoCenterSpeed);
            }

            // Smooth Interpolation
            racketTransform.localRotation = Quaternion.Slerp(racketTransform.localRotation, targetRacketRotation, Time.deltaTime * smoothingSpeed);
            racketTransform.localPosition = Vector3.Lerp(racketTransform.localPosition, targetRacketPosition, Time.deltaTime * smoothingSpeed);
        }

        // --- 2. ระบบบังคับตัวละครเดินจาก Joystick ---
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (playerTransform == null || currentJoystickInput.magnitude < 0.1f) return;

        Vector3 moveDirection = new Vector3(currentJoystickInput.x, 0, currentJoystickInput.y);
        playerTransform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, Time.deltaTime * turnSpeed);
    }

    public void OnReceiveMotionData(string data)
    {
        if (ScanUi != null)
        {
            ScanUi.SetActive(false);
        }
        string[] values = data.Split(',');
        if (values.Length >= 5)
        {
            if (float.TryParse(values[0], out float gx) &&
                float.TryParse(values[1], out float gy) &&
                float.TryParse(values[2], out float gz))
            {
                if (racketTransform != null)
                {
                    float rotX = gx * sensitivity * Time.deltaTime * invertX;
                    float rotY = gy * sensitivity * Time.deltaTime * invertY;
                    float rotZ = gz * sensitivity * Time.deltaTime * invertZ;

                    // --- คำนวณการหมุน (Rotation) ---
                    float rawX = GetAxisValue(xAxisSource, rotX, rotY, rotZ);
                    float rawY = GetAxisValue(yAxisSource, rotX, rotY, rotZ);
                    float rawZ = GetAxisValue(zAxisSource, rotX, rotY, rotZ);

                    float finalX = rawX * xAxisMultiplier;
                    float finalY = rawY * yAxisMultiplier;
                    float finalZ = rawZ * zAxisMultiplier;

                    Quaternion deltaRotation = Quaternion.Euler(finalX, finalY, finalZ);
                    targetRacketRotation = targetRacketRotation * deltaRotation;

                    // --- NEW: คำนวณการเลื่อนตำแหน่ง ซ้าย-ขวา (Position Slide) ---
                    if (enablePositionSlide)
                    {
                        float tiltValue = GetAxisValue(tiltAxisSource, rotX, rotY, rotZ);
                        float offsetX = tiltValue * positionSensitivity * invertPosition;

                        // เพิ่ม Offset เข้าไปที่แกน X (Local Position)
                        targetRacketPosition.x += offsetX;

                        // Limit ไม่ให้เลื่อนเกินขอบเขตที่ตั้งไว้ (Clamping relative to initial position)
                        float clampedX = Mathf.Clamp(
                            targetRacketPosition.x,
                            initialRacketPosition.x - maxPositionOffset,
                            initialRacketPosition.x + maxPositionOffset
                        );

                        targetRacketPosition = new Vector3(clampedX, targetRacketPosition.y, targetRacketPosition.z);
                    }
                }
            }

            if (float.TryParse(values[3], out float jx) &&
                float.TryParse(values[4], out float jy))
            {
                currentJoystickInput = new Vector2(jx, jy);
            }
        }
    }

    private float GetAxisValue(AxisSource source, float rx, float ry, float rz)
    {
        switch (source)
        {
            case AxisSource.RotX: return rx;
            case AxisSource.RotY: return ry;
            case AxisSource.RotZ: return rz;
            default: return 0f;
        }
    }

    public void TriggerVibrate()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
#else
        if (_localServer != null)
        {
            _localServer.SendToAll("vibrate");
        }
#endif
    }

    void OnApplicationQuit()
    {
        _localServer?.Stop();
    }
}