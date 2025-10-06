using System.Collections.Generic;
using UnityEngine;

public class DialogueResolver : MonoBehaviour
{
    [SerializeField] private DialogueRegistry registry;

    private readonly Dictionary<DialogueTag, int> _progressByTag = new();

    public string GetNextLine(DialogueTag tag)
    {
        if (tag == null) return null;

        if (TryGetNextFromTag(tag, out string line))
            return line;

        return GetNextFromParents(tag.parentTag);
    }

    public string GetNextLine(string tagName)
    {
        var tag = registry?.GetTag(tagName);
        if (tag == null)
        {
            Debug.LogWarning($"Tag '{tagName}' not found in registry.");
            return null;
        }
        return GetNextLine(tag);
    }

    private bool TryGetNextFromTag(DialogueTag tag, out string line)
    {
        line = null;
        if (tag.lines == null || tag.lines.Count == 0) return false;

        if (!_progressByTag.TryGetValue(tag, out int idx))
            idx = 0;

        if (idx < tag.lines.Count)
        {
            line = tag.lines[idx];
            _progressByTag[tag] = idx + 1;
            return true;
        }
        return false;
    }

    private string GetNextFromParents(DialogueTag parent)
    {
        while (parent != null)
        {
            if (TryGetNextFromTag(parent, out string line))
                return line;

            parent = parent.parentTag;
        }
        return null;
    }
}
