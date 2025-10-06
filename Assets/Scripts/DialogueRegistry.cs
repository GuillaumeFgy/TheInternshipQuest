using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DialogueRegistry", menuName = "Dialogue/Registry", order = 1)]
public class DialogueRegistry : ScriptableObject
{
    [Tooltip("All dialogue tags used in this project.")]
    public List<DialogueTag> allTags = new();

    private Dictionary<string, DialogueTag> _lookup;

    public void Init()
    {
        _lookup = new Dictionary<string, DialogueTag>();
        foreach (var tag in allTags)
        {
            if (tag != null && !string.IsNullOrEmpty(tag.tagName))
            {
                if (!_lookup.ContainsKey(tag.tagName))
                    _lookup.Add(tag.tagName, tag);
                else
                    Debug.LogWarning($"Duplicate dialogue tag name: {tag.tagName}");
            }
        }
    }

    public DialogueTag GetTag(string name)
    {
        if (_lookup == null) Init();
        _lookup.TryGetValue(name, out var tag);
        return tag;
    }
}
