using System;
using UnityEngine;
using UnityEngine.SceneManagement;



public class DialogueRunner : MonoBehaviour
{
    [Header("Data")] public DialogueGraph graph;
    [Header("Optional Services")] public DiceRollWindow diceWindow; // Assign your existing window here


    public event Action<DialogueNode> OnNodeEntered; // Subscribe from UI
    public DialogueNode CurrentNode { get; private set; }
    public bool IsRunning { get; private set; }


    public void Begin(DialogueGraph newGraph = null)
    {
        if (newGraph != null) graph = newGraph;
        if (graph == null)
        {
            Debug.LogError("DialogueRunner.Begin called with no graph set.");
            return;
        }


        var start = graph.GetStartNode();
        if (start == null)
        {
            Debug.LogError("DialogueRunner: Graph has no valid start node.");
            return;
        }


        IsRunning = true;
        EnterNode(start);
    }
    public void ChooseOption(int optionIndex)
    {
        if (!IsRunning || CurrentNode == null) return;
        if (CurrentNode.options == null || optionIndex < 0 || optionIndex >= CurrentNode.options.Count)
        {
            Debug.LogWarning("DialogueRunner.ChooseOption: invalid option index.");
            return;
        }


        var option = CurrentNode.options[optionIndex];

        if (option.requiresDiceCheck)
        {
            if (diceWindow == null)
            {
                Debug.LogWarning("DialogueRunner: requiresDiceCheck but no diceWindow assigned. Ending dialogue.");
                End();
                return;
            }

            diceWindow.Open(option.dicePrompt, option.target, result =>
            {
                bool success = result >= option.target;

                var nextGuid = success ? option.nextOnSuccessGuid : option.nextOnFailGuid;
                ContinueTo(nextGuid);
            });
            return;
        }

        if (option.loadCreditsScene)
        {
            LoadCredits();
            return;
        }

        ContinueTo(option.nextNodeGuid);
    }


    private void ContinueTo(string nextGuid)
    {
        if (string.IsNullOrEmpty(nextGuid))
        {
            End();
            return;
        }


        var next = graph.GetNode(nextGuid);
        if (next == null)
        {
            Debug.LogWarning($"DialogueRunner: next node {nextGuid} not found. Ending dialogue.");
            End();
            return;
        }


        EnterNode(next);
    }


    public void End()
    {
        IsRunning = false;
        CurrentNode = null;
        OnNodeEntered?.Invoke(null); // Signal end to UI
    }


    private void EnterNode(DialogueNode node)
    {
        CurrentNode = node;
        OnNodeEntered?.Invoke(node);
    }

    private void LoadCredits()
    {
        SceneManager.LoadScene("CreditsScene");
    }
}