using UnityEngine;
using UnityEngine.SceneManagement;

// Prototype.unity and the editor's scene generator share this runtime map setup.
public static class PrototypeMap
{
    static Sprite sprite;
    static readonly Color floorColor = new Color(.20f, .25f, .28f);
    static readonly Color wallColor = new Color(.35f, .42f, .47f);
    static readonly Color doorColor = new Color(.1f, .75f, .95f);

    // AfterSceneLoad runs only for the first scene of a player session. Subscribe
    // before that load so Prototype is also initialized when loaded or restarted.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterSceneLoader()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Prototype" && scene == SceneManager.GetActiveScene()) Build();
    }

    static void Build()
    {
        if (SceneManager.GetActiveScene().name != "Prototype") return;
        var player = GameObject.Find("Player");
        var enemy = GameObject.Find("Enemy");
        var nav = Object.FindFirstObjectByType<GridPathfinder>();
        var camera = Object.FindFirstObjectByType<FollowCamera>();
        if (player == null || enemy == null || nav == null || camera == null) { Debug.LogError("Prototype map requires its original player, enemy, pathfinder and camera."); return; }
        sprite = player.GetComponent<SpriteRenderer>().sprite;
        if (sprite == null) { Debug.LogError("Prototype map requires the existing placeholder sprite."); return; }
        foreach (string old in new[] { "Floor Grid", "Wall North", "Wall South", "Wall East", "Wall West" })
        {
            var obj = GameObject.Find(old); if (obj != null) obj.SetActive(false);
        }
        var root = new GameObject("Prototype Map");
        var corridor = Area(root.transform, "Corridor", new Vector2(27, 23), new Vector2(38, 30));
        var lobby = Area(root.transform, "1 Lobby", new Vector2(52, 23), new Vector2(12, 30));
        var hall = Area(root.transform, "0 Hall", new Vector2(31, 23), new Vector2(30, 14));
        var cctv = Area(root.transform, "2 CCTV", new Vector2(32, 42), new Vector2(28, 8));
        var office3 = Area(root.transform, "3 Office", new Vector2(36.5f, 4), new Vector2(19, 8));
        var pantry = Area(root.transform, "4 Pantry", new Vector2(52, 4), new Vector2(12, 8));
        var office5 = Area(root.transform, "5 Office", new Vector2(17.5f, 4), new Vector2(19, 8));
        var president = Area(root.transform, "6 President", new Vector2(4, 11.5f), new Vector2(8, 23));
        var records = Area(root.transform, "7 Records", new Vector2(9, 34.5f), new Vector2(18, 23));
        var lift = Area(root.transform, "Elevator", new Vector2(47, 43), new Vector2(2, 6));
        var exit = Area(root.transform, "Emergency Exit", new Vector2(49, 43), new Vector2(2, 6));
        var bathroom = Area(root.transform, "Bathroom", new Vector2(54, 42), new Vector2(8, 8));
        var stairs = Area(root.transform, "Stairs", new Vector2(48, 39), new Vector2(4, 2));

        // The central 34 x 22 outdoor area is intentionally not floor. Its internal
        // hall and the 10 x 4 access passage are separate from the corridor.
        Floor(corridor, 8, 34, 46, 38); Floor(corridor, 8, 8, 46, 12); Floor(corridor, 8, 12, 12, 34);
        Wall(corridor, 8, 8, 46, 8); Wall(corridor, 8, 38, 46, 38); Wall(corridor, 8, 8, 8, 38); Wall(corridor, 46, 8, 46, 38);
        Wall(corridor, 12, 12, 46, 12); Wall(corridor, 12, 34, 46, 34); Wall(corridor, 12, 12, 12, 34);
        RectRoom(lobby, 46, 8, 58, 38);
        Floor(hall, 16, 16, 36, 30); Floor(hall, 36, 21, 46, 25);
        Wall(hall, 16, 16, 36, 16); Wall(hall, 16, 30, 36, 30); Wall(hall, 16, 16, 16, 30);
        Wall(hall, 36, 16, 36, 21); Wall(hall, 36, 25, 36, 30); Wall(hall, 36, 21, 46, 21); Wall(hall, 36, 25, 46, 25); Wall(hall, 46, 21, 46, 25);
        RectRoom(cctv, 18, 38, 46, 46); RectRoom(office3, 27, 0, 46, 8); RectRoom(pantry, 46, 0, 58, 8);
        RectRoom(office5, 8, 0, 27, 8); RectRoom(president, 0, 0, 8, 23);
        Floor(records, 0, 23, 8, 46); Floor(records, 8, 38, 18, 46);
        Wall(records, 0, 23, 8, 23); Wall(records, 0, 23, 0, 46); Wall(records, 0, 46, 18, 46); Wall(records, 18, 38, 18, 46); Wall(records, 8, 38, 18, 38); Wall(records, 8, 23, 8, 38);
        RectRoom(lift, 46, 40, 48, 46); RectRoom(exit, 48, 40, 50, 46); RectRoom(bathroom, 50, 38, 58, 46); RectRoom(stairs, 46, 38, 50, 40);

        Link(corridor, new Vector2(16, 38), records, new Vector2(16, 38), Vector2.up);
        Link(corridor, new Vector2(24, 38), cctv, new Vector2(24, 38), Vector2.up);
        Link(corridor, new Vector2(8, 25), records, new Vector2(8, 25), Vector2.left);
        Link(corridor, new Vector2(8, 19), president, new Vector2(8, 19), Vector2.left);
        Link(corridor, new Vector2(11, 8), office5, new Vector2(11, 8), Vector2.down);
        Link(corridor, new Vector2(25, 8), office5, new Vector2(25, 8), Vector2.down);
        Link(corridor, new Vector2(29, 8), office3, new Vector2(29, 8), Vector2.down);
        Link(corridor, new Vector2(44, 8), office3, new Vector2(44, 8), Vector2.down);
        Link(office5, new Vector2(27, 3), office3, new Vector2(27, 3), Vector2.right);
        Link(corridor, new Vector2(46, 10), lobby, new Vector2(46, 10), Vector2.right);
        Link(corridor, new Vector2(46, 36), lobby, new Vector2(46, 36), Vector2.right);
        Link(lobby, new Vector2(48, 8), pantry, new Vector2(48, 8), Vector2.down);
        Link(lobby, new Vector2(46, 23), hall, new Vector2(46, 23), Vector2.left);

        CreateNpc(lobby, new Vector2(52, 26));
        Box(lobby, "NPC Adjacent Object", new Vector2(53.35f, 26), new Vector2(.8f, .8f), new Color(.28f, .64f, .48f), true, 1);

        // Preserve the original crates, colliders and their relative arrangement.
        // The old room's origin maps to the middle of the new 20 x 14 hall.
        foreach (string name in new[] { "Crate Center", "Crate NW", "Crate SE" })
        {
            var crate = GameObject.Find(name);
            if (crate == null) { Debug.LogWarning("Original obstacle not found: " + name); continue; }
            crate.transform.SetParent(hall.transform, true);
            crate.transform.position += new Vector3(26, 23, 0);
        }
        enemy.transform.SetParent(hall.transform, true);
        // Keep the original enemy inside the initial view even in a narrow Game view.
        // The chosen point is clear of the restored central and northwest crates.
        // SyncTransforms pushes Transform changes to physics. Setting only the
        // Rigidbody2D positions here would let stale Transforms overwrite them.
        player.transform.position = new Vector3(23, 23, player.transform.position.z);
        enemy.transform.position = new Vector3(25, 25, enemy.transform.position.z);
        nav.SetBounds(new Vector2(16, 16), new Vector2(36, 30));
        Physics2D.SyncTransforms();
        var transitions = root.AddComponent<RoomTransition>();
        transitions.rooms = new[] { corridor, lobby, hall, cctv, office3, pantry, office5, president, records, lift, exit, bathroom, stairs };
        transitions.startingRoom = hall; transitions.player = player.transform; transitions.followCamera = camera;
        var hud = Object.FindFirstObjectByType<GameHUD>();
        if (hud != null && hud.GetComponent<Canvas>() != null)
        {
            var dialogue = root.AddComponent<DialogueController>();
            dialogue.Initialize(player, hud.GetComponent<Canvas>()); transitions.dialogue = dialogue;
        }
        else Debug.LogError("Dialogue requires the existing HUD canvas.");
    }

    static void CreateNpc(RoomArea room, Vector2 position)
    {
        var obj = new GameObject("Test NPC", typeof(MeshFilter), typeof(MeshRenderer), typeof(NpcInteractable));
        obj.transform.SetParent(room.transform); obj.transform.position = position;
        var npc = obj.GetComponent<NpcInteractable>(); npc.dialogue = Resources.Load<DialogueData>("TestNpcDialogue");
        if (npc.dialogue == null) Debug.LogError("TestNpcDialogue asset is missing.");
        var mesh = new Mesh { name = "NPC Triangle" };
        mesh.vertices = new[] { new Vector3(-.36f, -.3f), new Vector3(.36f, -.3f), new Vector3(0, .4f) };
        mesh.triangles = new[] { 0, 1, 2 }; mesh.RecalculateBounds(); obj.GetComponent<MeshFilter>().mesh = mesh; npc.ownedMesh = mesh;
        var material = new Material(Shader.Find("Sprites/Default")); material.color = new Color(1f, .35f, .65f); npc.ownedMaterial = material;
        var renderer = obj.GetComponent<MeshRenderer>(); renderer.material = material; renderer.sortingOrder = 2;
        var label = new GameObject("NPC Name", typeof(TextMesh)); label.transform.SetParent(obj.transform, false); label.transform.localPosition = new Vector3(0, .6f, 0);
        var text = label.GetComponent<TextMesh>(); text.text = "NPC"; text.anchor = TextAnchor.LowerCenter; text.alignment = TextAlignment.Center; text.fontSize = 40; text.characterSize = .035f; text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var textRenderer = label.GetComponent<MeshRenderer>(); textRenderer.material = text.font.material; textRenderer.sortingOrder = 5;
    }

    static RoomArea Area(Transform parent, string name, Vector2 center, Vector2 size)
    {
        var obj = new GameObject(name); obj.transform.SetParent(parent);
        var room = obj.AddComponent<RoomArea>(); room.roomName = name; room.cameraCenter = center; room.cameraSize = size; return room;
    }
    static void RectRoom(RoomArea room, float x1, float y1, float x2, float y2)
    {
        Floor(room, x1, y1, x2, y2); Wall(room, x1, y1, x2, y1); Wall(room, x1, y2, x2, y2); Wall(room, x1, y1, x1, y2); Wall(room, x2, y1, x2, y2);
    }
    static void Floor(RoomArea room, float x1, float y1, float x2, float y2) { Box(room, "Floor", new Vector2((x1+x2)/2, (y1+y2)/2), new Vector2(x2-x1, y2-y1), floorColor, false, 0); }
    static void Wall(RoomArea room, float x1, float y1, float x2, float y2) { Box(room, "Wall", new Vector2((x1+x2)/2, (y1+y2)/2), new Vector2(Mathf.Max(.2f, x2-x1), Mathf.Max(.2f, y2-y1)), wallColor, true, 1); }
    static void Box(RoomArea room, string name, Vector2 pos, Vector2 size, Color color, bool solid, int order)
    {
        var obj = new GameObject(name); obj.transform.SetParent(room.transform); obj.transform.position = pos; obj.transform.localScale = size;
        var renderer = obj.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = color; renderer.sortingOrder = order;
        if (solid) { obj.layer = 8; var collider = obj.AddComponent<BoxCollider2D>(); collider.size = Vector2.one; }
    }
    static void Link(RoomArea a, Vector2 from, RoomArea b, Vector2 to, Vector2 direction)
    {
        var first = Door(a, from, -direction); var second = Door(b, to, direction);
        first.destination = b; first.arrival = second.transform.GetChild(0); second.destination = a; second.arrival = first.transform.GetChild(0);
    }
    static RoomDoor Door(RoomArea room, Vector2 point, Vector2 inward)
    {
        var obj = new GameObject("Door"); obj.transform.SetParent(room.transform); obj.transform.position = point;
        var renderer = obj.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = doorColor; renderer.sortingOrder = 2;
        obj.transform.localScale = new Vector3(.8f, .8f, 1);
        var spawn = new GameObject("Arrival"); spawn.transform.SetParent(obj.transform, false);
        // A point just inside this room, away from the boundary collider.

        spawn.transform.position = point + inward * 1.25f;
        return obj.AddComponent<RoomDoor>();
    }
}
