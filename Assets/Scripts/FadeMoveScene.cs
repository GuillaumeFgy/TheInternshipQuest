using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeMoveScene : MonoBehaviour
{
    [Header("Fade Settings")]
    public CanvasGroup targetCanvasGroup;      // The GameObject to fade (must have CanvasGroup)
    public float fadeDuration = 1.0f;          // Time for fade in/out

    [Header("Move Settings")]
    public Transform moveTarget;               // The destination point
    public float moveDuration = 2.0f;          // Time to move from start to destination
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scene Settings")]
    public string sceneToLoad = "LarianScene"; // Scene to load after fade out
    public float waitBeforeSceneLoad = 0.5f;   // Small delay before switching scenes

    private Vector3 startPos;

    void Start()
    {
        if (targetCanvasGroup == null)
        {
            Debug.LogError("No CanvasGroup assigned!");
            return;
        }

        if (moveTarget == null)
        {
            Debug.LogError("No move target assigned!");
            return;
        }

        startPos = targetCanvasGroup.transform.position;
        targetCanvasGroup.alpha = 0f; // Start invisible

        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        // Fade In
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // Move
        yield return StartCoroutine(MoveToPoint(startPos, moveTarget.position, moveDuration));

        // Fade Out
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        // Small wait before scene change
        yield return new WaitForSeconds(waitBeforeSceneLoad);

        // Load new scene
        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            targetCanvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        targetCanvasGroup.alpha = to;
    }

    private IEnumerator MoveToPoint(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = moveCurve.Evaluate(t);
            targetCanvasGroup.transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
        targetCanvasGroup.transform.position = to;
    }
}
