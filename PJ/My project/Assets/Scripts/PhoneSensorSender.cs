using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // เพิ่ม namespace สำหรับ UI

public class PhoneSensorSender : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void InitPeerServer(string gameId, string controllerUrl);

    public enum AxisSource { Zero, RotX, RotY, RotZ }

    public static PhoneSensorSender Instance { get; private set; }

    [Header("Scene Re-binding")]
    public string racketTag = "ItemTarget";
    public string playerTag = "PlayerTarget";
    [Tooltip("Tag ของ RawImage เลเซอร์ (กรณีต้องการให้ค้นหาอัตโนมัติ)")]
    public string laserTag = "LaserTarget";

    [Header("References")]
    public Transform racketTransform;
    public Transform playerTransform;
    [Tooltip("ใส่ RectTransform ของ RawImage ที่ต้องการใช้เป็นแสงเลเซอร์")]
    public RectTransform laserRawImage;

    [Header("Player Movement Settings (Joystick)")]
    public float moveSpeed = 5.0f;
    public float turnSpeed = 10.0f;

    [Header("Global Sensitivity")]
    public float sensitivity = 2.0f;

    [Header("Per-Axis Multipliers (ความแรงแยกแกน)")]
    [Range(0f, 5f)] public float xAxisMultiplier = 1.0f;
    [Range(0f, 5f)] public float yAxisMultiplier = 1.0f;
    [Range(0f, 5f)] public float zAxisMultiplier = 1.0f;

    [Header("Drift Correction & Smoothing")]
    public bool autoCenter = true;
    [Range(0.1f, 5.0f)] public float autoCenterSpeed = 1.5f;
    [Range(5f, 30f)] public float smoothingSpeed = 15.0f;

    [Header("Item Position Sliding")]
    public bool enablePositionSlide = true;
    public AxisSource tiltAxisSource = AxisSource.RotZ;
    public float positionSensitivity = 0.05f;
    public float maxPositionOffset = 1.5f;
    public float invertPosition = -1f;

    // ================== NEW: LASER POINTER SETTINGS ==================
    [Header("Laser Pointer (RawImage Screen Movement)")]
    public bool enableLaserPointer = true;
    public AxisSource laserXAxis = AxisSource.RotY; // แกนหมุนมือถือที่จะให้เลื่อซ้าย-ขวา
    public AxisSource laserYAxis = AxisSource.RotX; // แกนหมุนมือถือที่จะให้เลื่อนขึ้น-ลง
    public float laserSensitivity = 500f;           // ความไวการเคลื่อนที่จุดเลเซอร์ (Pixels)
    public Vector2 laserScreenBounds = new Vector2(960f, 540f); // ขอบเขตการวิ่ง (ครึ่งหนึ่งของ Screen Resolution)
    public float laserInvertX = 1f;
    public float laserInvertY = 1f;

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

    // --- Rotation & Position variables ---
    private Quaternion initialRacketRotation;
    private Quaternion targetRacketRotation;
    private Vector3 initialRacketPosition;
    private Vector3 targetRacketPosition;

    // --- NEW: Laser variables ---
    private Vector2 targetLaserPosition;
    private Vector2 initialLaserPosition;

    private Vector2 currentJoystickInput = Vector2.zero;
    private LocalPhoneServer _localServer;

    public bool IsServerReady { get; private set; }
    public string UniqueGameId { get; private set; }
    public string FullControllerUrl { get; private set; }

    public UnityEvent UnityEvent_Scaned;

    [Header("Deadzone / Threshold Settings")]
    [Tooltip("ค่าการหมุนขั้นต่ำที่ต้องข้ามผ่านก่อนที่ Object จะหมุนตาม (ช่วยกันมือสั่น)")]
    public float rotationThreshold = 0.05f;
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
        laserRawImage = null;

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

        // NEW: Automatic laser rebind by tag
        if (!string.IsNullOrEmpty(laserTag))
        {
            GameObject laserObj = GameObject.FindGameObjectWithTag(laserTag);
            if (laserObj != null) laserRawImage = laserObj.GetComponent<RectTransform>();
        }

        InitPositions();
    }

    private void InitPositions()
    {
        if (racketTransform != null)
        {
            initialRacketRotation = racketTransform.localRotation;
            targetRacketRotation = initialRacketRotation;

            initialRacketPosition = racketTransform.localPosition;
            targetRacketPosition = initialRacketPosition;
        }

        // NEW: Laser initial state
        if (laserRawImage != null)
        {
            initialLaserPosition = laserRawImage.anchoredPosition;
            targetLaserPosition = initialLaserPosition;
        }
    }

    void Start()
    {
        if (IsServerReady) return;

        InitPositions();

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

        // 1. ควบคุม Item / Racket
        if (racketTransform != null)
        {
            if (autoCenter)
            {
                targetRacketRotation = Quaternion.Slerp(targetRacketRotation, initialRacketRotation, Time.deltaTime * autoCenterSpeed);
                targetRacketPosition = Vector3.Lerp(targetRacketPosition, initialRacketPosition, Time.deltaTime * autoCenterSpeed);
            }

            racketTransform.localRotation = Quaternion.Slerp(racketTransform.localRotation, targetRacketRotation, Time.deltaTime * smoothingSpeed);
            racketTransform.localPosition = Vector3.Lerp(racketTransform.localPosition, targetRacketPosition, Time.deltaTime * smoothingSpeed);
        }

        // 2. ควบคุมเลื่อน RawImage (Laser Pointer) - ตัดการ autoCenter ออก
        if (enableLaserPointer && laserRawImage != null)
        {
            // ลบเงื่อนไข autoCenter ของ laser ออก เพื่อให้ cursor ค้างอยู่ที่ตำแหน่งล่าสุดเสมอ
            laserRawImage.anchoredPosition = Vector2.Lerp(laserRawImage.anchoredPosition, targetLaserPosition, Time.deltaTime * smoothingSpeed);
        }

        // 3. ควบคุมตัวละคร
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
        if (data == "ACTION_CLICK")
        {
            Debug.Log("🎯 [PhoneSensorSender] ได้รับสัญญาณ ACTION_CLICK จากมือถือ");

            // ค้นหา PhoneButtonListener ในฉาก แล้วสั่งให้ทำงาน
            PhoneButtonListener listener = FindObjectOfType<PhoneButtonListener>();
            if (listener != null)
            {
                listener.HandleButtonClick();
            }
            return;
        }

        // --- 2. การทำงานดั้งเดิมของคุณ ---
        UnityEvent_Scaned.Invoke();

        string[] values = data.Split(',');
        if (values.Length >= 5)
        {
            if (float.TryParse(values[0], out float gx) &&
                float.TryParse(values[1], out float gy) &&
                float.TryParse(values[2], out float gz))
            {
                // 1. นำค่าเซนเซอร์มากรองด้วย Threshold ก่อนนำไปคำนวณต่อ
                float filteredGx = ApplyThreshold(gx);
                float filteredGy = ApplyThreshold(gy);
                float filteredGz = ApplyThreshold(gz);

                // 2. คำนวณความเร็วตามแกนที่กรองค่าแล้ว
                float rotX = filteredGx * sensitivity * Time.deltaTime * invertX;
                float rotY = filteredGy * sensitivity * Time.deltaTime * invertY;
                float rotZ = filteredGz * sensitivity * Time.deltaTime * invertZ;

                // --- คำนวณ 3D Racket ---
                if (racketTransform != null)
                {
                    float rawX = GetAxisValue(xAxisSource, rotX, rotY, rotZ);
                    float rawY = GetAxisValue(yAxisSource, rotX, rotY, rotZ);
                    float rawZ = GetAxisValue(zAxisSource, rotX, rotY, rotZ);

                    float finalX = rawX * xAxisMultiplier;
                    float finalY = rawY * yAxisMultiplier;
                    float finalZ = rawZ * zAxisMultiplier;

                    Quaternion deltaRotation = Quaternion.Euler(finalX, finalY, finalZ);
                    targetRacketRotation = targetRacketRotation * deltaRotation;

                    if (enablePositionSlide)
                    {
                        float tiltValue = GetAxisValue(tiltAxisSource, rotX, rotY, rotZ);
                        float offsetX = tiltValue * positionSensitivity * invertPosition;

                        targetRacketPosition.x += offsetX;
                        float clampedX = Mathf.Clamp(
                            targetRacketPosition.x,
                            initialRacketPosition.x - maxPositionOffset,
                            initialRacketPosition.x + maxPositionOffset
                        );

                        targetRacketPosition = new Vector3(clampedX, targetRacketPosition.y, targetRacketPosition.z);
                    }
                }

                // --- คำนวณ 2D Laser Pointer (RawImage) ---
                if (enableLaserPointer && laserRawImage != null)
                {
                    float rawLaserX = GetAxisValue(laserXAxis, rotX, rotY, rotZ) * laserInvertX;
                    float rawLaserY = GetAxisValue(laserYAxis, rotX, rotY, rotZ) * laserInvertY;

                    Vector2 laserDelta = new Vector2(rawLaserX, rawLaserY) * laserSensitivity;
                    targetLaserPosition += laserDelta;

                    targetLaserPosition.x = Mathf.Clamp(targetLaserPosition.x, -laserScreenBounds.x, laserScreenBounds.x);
                    targetLaserPosition.y = Mathf.Clamp(targetLaserPosition.y, -laserScreenBounds.y, laserScreenBounds.y);
                }
            }

            if (float.TryParse(values[3], out float jx) &&
                float.TryParse(values[4], out float jy))
            {
                currentJoystickInput = new Vector2(jx, jy);
            }
        }
    }
    // ฟังก์ชันช่วยตัดค่าที่ต่ำกว่า Threshold ให้กลายเป็น 0
    private float ApplyThreshold(float inputVal)
    {
        if (Mathf.Abs(inputVal) < rotationThreshold)
        {
            return 0f;
        }
        // หักลบค่า threshold ออกเล็กน้อยเพื่อไม่ให้ค่ากระตุกกระทันหันตอนข้าม threshold
        return Mathf.Sign(inputVal) * (Mathf.Abs(inputVal) - rotationThreshold);
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