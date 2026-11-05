using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrototypeEnemyDiagnostics : EditorWindow
{
    Vector2 scroll;
    string report = "Enter Play Mode in Prototype, then select Refresh.";

    [MenuItem("Tools/Game Prototype/Enemy Diagnostics")]
    static void Open() => GetWindow<PrototypeEnemyDiagnostics>("Enemy Diagnostics");

    void OnGUI()
    {
        EditorGUILayout.HelpBox("Run in Prototype Play Mode. Frustum checks are not proof of visible pixels; inspect the Game view as well.", MessageType.Info);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh")) report = Inspect();
        if (GUILayout.Button("Copy report")) { report = Inspect(); EditorGUIUtility.systemCopyBuffer = report; }
        if (GUILayout.Button("Log report")) { report = Inspect(); Debug.Log(report); }
        EditorGUILayout.EndHorizontal();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.SelectableLabel(report, EditorStyles.textArea, GUILayout.MinHeight(420), GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    static string Inspect()
    {
        var result = new StringBuilder();
        var scene = SceneManager.GetActiveScene();
        result.AppendLine($"Play Mode: {EditorApplication.isPlaying}; scene: {scene.name}; loaded: {scene.isLoaded}");
        if (!EditorApplication.isPlaying) result.AppendLine("NOT A RUNTIME CHECK: enter Play Mode first.");
        var camera = Camera.main;
        if (camera == null) result.AppendLine("Main camera: MISSING");
        else result.AppendLine($"Camera: {camera.name}; enabled={camera.enabled}; active={camera.gameObject.activeInHierarchy}; position={camera.transform.position}; cullingMask=0x{camera.cullingMask:X8}; clip={camera.nearClipPlane}..{camera.farClipPlane}; pixelRect={camera.pixelRect}");
        var rooms = Object.FindObjectsByType<RoomArea>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var room in rooms) if (room.gameObject.scene == scene && room.gameObject.activeInHierarchy)
            result.AppendLine($"Active room: {room.name}");
        var enemies = Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int count = 0;
        foreach (var ai in enemies)
        {
            if (ai.gameObject.scene != scene) continue;
            count++;
            var obj = ai.gameObject;
            var renderer = obj.GetComponent<SpriteRenderer>();
            var room = obj.GetComponentInParent<RoomArea>(true);
            result.AppendLine($"Enemy {count}: {obj.name}; activeSelf={obj.activeSelf}; activeInHierarchy={obj.activeInHierarchy}; parent={(obj.transform.parent == null ? "none" : obj.transform.parent.name)}; parentActive={(obj.transform.parent != null && obj.transform.parent.gameObject.activeInHierarchy)}; room={(room == null ? "none" : room.name)}");
            result.AppendLine($"  world={obj.transform.position}; local={obj.transform.localPosition}; scale={obj.transform.lossyScale}; layer={obj.layer}; AI enabled={ai.enabled}; state={ai.CurrentState}; target={(ai.target == null ? "none" : ai.target.name)}; pathfinder={(ai.pathfinder == null ? "none" : ai.pathfinder.name)}; path points={ai.Path.Count}");
            if (renderer == null) { result.AppendLine("  SpriteRenderer: MISSING"); continue; }
            result.AppendLine($"  renderer enabled={renderer.enabled}; forceOff={renderer.forceRenderingOff}; sprite={(renderer.sprite == null ? "none" : renderer.sprite.name)}; material={(renderer.sharedMaterial == null ? "none" : renderer.sharedMaterial.name)}; color={renderer.color}; sorting layer={renderer.sortingLayerName}; order={renderer.sortingOrder}; bounds={renderer.bounds}");
            if (camera != null)
            {
                var viewport = camera.WorldToViewportPoint(renderer.bounds.center);
                bool inFrustum = GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), renderer.bounds);
                bool layerIncluded = (camera.cullingMask & (1 << obj.layer)) != 0;
                result.AppendLine($"  viewport={viewport}; intersects camera frustum={inFrustum}; camera includes layer={layerIncluded}; renderer.isVisible={renderer.isVisible} (may include Scene view)");
            }
        }
        result.AppendLine($"EnemyAI count in active scene, including inactive: {count}");
        if (count == 0) result.AppendLine("No EnemyAI found. Check the Prototype scene and Console for map initialization errors.");
        return result.ToString();
    }
}
