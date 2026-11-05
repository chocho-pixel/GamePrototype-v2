using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Lives outside room roots so the player, HUD and transition survive room changes.
public class RoomTransition : MonoBehaviour
{
    public RoomArea[] rooms;
    public RoomArea startingRoom;
    public Transform player;
    public FollowCamera followCamera;
    [Min(.01f)] public float fadeDuration = .2f;
    RoomArea currentRoom;
    RoomDoor nearby;
    bool transitioning;
    float fade;

    void Start()
    {
        if (rooms == null || startingRoom == null || player == null || followCamera == null ||
            System.Array.IndexOf(rooms, startingRoom) < 0)
        {
            Debug.LogError("Room transition is missing its starting room, player or camera.", this);
            enabled = false;
            return;
        }
        currentRoom = startingRoom;
        foreach (var room in rooms) if (room != null) room.gameObject.SetActive(room == currentRoom);
        SetCamera();
    }

    void Update()
    {
        nearby = null;
        if (transitioning || currentRoom == null || player == null) return;
        var health = player.GetComponent<Health>();
        if (health != null && health.IsDead) return;
        float closest = float.PositiveInfinity;
        foreach (var door in currentRoom.GetComponentsInChildren<RoomDoor>())
        {
            if (door.destination == null || door.arrival == null ||
                rooms == null || System.Array.IndexOf(rooms, door.destination) < 0 ||
                !door.arrival.IsChildOf(door.destination.transform)) continue;
            float distance = Vector2.Distance(player.position, door.transform.position);
            if (distance <= door.interactionRadius && distance < closest) { closest = distance; nearby = door; }
        }
        if (nearby != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            StartCoroutine(ChangeRoom(nearby));
    }

    IEnumerator ChangeRoom(RoomDoor door)
    {
        transitioning = true;
        var movement = player.GetComponent<PlayerController>();
        var attack = player.GetComponent<PlayerAttack>();
        var body = player.GetComponent<Rigidbody2D>();
        bool movementWasEnabled = movement != null && movement.enabled;
        bool attackWasEnabled = attack != null && attack.enabled;
        if (movement != null) movement.enabled = false;
        if (attack != null) attack.enabled = false;
        if (body != null) body.linearVelocity = Vector2.zero;
        yield return Fade(1);
        Vector3 arrival = door.arrival.position;
        var destination = door.destination;
        currentRoom.gameObject.SetActive(false);
        currentRoom = destination;
        currentRoom.gameObject.SetActive(true);
        if (body != null) { body.position = arrival; body.linearVelocity = Vector2.zero; }
        else player.position = arrival;
        Physics2D.SyncTransforms();
        SetCamera();
        yield return Fade(0);
        if (movement != null) movement.enabled = movementWasEnabled;
        if (attack != null) attack.enabled = attackWasEnabled;
        transitioning = false;
    }

    IEnumerator Fade(float target)
    {
        float start = fade, elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fade = Mathf.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }
        fade = target;
    }

    void SetCamera()
    {
        followCamera.SnapToTarget();
    }

    void OnGUI()
    {
        if (!transitioning && currentRoom != null)
            GUI.Box(new Rect(Screen.width - 200, 20, 180, 30), currentRoom.roomName);
        if (!transitioning && nearby != null)
            GUI.Box(new Rect(Screen.width * .5f - 75, Screen.height - 100, 150, 35), "[E] Open door");
        if (fade > 0)
        {
            var old = GUI.color;
            GUI.color = new Color(0, 0, 0, fade);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = old;
        }
    }
}
