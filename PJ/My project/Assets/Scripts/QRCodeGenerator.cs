using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class QRCodeGenerator : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage qrCodeDisplay;
    public TMP_Text urlDisplayText;

    [Header("Settings")]
    public PhoneSensorSender phoneSensorSender;
    public int maxWaitSeconds = 10;

    void Start()
    {
        if (phoneSensorSender == null)
        {
            phoneSensorSender = PhoneSensorSender.Instance;
        }

        if (phoneSensorSender == null)
        {
            Debug.LogError("QRCodeGenerator: ไม่พบ PhoneSensorSender ใน Scene");
            return;
        }

        StartCoroutine(WaitForUrlThenGenerate());
    }

    IEnumerator WaitForUrlThenGenerate()
    {
        float timer = 0f;

        // รอจนกว่า PhoneSensorSender จะสร้าง URL และยืนยันการเชื่อมต่อ PeerServer สำเร็จ
        while (string.IsNullOrEmpty(phoneSensorSender.FullControllerUrl) && timer < maxWaitSeconds)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        string fullUrl = phoneSensorSender.FullControllerUrl;

        if (string.IsNullOrEmpty(fullUrl))
        {
            Debug.LogError("QRCodeGenerator: ไม่สามารถเชื่อมต่อ PeerServer ได้ตามเวลาที่กำหนด");
            if (urlDisplayText != null)
            {
                urlDisplayText.text = "เกิดข้อผิดพลาด: เซิร์ฟเวอร์ไม่ตอบสนอง";
            }
            yield break;
        }

        if (urlDisplayText != null)
        {
            urlDisplayText.text = "Scan QR Code or Open:\n" + fullUrl;
        }

        StartCoroutine(GenerateQRCode(fullUrl));
    }

    IEnumerator GenerateQRCode(string urlToEncode)
    {
        string apiUrl = "https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=" + UnityWebRequest.EscapeURL(urlToEncode);

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
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