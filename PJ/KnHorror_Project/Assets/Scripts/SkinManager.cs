using UnityEngine;

public class SkinManager : MonoBehaviour
{
    [System.Serializable]
    public struct SkinSet
    {
        [Header("Target Skinned Mesh Renderer")]
        public SkinnedMeshRenderer hairRenderer;
        public SkinnedMeshRenderer shirtRenderer;
        public SkinnedMeshRenderer skirtRenderer;
        public SkinnedMeshRenderer faceRenderer;

        [Header("Materials")]
        public Material materialHair;
        public Material materialShirt;
        public Material materialSkirt;
        public Material materialFace;

        [Header("Meshes")]
        public Mesh meshHair;
        public Mesh meshShirt;
        public Mesh meshSkirt;
        public Mesh meshFace;
    }

    [Header("Skin Inventory (ช่อง [ ] เซ็ตชุดต่างๆ ตามรหัส SKIN_WEAR)")]
    public SkinSet[] availableSkins;

    private const string SKIN_KEY = "SKIN_WEAR";

    public bool ThisIsPlayerModel = true;

    private void Start()
    {
        if (ThisIsPlayerModel)
        {
            ApplyWornSkin();
        }
    }

    // 🎨 ฟังก์ชันเช็ค PlayerPrefs และเปลี่ยนสكينตามรหัสที่สวมใส่
    public void ApplyWornSkin()
    {
        // ดึงค่ารหัสสกินปัจจุบันจาก PlayerPrefs (ถ้าไม่มีให้เป็น 0)
        int currentSkinIndex = PlayerPrefs.GetInt(SKIN_KEY, 0);

        // เช็คว่ารหัสที่ได้อยู่ในขอบเขตอาเรย์ที่เราตั้งไว้ไหม เพื่อป้องกัน Error Out of Bounds
        if (currentSkinIndex >= 0 && currentSkinIndex < availableSkins.Length)
        {
            SkinSet skin = availableSkins[currentSkinIndex];

            // 1. เปลี่ยนทรงผม (Hair)
            ApplyPart(skin.hairRenderer, skin.meshHair, skin.materialHair);

            // 2. เปลี่ยนเสื้อ (Shirt)
            ApplyPart(skin.shirtRenderer, skin.meshShirt, skin.materialShirt);

            // 3. เปลี่ยนกระโปรง (Skirt)
            ApplyPart(skin.skirtRenderer, skin.meshSkirt, skin.materialSkirt);

            // 4. เปลี่ยนหน้า (Face)
            ApplyPart(skin.faceRenderer, skin.meshFace, skin.materialFace);

            Debug.Log($"<color=magenta>👗 [SkinManager] สวมใส่สกินเซ็ตที่รหัส: {currentSkinIndex} เรียบร้อยจ่ะแม่!</color>");
        }
        else
        {
            Debug.LogWarning($"⚠️ [SkinManager] ไม่พบเซ็ตสกินรหัสที่ {currentSkinIndex} ในอาเรย์ (ตรวจสอบขนาดช่อง Size หรือค่า PlayerPrefs)");
        }
    }

    // 🛠️ Helper Method: ฟังก์ชันย่อยช่วยเปลี่ยน Mesh และ Material ให้ปลอดภัย
    private void ApplyPart(SkinnedMeshRenderer renderer, Mesh targetMesh, Material targetMat)
    {
        if (renderer != null)
        {
            // เปลี่ยน Mesh (ถ้ามีการกำหนดค่ามา)
            if (targetMesh != null)
            {
                renderer.sharedMesh = targetMesh;
            }

            // เปลี่ยน Material (ถ้ามีการกำหนดค่ามา)
            if (targetMat != null)
            {
                renderer.material = targetMat;
            }
        }
    }

    // 🔄 (แถม) ฟังก์ชันสำหรับให้ปุ่มกดเปลี่ยนสกินแล้วบันทึกลง PlayerPrefs ทันที
    public void ChangeAndSaveSkin(int skinIndex)
    {
        PlayerPrefs.SetInt(SKIN_KEY, skinIndex);
        PlayerPrefs.Save();
    }
}