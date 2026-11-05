using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Dialogue text is kept in DialogueData; this component owns only presentation and input state.
public class DialogueController : MonoBehaviour
{
    [SerializeField, Min(.001f)] float secondsPerCharacter = .04f;
    enum Stage { Opening, Question, Choices, Response }
    Stage stage;
    GameObject player, panel, choiceGroup, prompt;
    PlayerController movement;
    PlayerAttack attack;
    Rigidbody2D body;
    Health health;
    bool movementWasEnabled, attackWasEnabled, typing;
    float elapsed;
    int visible;
    int[] textElements;
    string line;
    NpcInteractable current;
    Text speakerText, lineText, statusText;
    Font font;
    public bool IsOpen => current != null;

    public void Initialize(GameObject actor, Canvas canvas)
    {
        player = actor; movement = actor.GetComponent<PlayerController>(); attack = actor.GetComponent<PlayerAttack>();
        body = actor.GetComponent<Rigidbody2D>(); health = actor.GetComponent<Health>();
        font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Apple SD Gothic Neo", "Noto Sans CJK KR", "Arial Unicode MS", "Arial" }, 32);
        if (font == null) { Debug.LogError("No system font is available for dialogue."); return; }
        GameOverUIFactory.EnsureEventSystem();
        CreateUI(canvas.transform);
    }

    public void SetNearby(NpcInteractable npc)
    {
        if (prompt != null) prompt.SetActive(npc != null && !IsOpen);
    }

    public void Begin(NpcInteractable npc)
    {
        if (IsOpen || panel == null || npc == null || !npc.CanTalk || health.IsDead) return;
        current = npc;
        movementWasEnabled = movement != null && movement.enabled; attackWasEnabled = attack != null && attack.enabled;
        if (movement != null) movement.enabled = false;
        if (attack != null) attack.enabled = false;
        if (body != null) body.linearVelocity = Vector2.zero;
        prompt.SetActive(false); panel.SetActive(true);
        speakerText.text = npc.dialogue.speaker;
        StartLine(Stage.Opening, npc.dialogue.opening);
    }

    void Update()
    {
        if (!IsOpen) return;
        if (health == null || health.IsDead || !current.gameObject.activeInHierarchy) { End(); return; }
        bool space = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        if (space && typing) { Advance(); return; }
        if (typing)
        {
            elapsed += Time.unscaledDeltaTime;
            while (typing && elapsed >= Mathf.Max(.001f, secondsPerCharacter))
            {
                elapsed -= Mathf.Max(.001f, secondsPerCharacter);
                visible++;
                if (visible >= textElements.Length) FinishTyping(); else RenderLine();
            }
        }
        if (!space) return;
        Advance();
    }

    void Advance()
    {
        if (!IsOpen) return;
        if (typing) { FinishTyping(); return; }
        if (stage == Stage.Opening) StartLine(Stage.Question, current.dialogue.question);
        else if (stage == Stage.Response) End();
    }

    void StartLine(Stage next, string text)
    {
        stage = next; line = text ?? ""; textElements = StringInfo.ParseCombiningCharacters(line);
        visible = 0; elapsed = 0; typing = textElements.Length > 0;
        choiceGroup.SetActive(false); statusText.text = ""; RenderLine();
        if (!typing) FinishTyping();
    }

    // The transparent suffix keeps the complete text in the layout while revealing
    // Unicode text elements one by one, including Korean combining sequences.
    void RenderLine()
    {
        int end = visible >= textElements.Length ? line.Length : textElements[visible];
        lineText.text = Escape(line.Substring(0, end)) + "<color=#FFFFFF00>" + Escape(line.Substring(end)) + "</color>";
    }
    static string Escape(string text) => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    void FinishTyping()
    {
        typing = false; visible = textElements.Length; RenderLine();
        if (stage == Stage.Question) { stage = Stage.Choices; ShowChoices(); }
        else statusText.text = "[Space]";
    }

    void ShowChoices()
    {
        for (int i = choiceGroup.transform.childCount - 1; i >= 0; i--) Destroy(choiceGroup.transform.GetChild(i).gameObject);
        var choices = current.dialogue.choices;
        if (choices == null || choices.Length == 0) { Debug.LogError("Dialogue has no choices.", current); End(); return; }
        for (int i = 0; i < choices.Length; i++)
        {
            var item = new GameObject("Choice " + (i + 1), typeof(RectTransform), typeof(Image), typeof(MouseOnlyDialogueButton));
            item.transform.SetParent(choiceGroup.transform, false);
            var rect = item.GetComponent<RectTransform>(); rect.anchorMin = new Vector2(0, 1f - (i + 1f) / choices.Length); rect.anchorMax = new Vector2(1, 1f - i / (float)choices.Length); rect.offsetMin = new Vector2(0, 3); rect.offsetMax = new Vector2(0, -3);
            var image = item.GetComponent<Image>(); image.color = new Color(.19f, .23f, .30f, 1);
            var button = item.GetComponent<MouseOnlyDialogueButton>(); button.targetGraphic = image;
            var colors = button.colors; colors.normalColor = Color.white; colors.highlightedColor = new Color(1f, .72f, .84f); colors.pressedColor = new Color(.85f, .53f, .68f); button.colors = colors;
            var navigation = button.navigation; navigation.mode = Navigation.Mode.None; button.navigation = navigation;
            button.onClick.AddListener(SelectChoice);
            var label = MakeText("Label", item.transform, choices[i], 30, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }
        choiceGroup.SetActive(true);
    }
    void SelectChoice() { if (IsOpen && stage == Stage.Choices) StartLine(Stage.Response, current.dialogue.response); }

    void End()
    {
        if (current != null) current.Completed = true;
        current = null;
        if (panel != null) panel.SetActive(false);
        if (movement != null) movement.enabled = movementWasEnabled;
        if (attack != null) attack.enabled = attackWasEnabled;
    }
    void OnDestroy() { if (IsOpen) End(); if (font != null) Destroy(font); }

    void CreateUI(Transform canvas)
    {
        prompt = new GameObject("NPC Interaction Prompt", typeof(RectTransform)); prompt.transform.SetParent(canvas, false);
        var promptText = MakeText("Prompt", prompt.transform, "[E] 대화하기", 30, TextAnchor.MiddleCenter);
        Stretch(prompt.GetComponent<RectTransform>(), new Vector2(.3f, .15f), new Vector2(.7f, .21f), Vector2.zero, Vector2.zero);
        Stretch(promptText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); prompt.SetActive(false);
        panel = new GameObject("Dialogue Panel", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(canvas, false);
        Stretch(panel.GetComponent<RectTransform>(), new Vector2(.05f, .03f), new Vector2(.95f, .30f), Vector2.zero, Vector2.zero);
        panel.GetComponent<Image>().color = new Color(.04f, .06f, .10f, .96f);
        speakerText = MakeText("Speaker", panel.transform, "NPC", 34, TextAnchor.MiddleLeft);
        Stretch(speakerText.rectTransform, new Vector2(.04f, .72f), new Vector2(.60f, .95f), Vector2.zero, Vector2.zero);
        lineText = MakeText("Dialogue Text", panel.transform, "", 32, TextAnchor.UpperLeft);
        Stretch(lineText.rectTransform, new Vector2(.04f, .20f), new Vector2(.65f, .70f), Vector2.zero, Vector2.zero);
        statusText = MakeText("Progress", panel.transform, "", 22, TextAnchor.LowerRight);
        Stretch(statusText.rectTransform, new Vector2(.43f, .04f), new Vector2(.64f, .19f), Vector2.zero, Vector2.zero);
        choiceGroup = new GameObject("Choices", typeof(RectTransform)); choiceGroup.transform.SetParent(panel.transform, false);
        Stretch(choiceGroup.GetComponent<RectTransform>(), new Vector2(.69f, .10f), new Vector2(.96f, .90f), Vector2.zero, Vector2.zero);
        choiceGroup.SetActive(false); panel.SetActive(false);
    }
    Text MakeText(string name, Transform parent, string value, int size, TextAnchor alignment)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false);
        var text = obj.GetComponent<Text>(); text.font = font; text.fontSize = size; text.text = value; text.color = Color.white;
        text.alignment = alignment; text.raycastTarget = false; text.resizeTextForBestFit = true; text.resizeTextMinSize = 12; text.resizeTextMaxSize = size;
        return text;
    }
    static void Stretch(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    { rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax; }
}

// Do not let EventSystem submit/navigate buttons with keyboard or gamepad.
public class MouseOnlyDialogueButton : Button
{
    public override void OnSubmit(BaseEventData eventData) { }
    public override void OnPointerClick(PointerEventData eventData)
    { if (eventData.button == PointerEventData.InputButton.Left) base.OnPointerClick(eventData); }
}
