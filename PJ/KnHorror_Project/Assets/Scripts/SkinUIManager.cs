using UnityEngine;

public class SkinUIManager : MonoBehaviour
{
    [Header("UI Showcase Objects (ลาก UI แต่ละเซ็ตมาใส่ตามลำดับรหัส 0, 1, 2... เลยจ่ะแม่)")]
    public GameObject[] SkinUiShowcase;

    private const string SKIN_KEY = "SKIN_WEAR";

    private void Start()
    {
        UpdateSkinUI();
    }

    // 🖥️ ฟังก์ชันเช็ค PlayerPrefs แล้วเปิด/ปิด UI Showcase ตามรหัสสกิน
    public void UpdateSkinUI()
    {
        // ดึงค่ารหัสสกินปัจจุบันจาก PlayerPrefs (ถ้าไม่มีให้เป็น 0)
        int currentSkinIndex = PlayerPrefs.GetInt(SKIN_KEY, 0);

        // วนลูปเช็คเปิด/ปิด GameObject ในอาเรย์
        for (int i = 0; i < SkinUiShowcase.Length; i++)
        {
            if (SkinUiShowcase[i] != null)
            {
                // ถ้า Index ในอาเรย์ตรงกับรหัสสกินปัจจุบัน ให้เปิด (true) ถ้าไม่ตรงให้ปิด (false)
                bool shouldBeActive = (i == currentSkinIndex);
                SkinUiShowcase[i].SetActive(shouldBeActive);
            }
        }

        Debug.Log($"<color=cyan>🖼️ [SkinUI] อัปเดตการแสดงผล UI Showcase ตามรหัสสกิน: {currentSkinIndex}</color>");
    }
}