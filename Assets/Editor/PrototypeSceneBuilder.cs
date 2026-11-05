using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections.Generic;

public static class PrototypeSceneBuilder
{
    const string ScenePath="Assets/Scenes/Prototype.unity";
    [MenuItem("Tools/Game Prototype/Create Test Scene")]
    public static void Build()
    {
        if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EnsureLayer();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var white=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        // A generated texture is a replaceable placeholder for all world sprites and tiles.
        Texture2D texture=new Texture2D(16,16); for(int y=0;y<16;y++) for(int x=0;x<16;x++) texture.SetPixel(x,y,Color.white); texture.Apply();
        System.IO.Directory.CreateDirectory("Assets/Settings"); System.IO.File.WriteAllBytes("Assets/Settings/Placeholder.png",texture.EncodeToPNG()); Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset("Assets/Settings/Placeholder.png",ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Settings/Placeholder.png"); importer.textureType=TextureImporterType.Sprite; importer.spritePixelsPerUnit=16; importer.filterMode=FilterMode.Point; importer.SaveAndReimport();
        Sprite square=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Settings/Placeholder.png");
        Tile tile=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Settings/Floor.asset"); if(tile==null){tile=ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(tile,"Assets/Settings/Floor.asset");} tile.sprite=square; tile.color=new Color(.20f,.25f,.28f); EditorUtility.SetDirty(tile);
        var grid=new GameObject("Floor Grid",typeof(Grid)); var floor=new GameObject("Floor Tilemap",typeof(Tilemap),typeof(TilemapRenderer)); floor.transform.SetParent(grid.transform); var map=floor.GetComponent<Tilemap>();
        for(int y=-5;y<5;y++) for(int x=-9;x<9;x++) map.SetTile(new Vector3Int(x,y,0),tile);
        Box("Wall North",new Vector2(0,5.25f),new Vector2(19,.5f),square,new Color(.35f,.42f,.47f)); Box("Wall South",new Vector2(0,-5.25f),new Vector2(19,.5f),square,new Color(.35f,.42f,.47f));
        Box("Wall West",new Vector2(-9.25f,0),new Vector2(.5f,10),square,new Color(.35f,.42f,.47f)); Box("Wall East",new Vector2(9.25f,0),new Vector2(.5f,10),square,new Color(.35f,.42f,.47f));
        Box("Crate Center",new Vector2(0,0),new Vector2(1.4f,2.2f),square,new Color(.58f,.39f,.2f)); Box("Crate NW",new Vector2(-3,2.5f),Vector2.one,square,new Color(.58f,.39f,.2f)); Box("Crate SE",new Vector2(3,-2.5f),Vector2.one,square,new Color(.58f,.39f,.2f));
        var nav=new GameObject("Pathfinder").AddComponent<GridPathfinder>(); var navData=new SerializedObject(nav); navData.FindProperty("obstacles").intValue=1<<8; navData.ApplyModifiedPropertiesWithoutUndo();
        var player=Actor("Player",new Vector2(-5,0),square); player.AddComponent<Health>(); player.AddComponent<PlayerController>(); player.AddComponent<PlayerAttack>(); player.AddComponent<CombatVisual>();
        var foe=Actor("Enemy",new Vector2(5,0),square); var hp=foe.AddComponent<Health>(); var data=new SerializedObject(hp);data.FindProperty("maxHealth").intValue=2;data.FindProperty("hitInvulnerability").floatValue=0;data.ApplyModifiedPropertiesWithoutUndo();
        var ai=foe.AddComponent<EnemyAI>(); ai.target=player.transform; ai.pathfinder=nav; foe.AddComponent<CombatVisual>();
        var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(FollowCamera)); camera.tag="MainCamera"; camera.transform.position=new Vector3(-5,0,-10); var cam=camera.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=4;cam.backgroundColor=new Color(.10f,.12f,.14f);cam.clearFlags=CameraClearFlags.SolidColor;camera.GetComponent<FollowCamera>().target=player.transform;
        var canvas=new GameObject("HUD",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(GameHUD)); canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay; var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);
        var hud=canvas.GetComponent<GameHUD>();hud.player=player.GetComponent<Health>();hud.enemy=hp;hud.hearts=new Image[5];
        Sprite heart=CreateHeart(); for(int i=0;i<5;i++){var o=new GameObject("Heart "+(i+1),typeof(RectTransform),typeof(Image));o.transform.SetParent(canvas.transform,false);var rect=o.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(30+i*54,-30);rect.sizeDelta=new Vector2(48,48);hud.hearts[i]=o.GetComponent<Image>();hud.hearts[i].sprite=heart;}
        GameOverUIFactory.Create(canvas.transform, out hud.gameOverPanel, out hud.restartButton); GameOverUIFactory.EnsureEventSystem();
        EditorSceneManager.SaveScene(scene,ScenePath); var scenes=new List<EditorBuildSettingsScene>{new EditorBuildSettingsScene(ScenePath,true)}; foreach(var s in EditorBuildSettings.scenes) if(s.path!=ScenePath) scenes.Add(s);EditorBuildSettings.scenes=scenes.ToArray(); AssetDatabase.SaveAssets();
        Debug.Log("Prototype scene generated: "+ScenePath);
    }
    [MenuItem("Tools/Game Prototype/Add Game Over UI to Current Scene")]
    public static void AddGameOverUI()
    {
        var hud = Object.FindFirstObjectByType<GameHUD>();
        if (hud == null || hud.GetComponent<Canvas>() == null)
        {
            Debug.LogError("Open a scene containing a GameHUD on a Canvas first.");
            return;
        }
        if (hud.gameOverPanel != null)
        {
            Debug.LogWarning("This HUD already has a Game Over panel.");
            return;
        }
        GameOverUIFactory.Create(hud.transform, out hud.gameOverPanel, out hud.restartButton);
        GameOverUIFactory.EnsureEventSystem();
        EditorUtility.SetDirty(hud);
        EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);
    }
    static Sprite CreateHeart(){
        var tex=new Texture2D(32,32);for(int y=0;y<32;y++)for(int x=0;x<32;x++){float xx=(x-15.5f)/15f, yy=(y-14f)/15f; float a=xx*xx+yy*yy-1; bool inside=a*a*a-xx*xx*yy*yy*yy<=0;tex.SetPixel(x,y,inside?Color.white:Color.clear);}tex.Apply();System.IO.File.WriteAllBytes("Assets/Settings/Heart.png",tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset("Assets/Settings/Heart.png",ImportAssetOptions.ForceSynchronousImport);var imp=(TextureImporter)AssetImporter.GetAtPath("Assets/Settings/Heart.png");imp.textureType=TextureImporterType.Sprite;imp.alphaIsTransparency=true;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Settings/Heart.png");}
    static GameObject Actor(string name,Vector2 pos,Sprite sprite){var o=new GameObject(name,typeof(SpriteRenderer),typeof(Rigidbody2D),typeof(CircleCollider2D));o.transform.position=pos;o.transform.localScale=Vector3.one*.7f;var sr=o.GetComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingOrder=2;var body=o.GetComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Dynamic;body.simulated=true;body.gravityScale=0;body.freezeRotation=true;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;var collider=o.GetComponent<CircleCollider2D>();collider.radius=.5f;collider.isTrigger=false;return o;}
    static void Box(string name,Vector2 pos,Vector2 size,Sprite sprite,Color color){var o=new GameObject(name,typeof(SpriteRenderer),typeof(BoxCollider2D));o.layer=8;o.transform.position=pos;o.transform.localScale=size;var sr=o.GetComponent<SpriteRenderer>();sr.sprite=sprite;sr.color=color;sr.sortingOrder=1;var collider=o.GetComponent<BoxCollider2D>();collider.size=Vector2.one;collider.offset=Vector2.zero;collider.isTrigger=false;}
    static void EnsureLayer(){var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var layers=tags.FindProperty("layers");var layer=layers.GetArrayElementAtIndex(8);if(!string.IsNullOrEmpty(layer.stringValue) && layer.stringValue!="Obstacles") throw new System.Exception("Layer 8 is already in use: "+layer.stringValue);layer.stringValue="Obstacles";tags.ApplyModifiedPropertiesWithoutUndo();}
}
