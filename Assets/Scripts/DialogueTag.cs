using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueTag", menuName = "Dialogue/Tag", order = 0)]
public class DialogueTag : ScriptableObject
{
    [Tooltip("Optional: when this tag's lines are exhausted, we fallback to the parent.")]
    public DialogueTag parentTag;

    [TextArea(2, 4)]
    public List<string> lines = new List<string>();

    // (Optional) Friendly identifier for debugging / quick lookup (not required to be unique).
    public string tagName;
}
