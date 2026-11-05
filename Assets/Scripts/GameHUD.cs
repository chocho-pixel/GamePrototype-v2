using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    public Health player; public Health enemy; public Image[] hearts;
    [Tooltip("Replace this panel with a designed hierarchy containing a Restart Button.")]
    public GameObject gameOverPanel;
    [Tooltip("Button that reloads the current scene.")]
    public Button restartButton;

    void Start()
    {
        // Older scenes receive the same UI at runtime; newly generated scenes serialize it.
        if (gameOverPanel == null)
            GameOverUIFactory.Create(transform, out gameOverPanel, out restartButton);
        GameOverUIFactory.EnsureEventSystem();
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        RefreshGameOver();
        if (player != null) player.Changed += RefreshGameOver;
    }
    void OnDestroy()
    {
        if (player != null) player.Changed -= RefreshGameOver;
        if (restartButton != null) restartButton.onClick.RemoveListener(Restart);
    }
    void RefreshGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(player != null && player.IsDead);
    }
    public void Restart() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    void Update()
    {
        if(Keyboard.current!=null && Keyboard.current.rKey.wasPressedThisFrame) { Restart(); return; }
        if(player!=null && hearts!=null) for(int i=0;i<hearts.Length;i++) hearts[i].color=i<player.Current ? new Color(1,.24f,.3f) : new Color(.23f,.25f,.3f);
    }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    bool show;
    void OnGUI(){ if(Keyboard.current!=null && Keyboard.current.f3Key.wasPressedThisFrame) show=!show; if(!show || player==null) return; GUI.Label(new Rect(10,65,600,25), $"Player HP {player.Current} | Dash invulnerable {player.DashInvulnerable} | Enemy HP {(enemy != null ? enemy.Current : 0)} | AI {(enemy != null ? enemy.GetComponent<EnemyAI>().CurrentState.ToString() : "-")}"); }
#endif
}
