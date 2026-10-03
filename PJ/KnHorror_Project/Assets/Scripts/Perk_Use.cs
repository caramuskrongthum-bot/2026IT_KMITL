using UnityEngine;

public class Perk_Use : MonoBehaviour
{
    [Header("UI Object to Toggle")]
    public GameObject wearUiObject;

    [Header("UI To Hide When Purchased")]
    public GameObject Ui_IsplayerBye;

    [Header("Perk Key Name & Price")]
    public string perkKeyName = "PERK_01";
    public string moneyKeyName = "MONEY_DATA";
    public int perkPrice = 100;

    private void Start()
    {
        UpdateWearUiState();
    }

    public void TogglePerk()
    {
        int currentValue = PlayerPrefs.GetInt(perkKeyName, 0);

        if (currentValue == 0)
        {
            int currentMoney = PlayerPrefs.GetInt(moneyKeyName, 0);

            if (currentMoney < perkPrice)
            {
                Debug.LogWarning($"<color=red>💸 [Perk System] เงินหมดค่ะซิส! มี {currentMoney} แต่ต้องใช้ {perkPrice}</color>");
                return;
            }

            currentMoney -= perkPrice;
            PlayerPrefs.SetInt(moneyKeyName, currentMoney);
            Debug.Log($"<color=yellow>💸 [Perk System] หักเงินไป {perkPrice} บาท เหลือเงิน {currentMoney} บาท</color>");

            // พอกดซื้อสำเร็จ ซ่อน UI ซื้อทันทีแม่
            if (Ui_IsplayerBye != null)
            {
                Ui_IsplayerBye.SetActive(false);
            }
        }
        else
        {
            Debug.Log($"<color=orange>🔒 [Perk System] ปิดการใช้งานเปิร์ค {perkKeyName}</color>");
        }

        int newValue = (currentValue == 1) ? 0 : 1;

        PlayerPrefs.SetInt(perkKeyName, newValue);
        PlayerPrefs.Save();

        UpdateWearUiState();

        Debug.Log($"<color=cyan>✨ [Perk Toggle] Perk '{perkKeyName}' ถูกสลับสถานะเป็น: {newValue}</color>");
    }

    private void UpdateWearUiState()
    {
        bool isActive = PlayerPrefs.GetInt(perkKeyName, 0) == 1;

        if (wearUiObject != null)
        {
            wearUiObject.SetActive(isActive);
        }
    }
}