using System.Collections.Generic; // อย่าลืมใส่บรรทัดนี้นะจ๊ะแม่
using TMPro;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public GameObject[] PrefabRoom;
    public int IndexRandom;
    public GameObject PrefabNextRoomUiPopUp;
    public int IndexRoom;
    public Transform Canva;
    Vector3 NewRoomLoadPos;

    [Header("Room Cleanup Settings")]
    [Tooltip("จำนวนห้องเก่าด้านหลังที่ต้องการเก็บไว้ (แนะนำ 2 หรือ 3 ห้องกำลังสวยจ่ะแม่)")]
    public int maxRoomsToKeep = 2;

    // เก็บรายการห้องทั้งหมดที่ถูกสร้างขึ้นมา
    private List<GameObject> spawnedRooms = new List<GameObject>();

    public void LoadNextRoom()
    {
        IndexRoom++;

        // 🎯 เช็คว่าผ่านห้องครบตามเป้าหมาย (Goal) จาก GameManager หรือยัง
        int targetGoal = (GameManager.Instance != null) ? GameManager.Instance.roomGoalCount : 50;

        if (IndexRoom >= targetGoal)
        {
            Debug.Log($"<color=magenta><b>[GAME CLEAR!]</b> แม่ขา! ผ่านครบ {targetGoal} ห้องตามเป้าหมายแล้ว เริ่ดมากกก! 👑✨</color>");
            return;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPassedRoom();
        }

        // 1. สร้างห้องใหม่
        GameObject r = Instantiate(PrefabRoom[IndexRandom = Random.Range(0, PrefabRoom.Length)]);
        NewRoomLoadPos.z += 10;
        r.transform.position = NewRoomLoadPos;

        // บันทึกห้องใหม่ลงในลิสต์รายชื่อ
        spawnedRooms.Add(r);

        // 2. 💅 ระบบทำลายห้องเก่าทิ้งแบบตัวแม่ (เก่าไปใหม่มา ไม่เก็บขยะไว้รกบ้าน)
        // ถ้าจำนวนห้องในลิสต์ มากกว่าจำนวนห้องที่เราอยากเก็บสำรองไว้
        if (spawnedRooms.Count > maxRoomsToKeep)
        {
            // ดึงห้องที่เก่าที่สุด (ตัวแรกสุดในลิสต์) ออกมา
            GameObject oldRoom = spawnedRooms[0];

            // เอาออกจากลิสต์
            spawnedRooms.RemoveAt(0);

            // สั่งทำลาย (Destroy) ทิ้งจากเกมทันที เริ่ดๆ ประหยัดเมม!
            if (oldRoom != null)
            {
                Destroy(oldRoom);
                Debug.Log($"<color=orange>🧹 ทำลายห้องเก่าทิ้งเรียบร้อย เพื่อความลื่นไหลของเกมจ่ะแม่!</color>");
            }
        }

        if (RuntimeNavMeshManager.Instance != null)
        {
            RuntimeNavMeshManager.Instance.RebuildNavMesh();
        }

        if (PrefabNextRoomUiPopUp != null && Canva != null)
        {
            GameObject p = Instantiate(PrefabNextRoomUiPopUp, Canva, false);
            TextMeshProUGUI t = p.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            if (t != null)
            {
                t.text = "Room " + IndexRoom;
            }
        }
    }
}