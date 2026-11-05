using UnityEngine;

[CreateAssetMenu(menuName = "Game Prototype/Dialogue")]
public class DialogueData : ScriptableObject
{
    public string speaker = "NPC";
    [TextArea] public string opening;
    [TextArea] public string question;
    public string[] choices;
    [TextArea] public string response;
    public bool repeatable = true;
}
