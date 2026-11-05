using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// The generated hierarchy uses ordinary UI components so sprites, fonts and layout can be replaced in the Editor.
public static class GameOverUIFactory
{
    public static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    public static void Create(Transform canvas, out GameObject panel, out Button restartButton)
    {
        panel = new GameObject("Game Over Overlay", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        var background = panel.GetComponent<Image>();
        background.color = new Color(0, 0, 0, .75f);

        var title = CreateText("Game Over Title", panel.transform, "GAME OVER", 90);
        var titleRect = title.rectTransform;
        titleRect.anchoredPosition = new Vector2(0, 65); titleRect.sizeDelta = new Vector2(800, 140);

        var buttonObject = new GameObject("Restart Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(panel.transform, false);
        var buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = buttonRect.anchorMax = buttonRect.pivot = new Vector2(.5f, .5f);
        buttonRect.anchoredPosition = new Vector2(0, -90); buttonRect.sizeDelta = new Vector2(320, 84);
        var buttonImage = buttonObject.GetComponent<Image>(); buttonImage.color = new Color(.9f, .9f, .9f, 1);
        restartButton = buttonObject.GetComponent<Button>(); restartButton.targetGraphic = buttonImage;
        var label = CreateText("Restart Label", buttonObject.transform, "RESTART", 38);
        label.color = new Color(.1f, .1f, .1f, 1); label.raycastTarget = false;
        var labelRect = label.rectTransform; labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
        panel.SetActive(false);
    }

    static Text CreateText(string name, Transform parent, string content, int fontSize)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        var text = obj.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = content; text.fontSize = fontSize; text.fontStyle = FontStyle.Bold;
        text.color = Color.white; text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
        var rect = text.rectTransform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        return text;
    }
}
