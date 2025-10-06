using UnityEngine;

public class DialogueTagPlayer : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private DialogueResolver resolver;
    [SerializeField] private DialoguesManager ui; // your existing dialogue box script

    [Header("Defaults (optional)")]
    [SerializeField] private string defaultTagName;
    [SerializeField] private float defaultDuration = 3f;

    public void PlayTag(DialogueTag tag)
    {
        float duration = -1f;
        if (resolver == null || ui == null) return;

        string line = resolver.GetNextLine(tag);
        if (!string.IsNullOrEmpty(line))
        {
            ui.ShowDialogue(line, duration > 0 ? duration : defaultDuration);
        }
    }

    public void PlayTag(string tagName, float duration = -1f)
    {
        if (resolver == null || ui == null) return;

        string line = resolver.GetNextLine(tagName);
        if (!string.IsNullOrEmpty(line))
        {
            ui.ShowDialogue(line, duration > 0 ? duration : defaultDuration);
        }
    }

    // For UI Buttons / Events in the Inspector
    public void PlayDefaultTag()
    {
        PlayTag(defaultTagName, defaultDuration);
    }
}
