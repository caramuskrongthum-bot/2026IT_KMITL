using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public GameObject PrefabRoom;
    Vector3 NewRoomLoadPos;
    public void LoadNextRoom()
    {
        GameObject r = Instantiate(PrefabRoom);
        NewRoomLoadPos.z += 10;
        r.transform.position = NewRoomLoadPos;
    }
}
