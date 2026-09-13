using TMPro;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public GameObject PrefabRoom;
    public GameObject PrefabNextRoomUiPopUp;
    public int IndexRoom;
    public Transform Canva;

    Vector3 NewRoomLoadPos;

    public void LoadNextRoom()
    {
        IndexRoom++;
        GameObject r = Instantiate(PrefabRoom);
        NewRoomLoadPos.z += 10;
        r.transform.position = NewRoomLoadPos;
        if (RuntimeNavMeshManager.Instance != null)
        {
            RuntimeNavMeshManager.Instance.RebuildNavMesh();
        }
        GameObject p = Instantiate(PrefabNextRoomUiPopUp,Canva,false);
        TextMeshProUGUI t = p.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        t.text = "Room " + IndexRoom;
    }
}