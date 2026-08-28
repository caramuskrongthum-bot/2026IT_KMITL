using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

// สคริปต์นี้ควรอยู่เฉพาะใน Scene "Menu" (จุดที่ให้ผู้เล่นสแกน QR ครั้งแรก)
// หลังจากสแกนแล้ว ไม่ต้องมีสคริปต์นี้อยู่ใน Scene อื่นอีก เพราะ PhoneSensorSender
// จะ persist ข้าม Scene ไปเอง (DontDestroyOnLoad) และ connection ยังใช้งานได้ต่อเนื่อง
public class QRCodeGenerator : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage qrCodeDisplay; // ลาก RawImage หรือ Image มาวางตรงนี้
    public TMP_Text urlDisplayText; // ลาก TextMeshPro มาวางตรงนี้

    [Header("Settings")]
    [Tooltip("ถ้าเว้นว่างไว้ จะใช้ PhoneSensorSender.Instance ที่มีอยู่แล้วโดยอัตโนมัติ " +
             "(ไม่จำเป็นต้องลากใส่เอง ตราบใดที่มี PhoneSensorSender อยู่ใน Scene นี้แล้ว)")]
    public PhoneSensorSender phoneSensorSender;

    [Tooltip("จำนวนครั้งที่จะลองใหม่ ถ้า FullControllerUrl ยังไม่พร้อมตอนเฟรมแรก")]
    public int retryFrames = 30;

    void Start()
    {
        // ถ้าไม่ได้ลาก reference มาเอง ให้ใช้ Singleton instance แทน
        if (phoneSensorSender == null)
        {
            phoneSensorSender = PhoneSensorSender.Instance;
        }

        if (phoneSensorSender == null)
        {
            Debug.LogError("QRCodeGenerator: ไม่พบ PhoneSensorSender ทั้งใน Inspector และ Instance — " +
                            "ตรวจสอบว่ามี PhoneSensorSender วางอยู่ใน Scene นี้แล้ว (เช่น Scene Menu)");
            return;
        }

        StartCoroutine(WaitForUrlThenGenerate());
    }

    IEnumerator WaitForUrlThenGenerate()
    {
        // PhoneSensorSender.Start() อาจยังรันไม่เสร็จ (เช่น กำลัง bind server) ในเฟรมแรก
        // เลยรอสักไม่กี่เฟรมให้ FullControllerUrl ถูก set ก่อน แทนที่จะยิง QR request ทันที
        int frames = 0;
        while (string.IsNullOrEmpty(phoneSensorSender.FullControllerUrl) && frames < retryFrames)
        {
            frames++;
            yield return null;
        }

        string uniqueGameId = phoneSensorSender.UniqueGameId;
        string fullUrl = phoneSensorSender.FullControllerUrl;

        if (string.IsNullOrEmpty(fullUrl))
        {
            Debug.LogError("QRCodeGenerator: PhoneSensorSender.FullControllerUrl ยังว่างอยู่ " +
                            "(เซิร์ฟเวอร์อาจ start ไม่สำเร็จ) — ไม่สร้าง QR Code เพื่อกัน error 400 จาก API");
            if (urlDisplayText != null)
            {
                urlDisplayText.text = "เกิดข้อผิดพลาด: ไม่สามารถสร้าง URL สำหรับควบคุมได้";
            }
            yield break;
        }

        if (urlDisplayText != null)
        {
            urlDisplayText.text = " Scan QR Code or Open:\n" + fullUrl;
        }

        StartCoroutine(GenerateQRCode(fullUrl));
    }

    IEnumerator GenerateQRCode(string urlToEncode)
    {
        // ใช้ API เจนรูป QR Code ขนาด 300x300
        string apiUrl = "https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=" + UnityWebRequest.EscapeURL(urlToEncode);

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // ดึงภาพ Texture ที่โหลดมาใส่ลงใน RawImage
                Texture2D qrTexture = DownloadHandlerTexture.GetContent(request);
                if (qrCodeDisplay != null)
                {
                    qrCodeDisplay.texture = qrTexture;
                }
            }
            else
            {
                Debug.LogError("QR Code Load Failed: " + request.error);
            }
        }
    }
}