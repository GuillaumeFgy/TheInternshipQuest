using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DialoguesManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private Text dialogueText;
    [SerializeField] private CanvasGroup dialogueCanvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float letterDelay = 0.03f;

    private Coroutine currentCoroutine;
    private Coroutine typingCoroutine;   // <- NEW: handle for the typewriter

    private void Awake()
    {
        dialogueBox.SetActive(false);
        if (dialogueCanvasGroup != null) dialogueCanvasGroup.alpha = 0f;
    }

    public void ShowDialogue(string message, float duration = 3f)
    {
        // Stop any ongoing coroutines cleanly
        if (typingCoroutine != null) { StopCoroutine(typingCoroutine); typingCoroutine = null; }
        if (currentCoroutine != null) { StopCoroutine(currentCoroutine); currentCoroutine = null; }

        // Hard reset UI to a clean state
        if (dialogueCanvasGroup != null) dialogueCanvasGroup.alpha = 0f;
        dialogueText.text = "";
        dialogueBox.SetActive(true);

        currentCoroutine = StartCoroutine(ShowDialogueRoutine(message, duration));
    }

    private IEnumerator ShowDialogueRoutine(string message, float duration)
    {
        // Ensure no leftover text shows before fade-in
        dialogueText.text = "";

        // Fade in
        if (dialogueCanvasGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0f, 1f, fadeDuration));

        // Start and track the typewriter so we can cancel it if interrupted
        typingCoroutine = StartCoroutine(TypeText(message));
        yield return typingCoroutine; // waits until finished unless interrupted
        typingCoroutine = null;

        // Hold, then fade out
        yield return new WaitForSeconds(duration);

        if (dialogueCanvasGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 1f, 0f, fadeDuration));

        dialogueBox.SetActive(false);
        dialogueText.text = "";
        currentCoroutine = null;
    }

    private IEnumerator TypeText(string message)
    {
        dialogueText.text = "";
        foreach (char c in message)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(letterDelay);
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float time)
    {
        if (group == null) yield break;
        float elapsed = 0f;
        group.alpha = from;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / time);
            yield return null;
        }
        group.alpha = to;
    }
}
