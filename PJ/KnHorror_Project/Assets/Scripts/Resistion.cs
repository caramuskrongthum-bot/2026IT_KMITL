using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Resistion : MonoBehaviour
{
    public Slider UiSlider; //
    public UnityEvent ResistionEvent; //[cite: 8]

    [Header("Spawn Settings")]
    public GameObject RealFormBlackHood; // Prefab ร่างจริงที่จะ Spawn ออกมา

    void Update()
    {
        UiSlider.value += 0.01f; //[cite: 8]
        UiSlider.value = Mathf.Clamp(UiSlider.value, 0, UiSlider.maxValue); //[cite: 8]
    }

    public void Resistion_Press()
    {
        UiSlider.value += 5f; //[cite: 8]
        UiSlider.value = Mathf.Clamp(UiSlider.value, 0, UiSlider.maxValue); //[cite: 8]

        if (UiSlider.value >= UiSlider.maxValue)
        {
            // 1. ปลดหลุดจากการโดนอุ้ม
            GameObject P = GameObject.FindGameObjectWithTag("Player"); //[cite: 8]
            if (P != null)
            {
                P.transform.parent = null; //[cite: 8]
            }

            // 2. เรียกฟังก์ชันจัดการสลับร่างมอนสเตอร์ BlackHood
            ReplaceBlackHoodWithRealForm();

            UiSlider.value = 0f; //[cite: 8]
            ResistionEvent.Invoke(); //[cite: 8]
        }
    }

    // ----------------------------------------------------
    // 💥 PUBLIC METHOD: ค้นหา ทำลาย และ Spawn ร่างจริง
    // ----------------------------------------------------
    public void ReplaceBlackHoodWithRealForm()
    {
        // หา GameObject ที่มี Tag "BlackHood" ในฉาก
        GameObject blackHood = GameObject.FindGameObjectWithTag("BlackHood");

        if (blackHood != null)
        {
            // เก็บ ตำแหน่ง (Position) และ ทิศทาง (Rotation) เดิมไว้ก่อน
            Vector3 spawnPosition = blackHood.transform.position;
            Quaternion spawnRotation = blackHood.transform.rotation;

            // ทำลายตัวเดิมทิ้ง
            Destroy(blackHood);

            // Spawn ร่างจริง (RealFormBlackHood) ออกมาในจุดเดิม
            if (RealFormBlackHood != null)
            {
                Instantiate(RealFormBlackHood, spawnPosition, spawnRotation);
            }
            else
            {
                Debug.LogWarning("Resistion: ยังไม่ได้ใส่ RealFormBlackHood ใน Inspector จ้า!");
            }
        }
        else
        {
            Debug.LogWarning("Resistion: ไม่พบ GameObject ที่มี Tag 'BlackHood' ในฉาก!");
        }
    }
}