using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Uses the production scene and production runtime initializer. Reflection is
// necessary because Unity asmdef test assemblies cannot reference Assembly-CSharp.
public class PrototypeEnemyPlayModeTests
{
    static Type RuntimeType(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    static Component RuntimeComponent(GameObject obj, string name) => obj.GetComponent(RuntimeType(name));

    [UnitySetUp]
    public IEnumerator LoadPrototype()
    {
        Time.timeScale = 1;
        yield return SceneManager.LoadSceneAsync("Prototype", LoadSceneMode.Single);
        yield return null;
        yield return new WaitForFixedUpdate();
    }

    static (GameObject enemy, GameObject player, Camera camera) SceneActors()
    {
        var scene = SceneManager.GetActiveScene();
        Assert.AreEqual("Prototype", scene.name);
        Assert.IsNotNull(GameObject.Find("Prototype Map"), "Production map initialization did not run for the loaded scene.");
        var actors = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Where(t => t.name == "Enemy").ToArray();
        Assert.AreEqual(1, actors.Length, "The original scene must contain exactly one enemy.");
        var enemy = actors[0].gameObject;
        var player = GameObject.Find("Player");
        var camera = Camera.main;
        Assert.IsNotNull(player);
        Assert.IsNotNull(camera);
        return (enemy, player, camera);
    }

    [UnityTest]
    public IEnumerator OriginalEnemyIsActiveInHallAndClearOfObstacles()
    {
        var (enemy, player, camera) = SceneActors();
        var room = enemy.GetComponentInParent(RuntimeType("RoomArea"));
        Assert.IsNotNull(room, "Enemy is not inside a room.");
        Assert.AreEqual("0 Hall", room.gameObject.name);
        Assert.IsTrue(enemy.activeSelf);
        Assert.IsTrue(enemy.activeInHierarchy);
        Assert.IsTrue(room.gameObject.activeInHierarchy);
        var ai = RuntimeComponent(enemy, "EnemyAI");
        Assert.IsNotNull(ai);
        Assert.IsTrue(((Behaviour)ai).enabled);
        Assert.AreSame(player.transform, ai.GetType().GetField("target").GetValue(ai));
        Assert.IsNotNull(ai.GetType().GetField("pathfinder").GetValue(ai));
        Assert.IsTrue(enemy.GetComponent<Rigidbody2D>().simulated);
        var collider = enemy.GetComponent<Collider2D>();
        Assert.IsNotNull(collider);
        Assert.IsTrue(collider.enabled && !collider.isTrigger);
        foreach (var other in Physics2D.OverlapCircleAll(enemy.transform.position, .34f, 1 << 8))
            Assert.IsFalse(collider.Distance(other).isOverlapped, "Enemy overlaps obstacle " + other.name);
        Assert.That(enemy.transform.position.x, Is.InRange(16.35f, 35.65f));
        Assert.That(enemy.transform.position.y, Is.InRange(16.35f, 29.65f));
        Debug.Log($"Enemy test: world={enemy.transform.position}, camera={camera.transform.position}, active={enemy.activeInHierarchy}, room={room.gameObject.name}");
        yield return null;
    }

    [UnityTest]
    public IEnumerator EnemyRendererIsWithinCameraAndVisibleInRenderedPixels()
    {
        var (enemy, _, camera) = SceneActors();
        var renderer = enemy.GetComponent<SpriteRenderer>();
        Assert.IsNotNull(renderer);
        Assert.IsTrue(enemy.activeInHierarchy && renderer.enabled && !renderer.forceRenderingOff);
        Assert.IsNotNull(renderer.sprite);
        Assert.IsNotNull(renderer.sharedMaterial);
        Assert.Greater(renderer.color.a, 0);
        Assert.AreNotEqual(0, camera.cullingMask & (1 << enemy.layer));
        Assert.IsTrue(camera.enabled && camera.gameObject.activeInHierarchy);
        Assert.IsTrue(GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), renderer.bounds), "Enemy bounds are outside the camera frustum.");
        var viewport = camera.WorldToViewportPoint(renderer.bounds.center);
        Assert.That(viewport.x, Is.InRange(0f, 1f)); Assert.That(viewport.y, Is.InRange(0f, 1f));
        Assert.That(viewport.z, Is.InRange(camera.nearClipPlane, camera.farClipPlane));
        Debug.Log($"Enemy render: world={enemy.transform.position}, camera={camera.transform.position}, viewport={viewport}, sprite={renderer.sprite.name}, sortingLayer={renderer.sortingLayerName}, order={renderer.sortingOrder}, mask={camera.cullingMask}, room={enemy.transform.parent?.name}, active={enemy.activeInHierarchy}");
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            Assert.Ignore("No graphics device; frustum checks passed but pixels cannot be checked.");
        var target = new RenderTexture(640, 480, 24);
        var image = new Texture2D(640, 480, TextureFormat.RGB24, false);
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 640, 480), 0, 0);
            image.Apply();
            string captureDirectory = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "TestResults");
            Directory.CreateDirectory(captureDirectory);
            string capturePath = Path.Combine(captureDirectory, "PrototypeEnemy.png");
            File.WriteAllBytes(capturePath, image.EncodeToPNG());
            Debug.Log("Enemy camera capture: " + capturePath);
            var point = camera.WorldToViewportPoint(renderer.bounds.center);
            var color = image.GetPixel(Mathf.Clamp((int)(point.x * 640), 0, 639), Mathf.Clamp((int)(point.y * 480), 0, 479));
            Assert.IsTrue(color.r > color.g * 1.3f && color.r > color.b * 1.3f && color.r > .1f, "Expected red enemy pixel, got " + color);
        }
        finally
        {
            camera.targetTexture = previousTarget; RenderTexture.active = previousActive;
            UnityEngine.Object.Destroy(target); UnityEngine.Object.Destroy(image);
        }
        yield return null;
    }

    [UnityTest]
    public IEnumerator EnemyDetectsAndMovesTowardPlayer()
    {
        var (enemy, player, _) = SceneActors();
        var ai = RuntimeComponent(enemy, "EnemyAI");
        Assert.IsNotNull(ai);
        Assert.AreSame(player.transform, ai.GetType().GetField("target").GetValue(ai));
        var initial = enemy.transform.position;
        var initialDistance = Vector2.Distance(initial, player.transform.position);
        yield return new WaitForSeconds(.6f);
        var moved = Vector2.Distance(initial, enemy.transform.position);
        Assert.Greater(moved, .05f, "Enemy did not move while the player was in the hall.");
        Assert.Less(Vector2.Distance(enemy.transform.position, player.transform.position), initialDistance, "Enemy did not approach player.");
    }
}
