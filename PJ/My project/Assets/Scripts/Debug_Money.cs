using UnityEngine;

public class Debug_Money : MonoBehaviour
{
    public void AddMoney()
    {
        int currentMoney = PlayerPrefs.GetInt("MONEY", 0);

        // 2. บวกเงินเพิ่ม 100
        currentMoney += 100;

        // 3. บันทึกค่าเงินใหม่กลับลง PlayerPrefs
        PlayerPrefs.SetInt("MONEY", currentMoney);

        // 4. บันทึกข้อมูลลงดิสก์ทันที
        PlayerPrefs.Save();

        Debug.Log($"💵 เพิ่มเงินสำเร็จ! เงินปัจจุบัน: {currentMoney}");
    }
}