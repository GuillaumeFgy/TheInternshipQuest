using UnityEngine;

public class StatsBarController : MonoBehaviour
{
    public RectTransform fillDamnation;
    public RectTransform fillHeat;
    public RectTransform fillSatisfaction;

    [SerializeField] private float maxHeight = 400f;
    private void Start()
    {
        UpdateBars(1f, 0f, 0f);
    }

    public void UpdateBars(float stat1, float stat2, float stat3)
    {
        fillDamnation.sizeDelta = new Vector2(fillDamnation.sizeDelta.x, stat1 * maxHeight);
        fillHeat.sizeDelta = new Vector2(fillHeat.sizeDelta.x, stat2 * maxHeight);
        fillSatisfaction.sizeDelta = new Vector2(fillSatisfaction.sizeDelta.x, stat3 * maxHeight);
    }
}
