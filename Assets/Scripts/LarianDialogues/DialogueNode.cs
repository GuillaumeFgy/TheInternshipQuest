using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class DialogueNode
{
    [Tooltip("Unique identifier for this node")] public string guid = System.Guid.NewGuid().ToString();
    [Tooltip("Who's speaking this line (e.g., 'NPC', 'Player')")] public string speaker;
    [TextArea(2, 5)][Tooltip("The dialogue line to display")] public string line;
    [Tooltip("Choices available after this line")] public List<DialogueOption> options = new List<DialogueOption>();
}