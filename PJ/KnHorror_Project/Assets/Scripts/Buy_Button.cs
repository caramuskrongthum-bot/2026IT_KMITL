using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Buy_Button : MonoBehaviour
{
    [Header("UI to Block/Cover")]
    public GameObject lockUiOverlay;

    [Header("The Buy Button")]
    public Button buyButton;

    [Header("Item & Money Settings")]
    public string itemId = "ITEM_01";
    public string moneyKeyName = "MONEY_DATA";
    public int itemPrice = 100;

    [Header("Events (สิ่งที่จะให้ทำ: ซื้อสำเร็จ หรือ กดใช้งานเมื่อซื้อแล้ว)")]
    public UnityEvent onBuySuccess;

    private void Start()
    {
        CheckPurchaseStatus();
    }

    private void OnEnable()
    {
        CheckPurchaseStatus();
    }

    private void CheckPurchaseStatus()
    {
        int isPurchased = PlayerPrefs.GetInt(itemId, 0);

        if (isPurchased == 1)
        {
            // ถ้าซื้อแล้ว -> ซ่อน UI บัง 
            // *หมายเหตุ: ตรงนี้เราปล่อยให้ buyButton.interactable = trueไว้นะแม่ เพื่อให้กดซ้ำเพื่อเรียก Event (เช่นกดสวมใส่) ได้!*
            if (lockUiOverlay != null) lockUiOverlay.SetActive(false);
            if (buyButton != null) buyButton.interactable = true;
        }
        else
        {
            // ถ้ายังไม่ซื้อ -> แสดง UI บัง และเปิดปุ่มให้กดซื้อได้
            if (lockUiOverlay != null) lockUiOverlay.SetActive(true);
            if (buyButton != null) buyButton.interactable = true;
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกตอนกดปุ่ม
    public void OnButtonClicked()
    {
        int isPurchased = PlayerPrefs.GetInt(itemId, 0);

        // 🟢 กรณีที่ 1: ซื้อไปแล้ว (กดซ้ำ) -> ไม่หักเงิน ไม่ซื้อซ้ำ แต่รัน Event ทันที (เช่น กดเพื่อสวมใส่สกิน)
        if (isPurchased == 1)
        {
            Debug.Log($"<color=cyan>✨ [Buy System] ไอเทม {itemId} ถูกซื้อไปแล้ว รัน Event ใช้งานทันทีโดยไม่หักเงินเพิ่มจ้าแม่!</color>");
            onBuySuccess?.Invoke();
            return;
        }

        // 💸 กรณีที่ 2: ยังไม่เคยซื้อ -> เช็คเงินและทำการซื้อ
        int currentMoney = PlayerPrefs.GetInt(moneyKeyName, 0);

        if (currentMoney < itemPrice)
        {
            Debug.LogWarning($"<color=red>💸 [Buy System] เงินไม่พอแม่! มี {currentMoney} แต่ต้องใช้ {itemPrice}</color>");
            return;
        }

        // หักเงิน
        currentMoney -= itemPrice;
        PlayerPrefs.SetInt(moneyKeyName, currentMoney);

        // บันทึกสถานะว่าซื้อแล้ว (เปลี่ยนเป็น 1)
        PlayerPrefs.SetInt(itemId, 1);
        PlayerPrefs.Save();

        Debug.Log($"<color=green>✨ [Buy System] ซื้อไอเทม {itemId} สำเร็จครั้งแรก! หักเงินไป {itemPrice} เหลือ {currentMoney}</color>");

        // ซ่อน UI บัง
        if (lockUiOverlay != null)
        {
            lockUiOverlay.SetActive(false);
        }

        // รัน Event ตอนซื้อสำเร็จครั้งแรก
        onBuySuccess?.Invoke();
    }
}