using TMPro;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("Shop Settings")]
    [Tooltip("ราคาอัปเกรดดาเมจ")]
    public int upgradeDamagePrice = 100;
    [Tooltip("ราคาซื้อหัวใจเพิ่ม 1 ดวง")]
    public int upgradeHealthPrice = 50; // ตั้งไว้ 50 บาทตามต้องการ

    [Header("UI Elements (Optional)")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI damageAdditionText;
    public TextMeshProUGUI healthAdditionText; // ข้อความแสดงโบนัสหัวใจ (ถ้ามี)

    private void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// ฟังก์ชันสำหรับผูกกับ On Click () ของปุ่มซื้อดาเมจ
    /// </summary>
    public void BuyDamageUpgrade()
    {
        int currentMoney = PlayerPrefs.GetInt("MONEY", 0);

        if (currentMoney >= upgradeDamagePrice)
        {
            currentMoney -= upgradeDamagePrice;
            PlayerPrefs.SetInt("MONEY", currentMoney);

            int currentDamageAddition = PlayerPrefs.GetInt("PLAYER_DAMAGE_ADDITION", 0);
            currentDamageAddition += 50;
            PlayerPrefs.SetInt("PLAYER_DAMAGE_ADDITION", currentDamageAddition);
            PlayerPrefs.Save();

            ShowAlert($"Attack got buff +50! Total: {currentDamageAddition}");
            UpdateUI();
        }
        else
        {
            ShowAlert("Not enough money!");
        }
    }

    /// <summary>
    /// ฟังก์ชันสำหรับผูกกับ On Click () ของปุ่มซื้อหัวใจ (ราคา 50 บาท)
    /// </summary>
    public void BuyHealthUpgrade()
    {
        int currentMoney = PlayerPrefs.GetInt("MONEY", 0);

        if (currentMoney >= upgradeHealthPrice)
        {
            // 1. หักเงิน 50 บาท
            currentMoney -= upgradeHealthPrice;
            PlayerPrefs.SetInt("MONEY", currentMoney);

            // 2. เพิ่มหัวใจ +1 ดวง
            int currentHealthAddition = PlayerPrefs.GetInt("PLAYER_HEALTH_ADDITION", 0);
            currentHealthAddition += 1;
            PlayerPrefs.SetInt("PLAYER_HEALTH_ADDITION", currentHealthAddition);

            PlayerPrefs.Save();

            ShowAlert("+1 Heart Upgraded!");
            Debug.Log($"🛒 ซื้อหัวใจสำเร็จ! เงินคงเหลือ: {currentMoney} | หัวใจโบนัส: +{currentHealthAddition}");

            UpdateUI();
        }
        else
        {
            ShowAlert("Not enough money!");
        }
    }

    private void ShowAlert(string message)
    {
        GameObject alertObj = GameObject.FindGameObjectWithTag("Alert_Manager");
        if (alertObj != null && alertObj.TryGetComponent<Game_Score_Manager>(out var alertManager))
        {
            alertManager.Get_Score(message);
        }
    }

    public void UpdateUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "Money: " + PlayerPrefs.GetInt("MONEY", 0).ToString();
        }

        if (damageAdditionText != null)
        {
            damageAdditionText.text = "Damage +: " + PlayerPrefs.GetInt("PLAYER_DAMAGE_ADDITION", 0).ToString();
        }

        if (healthAdditionText != null)
        {
            healthAdditionText.text = "Extra Hearts +: " + PlayerPrefs.GetInt("PLAYER_HEALTH_ADDITION", 0).ToString();
        }
    }
}