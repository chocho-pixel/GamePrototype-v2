using UnityEngine;

// The room hierarchy contains its floors, walls, doors and any room-local actors.
public class RoomArea : MonoBehaviour
{
    public string roomName;
    public Vector2 cameraCenter;
    public Vector2 cameraSize = new Vector2(18, 10);
}
