using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class RoomManager : MonoBehaviour
{
    [Header("Room Prefabs")]
    [Tooltip("รายชื่อห้องปกติที่จะสุ่มสร้างขึ้นเรื่อยๆ")]
    public GameObject[] PrefabRoom;

    [Tooltip("Prefab ห้องสุดท้าย (LastRoom) ที่จะ Spawn ออกมาเมื่อผ่านครบเป้าหมาย")]
    public GameObject LastRoomPrefab;

    [Header("Settings & UI")]
    public int IndexRandom;
    public GameObject PrefabNextRoomUiPopUp;
    public int IndexRoom;
    public Transform Canva;
    private Vector3 NewRoomLoadPos;

    [Header("Events")]
    public UnityEvent UnityEvent;

    public void LoadNextRoom()
    {
        IndexRoom++;

        // 🎯 เช็คว่าผ่านห้องครบตามเป้าหมาย (Goal) จาก GameManager หรือยัง
        int targetGoal = (GameManager.Instance != null) ? GameManager.Instance.roomGoalCount : 50;

        // ถ้าเดินมาถึงห้องที่เป็นเป้าหมายสุดท้ายพอดี ให้ Spawn "LastRoom" แทนห้องปกติจ่ะแม่!
        if (IndexRoom >= targetGoal)
        {
            // ขยับตำแหน่ง Z ไปข้างหน้าสำหรับห้องสุดท้าย
            NewRoomLoadPos.z += 10;

            if (LastRoomPrefab != null)
            {
                GameObject lastRoomInstance = Instantiate(LastRoomPrefab, NewRoomLoadPos, Quaternion.identity);
                Debug.Log("<color=magenta>🏨 [FOG] ถึงห้องสุดท้ายแล้วแม่! Spawn LastRoom เรียบร้อย!</color>");
            }
            else
            {
                Debug.LogWarning("⚠️ ยังไม่ได้ใส่ Prefab ห้องสุดท้าย (LastRoomPrefab) ใน Inspector จ่ะแม่!");
            }

            // สั่งรัน UnityEvent เพิ่มเติม (เช่น ตัดเข้าคัทซีน, ล็อกประตู, หรือเปิดเพลงบอส)
            UnityEvent.Invoke();
            return;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPassedRoom();
        }

        // 1. สร้างห้องปกติทั่วไป
        GameObject r = Instantiate(PrefabRoom[IndexRandom = Random.Range(0, PrefabRoom.Length)]);
        NewRoomLoadPos.z += 10;
        r.transform.position = NewRoomLoadPos;

        if (RuntimeNavMeshManager.Instance != null)
        {
            RuntimeNavMeshManager.Instance.RebuildNavMesh();
        }

        if (PrefabNextRoomUiPopUp != null && Canva != null)
        {
            GameObject p = Instantiate(PrefabNextRoomUiPopUp, Canva, false);
            TextMeshProUGUI t = p.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            if ( t != null) // เช็คตัวแปรข้อความ UI
            {
                t.text = "Room " + IndexRoom;
            }
        }
    }
}