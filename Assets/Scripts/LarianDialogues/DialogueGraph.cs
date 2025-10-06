using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(menuName = "Dialogue/Dialogue Graph", fileName = "NewDialogueGraph")]
public class DialogueGraph : ScriptableObject
{
    [Tooltip("All nodes in this dialogue")] public List<DialogueNode> nodes = new List<DialogueNode>();
    [Tooltip("GUID of the first node that starts the conversation")] public string startNodeGuid;


    public DialogueNode GetStartNode()
    {
        if (string.IsNullOrEmpty(startNodeGuid)) return nodes.FirstOrDefault();
        return nodes.FirstOrDefault(n => n.guid == startNodeGuid);
    }


    public DialogueNode GetNode(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return null;
        return nodes.FirstOrDefault(n => n.guid == guid);
    }


    public IEnumerable<string> Validate()
    {
        var problems = new List<string>();
        if (nodes == null || nodes.Count == 0) problems.Add("Graph has no nodes.");
        if (GetStartNode() == null) problems.Add("Start node is not set or missing from the list.");


        var guids = new HashSet<string>();
        foreach (var node in nodes)
        {
            if (string.IsNullOrEmpty(node.guid)) problems.Add("A node has an empty GUID.");
            else if (!guids.Add(node.guid)) problems.Add($"Duplicate GUID found: {node.guid}");


            if (node.options != null)
            {
                foreach (var opt in node.options)
                {
                    if (opt.requiresDiceCheck)
                    {
                        if (opt.target < 0) problems.Add($"Node {node.guid} has a dice target < 0.");
                        if (!string.IsNullOrEmpty(opt.nextOnSuccessGuid) && GetNode(opt.nextOnSuccessGuid) == null)
                            problems.Add($"Node {node.guid} has dice success pointing to missing GUID {opt.nextOnSuccessGuid}");
                        if (!string.IsNullOrEmpty(opt.nextOnFailGuid) && GetNode(opt.nextOnFailGuid) == null)
                            problems.Add($"Node {node.guid} has dice fail pointing to missing GUID {opt.nextOnFailGuid}");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(opt.nextNodeGuid) && GetNode(opt.nextNodeGuid) == null)
                            problems.Add($"Node {node.guid} has option pointing to missing GUID {opt.nextNodeGuid}");
                    }
                }
            }
        }
        return problems;
    }
}