using UnityEngine;

public class NpcInteractable : MonoBehaviour
{
    public DialogueData dialogue;
    [Min(.1f)] public float interactionRadius = 1.5f;
    public bool Completed { get; set; }
    public bool CanTalk => dialogue != null && (dialogue.repeatable || !Completed);
    public Mesh ownedMesh;
    public Material ownedMaterial;
    void OnDestroy()
    {
        if (ownedMesh != null) Destroy(ownedMesh);
        if (ownedMaterial != null) Destroy(ownedMaterial);
    }
}
