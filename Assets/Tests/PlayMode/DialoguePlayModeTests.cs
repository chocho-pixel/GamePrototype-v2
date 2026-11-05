using System;
using System.Collections;
using System.Linq;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class DialoguePlayModeTests
{
    GameObject player;
    Component dialogue;
    static Type TypeOf(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    static object Field(object obj, string name) => obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
    static Transform FindInScene(string name) => SceneManager.GetActiveScene().GetRootGameObjects()
        .SelectMany(o => o.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == name);

    [UnitySetUp]
    public IEnumerator LoadScene()
    {
        yield return SceneManager.LoadSceneAsync("Prototype", LoadSceneMode.Single);
        yield return null;
        player = GameObject.Find("Player");
        Assert.IsNotNull(player);
        dialogue = GameObject.Find("Prototype Map").GetComponent(TypeOf("DialogueController"));
        Assert.IsNotNull(dialogue);
    }

    IEnumerator KeyPress(Key key)
    {
        // Batch Editor has no focused Game view; exercise the same handlers called
        // by E/Space rather than claiming real OS keyboard input was delivered.
        if (key == Key.E)
        {
            var transition = GameObject.Find("Prototype Map").GetComponent(TypeOf("RoomTransition"));
            transition.GetType().GetMethod("Interact", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(transition, null);
        }
        else dialogue.GetType().GetMethod("Advance", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(dialogue, null);
        yield return null;
    }
    IEnumerator EnterLobby()
    {
        var hall = FindInScene("0 Hall");
        var door = hall.GetComponentsInChildren(TypeOf("RoomDoor"), true).Cast<Component>()
            .First(d => ((Component)d.GetType().GetField("destination").GetValue(d)).name == "1 Lobby");
        player.transform.position = door.transform.position + Vector3.left; Physics2D.SyncTransforms();
        yield return null;
        yield return KeyPress(Key.E);
        yield return new WaitForSeconds(.6f);
        Assert.IsTrue(FindInScene("1 Lobby").gameObject.activeInHierarchy, "Door did not enter lobby.");
    }
    IEnumerator BeginConversation()
    {
        var npc = FindInScene("Test NPC");
        player.transform.position = npc.position + Vector3.left; Physics2D.SyncTransforms();
        yield return null;
        Assert.IsTrue(((GameObject)Field(dialogue, "prompt")).activeSelf, "Nearby E prompt missing.");
        yield return KeyPress(Key.E);
        Assert.IsTrue((bool)dialogue.GetType().GetProperty("IsOpen").GetValue(dialogue));
    }

    [UnityTest]
    public IEnumerator LobbyNpcHasCorrectAppearanceAndExistingHallIsPreserved()
    {
        var enemy = FindInScene("Enemy"); Assert.IsNotNull(enemy);
        Assert.AreEqual("0 Hall", enemy.parent.name);
        foreach (string name in new[] { "Crate Center", "Crate NW", "Crate SE" }) Assert.AreEqual("0 Hall", FindInScene(name).parent.name);
        yield return EnterLobby();
        var npc = FindInScene("Test NPC"); Assert.AreEqual("1 Lobby", npc.parent.name);
        Assert.IsTrue(npc.gameObject.activeInHierarchy);
        Assert.AreEqual("NPC", npc.Find("NPC Name").GetComponent<TextMesh>().text);
        var vertices = npc.GetComponent<MeshFilter>().sharedMesh.vertices;
        Assert.Greater(vertices[2].y, vertices[0].y); Assert.Greater(vertices[2].y, vertices[1].y);
        var color = npc.GetComponent<MeshRenderer>().material.color; Assert.Greater(color.r, color.g);
        var box = FindInScene("NPC Adjacent Object"); Assert.AreEqual("1 Lobby", box.parent.name);
        Assert.IsFalse(box.GetComponent<Collider2D>().isTrigger); Assert.AreEqual(8, box.gameObject.layer);
        Assert.IsFalse(enemy.gameObject.activeInHierarchy);
        if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            var camera = Camera.main; var oldPosition = camera.transform.position;
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            var target = new RenderTexture(640, 480, 24); var image = new Texture2D(640, 480, TextureFormat.RGB24, false);
            try
            {
                camera.transform.position = new Vector3(npc.position.x, npc.position.y, oldPosition.z);
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 640, 480), 0, 0); image.Apply();
                var point = camera.WorldToViewportPoint(npc.position);
                var pixel = image.GetPixel((int)(point.x * 640), (int)(point.y * 480));
                Assert.IsTrue(pixel.r > pixel.g * 1.2f && pixel.b > pixel.g * 1.1f, "NPC triangle did not render pink: " + pixel);
                var directory = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "TestResults"); Directory.CreateDirectory(directory);
                File.WriteAllBytes(Path.Combine(directory, "NpcLobby.png"), image.EncodeToPNG());
            }
            finally
            {
                camera.transform.position = oldPosition; camera.targetTexture = oldTarget; RenderTexture.active = oldActive;
                UnityEngine.Object.Destroy(target); UnityEngine.Object.Destroy(image);
            }
        }
    }

    [UnityTest]
    public IEnumerator ClosestDoorOrNpcReceivesOnlyOneInteraction()
    {
        yield return EnterLobby();
        var npc = FindInScene("Test NPC");
        npc.position = new Vector3(47.5f, 23, 0);
        yield return null;
        var transition = GameObject.Find("Prototype Map").GetComponent(TypeOf("RoomTransition"));
        Assert.IsNotNull(Field(transition, "nearbyNpc")); Assert.IsNull(Field(transition, "nearby"));
        yield return KeyPress(Key.E);
        Assert.IsTrue((bool)dialogue.GetType().GetProperty("IsOpen").GetValue(dialogue));
        Assert.IsFalse((bool)Field(transition, "transitioning"));
        dialogue.GetType().GetMethod("End", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(dialogue, null);
        npc.position = new Vector3(48.65f, 23, 0);
        yield return null;
        Assert.IsNull(Field(transition, "nearbyNpc")); Assert.IsNotNull(Field(transition, "nearby"));
        yield return KeyPress(Key.E); yield return new WaitForSeconds(.6f);
        Assert.IsTrue(FindInScene("0 Hall").gameObject.activeInHierarchy);
        Assert.IsFalse((bool)dialogue.GetType().GetProperty("IsOpen").GetValue(dialogue));
    }

    [UnityTest]
    public IEnumerator DialogueTypingMouseChoicesAndRepeatRestoreInput()
    {
        yield return EnterLobby();
        for (int index = 0; index < 3; index++)
        {
            yield return BeginConversation();
            var text = (Text)Field(dialogue, "lineText");
            foreach (char ch in "오늘의 메뉴를 추천해 주세요! 돈까스 제육 규동 그렇군요 저는 라면을 먹겠습니다!")
                if (!char.IsWhiteSpace(ch)) Assert.IsTrue(text.font.HasCharacter(ch), "Dialogue font lacks character: " + ch);
            var controller = (Behaviour)player.GetComponent(TypeOf("PlayerController"));
            var attack = (Behaviour)player.GetComponent(TypeOf("PlayerAttack"));
            Assert.IsFalse(controller.enabled); Assert.IsFalse(attack.enabled);
            Assert.AreEqual("Hello World!", Field(dialogue, "line"));
            Assert.IsTrue((bool)Field(dialogue, "typing"));
            if (index == 0)
            {
                int before = (int)Field(dialogue, "visible");
                yield return new WaitForSeconds(.10f);
                int after = (int)Field(dialogue, "visible");
                Assert.Greater(after, before, "Characters did not appear over time.");
                Assert.Less(after, "Hello World!".Length, "Typing completed immediately.");
                yield return KeyPress(Key.E);
                Assert.GreaterOrEqual((int)Field(dialogue, "visible"), after, "E restarted an active dialogue.");
            }
            yield return KeyPress(Key.Space);
            Assert.AreEqual("Hello World!", Field(dialogue, "line")); Assert.IsFalse((bool)Field(dialogue, "typing"));
            yield return KeyPress(Key.Space);
            Assert.AreEqual("오늘의 메뉴를 추천해 주세요!", Field(dialogue, "line"));
            if (index == 0) yield return new WaitForSeconds(1.2f); else yield return KeyPress(Key.Space);
            var group = (GameObject)Field(dialogue, "choiceGroup"); Assert.IsTrue(group.activeSelf, "Choices should appear as soon as typing finishes.");
            Assert.AreEqual("오늘의 메뉴를 추천해 주세요!", Field(dialogue, "line"));
            Assert.AreEqual(3, group.transform.childCount);
            string[] expected = { "돈까스", "제육", "규동" };
            for (int i = 0; i < 3; i++) Assert.AreEqual(expected[i], group.transform.GetChild(i).GetComponentInChildren<Text>().text);
            yield return KeyPress(Key.Space); Assert.IsTrue(group.activeSelf, "Space must not select a choice.");
            var button = group.transform.GetChild(index).gameObject;
            var click = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(button, click, ExecuteEvents.pointerClickHandler); yield return null;
            Assert.IsFalse(group.activeSelf, "Mouse click did not select the choice.");
            Assert.AreEqual("그렇군요 저는 라면을 먹겠습니다!", Field(dialogue, "line"));
            yield return KeyPress(Key.Space); yield return KeyPress(Key.Space);
            Assert.IsFalse((bool)dialogue.GetType().GetProperty("IsOpen").GetValue(dialogue));
            Assert.IsTrue(controller.enabled); Assert.IsTrue(attack.enabled);
        }
    }
}
