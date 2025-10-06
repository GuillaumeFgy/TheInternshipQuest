using System;
using UnityEngine;


[Serializable]
public class DialogueOption
{
    [Tooltip("Text shown on the choice button")] public string text;


    [Header("Direct Jump (default)")]
    [Tooltip("If no dice check is required, jump directly to this node")] public string nextNodeGuid;

    [Header("Alternate Action")]
    [Tooltip("If true, clicking this option loads the CreditsScene instead of jumping to a node.")]
    public bool loadCreditsScene = false;


    [Header("Dice Check (optional)")]
    [Tooltip("If true, this option triggers a dice roll before proceeding")] public bool requiresDiceCheck = false;


    [Tooltip("Shown in your dice window (e.g., 'Strength Check DC 12')")]
    public string dicePrompt;


    [Tooltip("Target number you must meet or exceed to succeed (>= target)")]
    public int target = 10;


    [Tooltip("Next node if the roll succeeds (>= target)")]
    public string nextOnSuccessGuid;


    [Tooltip("Next node if the roll fails (< target)")]
    public string nextOnFailGuid;

}