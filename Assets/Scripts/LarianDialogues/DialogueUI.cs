using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DialogueUI : MonoBehaviour
{
    [Header("References")] public DialogueRunner runner;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI lineText;
    public Transform optionsContainer;
    public Button optionButtonPrefab;

    [Header("Typewriter")]
    public bool typewriter = true;
    [Tooltip("Characters per second for the typewriter effect.")]
    public float charsPerSecond = 40f;

    private Coroutine typingRoutine;


    [Header("Auto Start")] public bool playOnStart;


    void Awake()
    {
        if (runner != null) runner.OnNodeEntered += HandleNode;
    }

    void OnDestroy()
    {
        if (runner != null) runner.OnNodeEntered -= HandleNode;
        if (typingRoutine != null) StopCoroutine(typingRoutine);
    }



    void Start()
    {
        if (playOnStart) runner.Begin();
    }


    private void HandleNode(DialogueNode node)
    {
        ClearOptions();


        if (node == null)
        {
            // Dialogue ended
            if (speakerText) speakerText.text = "";
            if (lineText) lineText.text = "";
            gameObject.SetActive(false); // hide the UI when finished (optional)
            return;
        }


        gameObject.SetActive(true);
        if (speakerText) speakerText.text = node.speaker;
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = typewriter
            ? StartCoroutine(TypeLine(node.line))
            : StartCoroutine(TypeImmediate(node.line));


        if (node.options != null && node.options.Count > 0)
        {
            for (int i = 0; i < node.options.Count; i++)
            {
                var option = node.options[i];
                var btn = Instantiate(optionButtonPrefab, optionsContainer);
                var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (label) label.text = option.text;
                int captured = i;
                btn.onClick.AddListener(() => runner.ChooseOption(captured));
            }
        }
        else
        {
            // No options = end of conversation; show a Continue/Close button
            var btn = Instantiate(optionButtonPrefab, optionsContainer);
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label) label.text = "Close";
            btn.onClick.AddListener(() => runner.End());
        }
    }

    private void ClearOptions()
    {
        for (int i = optionsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(optionsContainer.GetChild(i).gameObject);
        }
    }
    private System.Collections.IEnumerator TypeImmediate(string text)
    {
        if (lineText) lineText.text = text;
        yield break;
    }

    private System.Collections.IEnumerator TypeLine(string text)
    {
        if (lineText == null)
            yield break;

        lineText.text = string.Empty;
        if (string.IsNullOrEmpty(text))
            yield break;

        float delay = (charsPerSecond <= 0f) ? 0f : 1f / charsPerSecond;

        // basic per-character reveal
        for (int i = 0; i < text.Length; i++)
        {
            lineText.text += text[i];
            if (delay > 0f) yield return new WaitForSeconds(delay);
            else yield return null; // next frame
        }
    }

}