using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetInspectorLister : EditorWindow
{
    private static List<AssetCreditInfo> scannedAssets = new List<AssetCreditInfo>();
    private Vector2 scrollPos;

    [System.Serializable]
    public class AssetCreditInfo
    {
        public string assetFolderName; // ชื่อโฟลเดอร์หรือแพ็กเกจที่ Import เข้ามา
        public string authorOrOwner;   // ชื่อเจ้าของ / คนทำ
    }

    [MenuItem("Tools/FOG OB-OB/Scan Imported Assets & Credits")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<AssetInspectorLister>("Imported Asset Scanner");
        ScanProjectAssets();
    }

    private static void ScanProjectAssets()
    {
        scannedAssets.Clear();

        // ไปสแกนดูโฟลเดอร์ข้างใน Assets/ ว่ามีโฟลเดอร์อะไรบ้าง (ส่วนใหญ่ Asset ที่ Import มาจะแยกเป็นโฟลเดอร์ของตัวเอง)
        string assetsPath = Application.dataPath;
        string[] directories = Directory.GetDirectories(assetsPath);

        foreach (string dir in directories)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            string folderName = dirInfo.Name;

            // กรองโฟลเดอร์ระบบของ Unity ออก (เช่น Scenes, Scripts, Materials, Prefabs ฯลฯ เอาเฉพาะโฟลเดอร์แปลกปลอมที่เป็น Asset นอกบ้าน)
            if (IsSystemFolder(folderName)) continue;

            scannedAssets.Add(new AssetCreditInfo
            {
                assetFolderName = folderName,
                authorOrOwner = GuessAuthorFromFolder(folderName) // ลองเดาชื่อหรือใส่ค่าเริ่มต้นให้
            });
        }

        Debug.Log($"✨ สแกนเจอ Asset ที่ Import เข้ามาในโฟลเดอร์ Assets ทั้งหมด {scannedAssets.Count} รายการจ่ะแม่!");
    }

    // เช็คว่าเป็นโฟลเดอร์ระบบของโปรเจกต์ไหม ถ้าใช่ให้ข้าม
    private static bool IsSystemFolder(string folderName)
    {
        string[] systemFolders = { "Scenes", "Scripts", "Prefabs", "Materials", "Textures", "Animations", "Animation", "Audio", "Sprites", "UI", "Plugins", "Settings", "Editor" };
        foreach (string sys in systemFolders)
        {
            if (folderName.Equals(sys, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    // ฟังก์ชันช่วยเดาชื่อคร่าวๆ จากชื่อโฟลเดอร์ (ถ้าจำไม่ได้ค่อยไปแก้ในหน้าต่าง UI)
    private static string GuessAuthorFromFolder(string folderName)
    {
        if (folderName.Contains("Cartoon FX")) return "Jean Moreno (Cartoon FX)";
        if (folderName.Contains("Furniture")) return "Asset Store Creator";
        if (folderName.Contains("Dissolve")) return "Shader Creator";
        return "ระบุชื่อเจ้าของ/คนทำที่นี่";
    }

    private void OnGUI()
    {
        GUILayout.Label("🛠️ FOG OB-OB Imported Asset Credit Formatter", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("ระบบจะสแกนหาโฟลเดอร์ Asset ที่หนู Import เข้ามาในโปรเจกต์โดยตรง หนสามารถตรวจเช็คและแก้ชื่อคนทำได้ด้านล่างนี้เลยจ่ะแม่!", MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("🔄 สแกนหา Asset ในโปรเจกต์ใหม่อีกรอบ", GUILayout.Height(30)))
        {
            ScanProjectAssets();
        }

        EditorGUILayout.Space();
        GUILayout.Label("📋 รายชื่อ Asset ที่พบและเครดิตคนทำ:", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
        foreach (var item in scannedAssets)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📦 ชื่อโฟลเดอร์/Asset:", item.assetFolderName);
            item.authorOrOwner = EditorGUILayout.TextField("👤 ชื่อเจ้าของ / คนทำ:", item.authorOrOwner);
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("📋 คัดลอกเครดิตฟอร์แมต (- ชื่อ / - คนทำ) ไปใช้", GUILayout.Height(40)))
        {
            string formattedResult = "";
            foreach (var item in scannedAssets)
            {
                formattedResult += $"- {item.assetFolderName}\n- {item.authorOrOwner}\n\n";
            }
            GUIUtility.systemCopyBuffer = formattedResult;
            Debug.Log("<color=green><b>[SUCCESS!]</b> ก๊อบปี้เครดิตฟอร์แมตเป๊ะๆ เรียบร้อยแม่!</color>");
            EditorUtility.DisplayDialog("สำเร็จ!", "ก๊อบปี้ฟอร์แมตเครดิตเรียบร้อยแล้ว เอาไป Ctrl+V วางได้เลยจ่ะแม่!", "OK");
        }
    }
}