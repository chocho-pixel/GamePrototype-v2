using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    public RoomArea destination;
    public Transform arrival;
    [Min(.1f)] public float interactionRadius = 1.5f;
}
