#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(DialogueGraph))]
public class DialogueGraphEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        var graph = (DialogueGraph)target;
        EditorGUILayout.Space();


        if (GUILayout.Button("Add New Node"))
        {
            Undo.RecordObject(graph, "Add Dialogue Node");
            var node = new DialogueNode { speaker = "NPC", line = "New line" };
            graph.nodes.Add(node);
            if (string.IsNullOrEmpty(graph.startNodeGuid)) graph.startNodeGuid = node.guid;
            EditorUtility.SetDirty(graph);
        }


        if (GUILayout.Button("Validate Graph"))
        {
            var issues = string.Join("\n", graph.Validate());
            if (string.IsNullOrEmpty(issues))
                EditorUtility.DisplayDialog("Dialogue Graph", "No problems found!", "OK");
            else
                EditorUtility.DisplayDialog("Dialogue Graph Issues", issues, "OK");
        }


        if (GUILayout.Button("Regenerate All GUIDs (DANGEROUS)"))
        {
            if (EditorUtility.DisplayDialog("Regenerate GUIDs?",
            "This will break existing links between options and nodes unless you fix them manually.",
            "Proceed", "Cancel"))
            {
                Undo.RecordObject(graph, "Regenerate GUIDs");
                foreach (var n in graph.nodes) n.guid = System.Guid.NewGuid().ToString();
                EditorUtility.SetDirty(graph);
            }
        }
    }
}
#endif