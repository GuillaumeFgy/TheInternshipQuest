using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DiceRollWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] protected TextMeshProUGUI diceValueText;
    [SerializeField] protected GameObject diceWindow; // If null, will use this GameObject // If null, will use this GameObject
    [SerializeField] protected Image image; // Face image that shows changing sprites // Face image that shows changing sprites
    [SerializeField] protected Image diceResult; // Overlay/result image you were toggling // Overlay/result image you were toggling
    [SerializeField] protected Button buttonZeroAlpha; // Invisible button the player clicks to roll // Invisible button the player clicks to roll


    [Header("Sprites")] public List<Sprite> diceSprites = new List<Sprite>(); // 1..20 in order
    [SerializeField] protected Sprite diceStart; // Default/start face // Default/start face


    private Action<int> _onComplete;
    private int _target;
    private string _prompt;

    protected void Awake()
    {
        if (diceResult != null) diceResult.enabled = false;
        if (buttonZeroAlpha != null) buttonZeroAlpha.onClick.RemoveAllListeners();
        diceWindow.SetActive(false);
    }


    /// <summary>
    /// Opens the dice window and arms the roll button. When the user clicks, a roll occurs.
    /// After the animation/timing, this calls onComplete(result) and closes the window.
    /// </summary>
    public void Open(string prompt, int target, Action<int> onComplete)
    {
        _onComplete = onComplete;
        _prompt = prompt;
        _target = target;

        // Show the dice prompt (e.g., "Strength Check DC 12")
        if (diceValueText) diceValueText.text = _prompt;

        if (diceResult) diceResult.enabled = false;
        if (image && diceStart) image.sprite = diceStart;

        // (Keep your existing parent-activation + SetActive(true) code here)
        EnsureHierarchyActive(diceWindow != null ? diceWindow : this.gameObject);
        (diceWindow != null ? diceWindow : this.gameObject).SetActive(true);

        // Arm the roll button (or auto-start if you prefer)
        if (buttonZeroAlpha != null)
        {
            buttonZeroAlpha.gameObject.SetActive(true);
            buttonZeroAlpha.onClick.RemoveAllListeners();
            buttonZeroAlpha.onClick.AddListener(() => StartCoroutine(RollRoutine()));
        }
    }



    private IEnumerator RollRoutine()
    {
        if (buttonZeroAlpha) buttonZeroAlpha.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);

        int number = UnityEngine.Random.Range(1, 21);

        yield return new WaitForSeconds(2f);

        if (diceSprites != null && diceSprites.Count >= number)
        {
            var face = diceSprites[number - 1];
            if (diceResult != null)
            {
                diceResult.enabled = true;
                diceResult.sprite = face;
            }
            else if (image != null)
            {
                image.sprite = face;
            }
        }

        if (diceValueText)
            diceValueText.text = (number >= _target) ? "Success" : "Failure";

        // Small pause so the player can read it (tweak as you like)
        yield return new WaitForSeconds(1.5f);

        // Close + callback
        Close();
        var cb = _onComplete; _onComplete = null;
        cb?.Invoke(number);
    }

    public void Close()
    {
        if (buttonZeroAlpha)
        {
            buttonZeroAlpha.onClick.RemoveAllListeners();
            buttonZeroAlpha.gameObject.SetActive(true);
        }
        if (image && diceStart) image.sprite = diceStart;
        if (diceResult)
        {
            diceResult.enabled = false;
            diceResult.sprite = null; // clear previous face to avoid white overlay next time
        }
        diceWindow.SetActive(false);
    }

    // Turn on any inactive parent(s) so this object can become active-in-hierarchy.
    private void EnsureHierarchyActive(GameObject go)
    {
        var chain = new System.Collections.Generic.List<Transform>();
        var t = go != null ? go.transform : null;
        while (t != null)
        {
            chain.Add(t);
            t = t.parent;
        }
        // Activate from top-most parent down to the target
        for (int i = chain.Count - 1; i >= 0; --i)
        {
            var g = chain[i].gameObject;
            if (!g.activeSelf) g.SetActive(true);
        }
    }

    // For debugging: which ancestor is blocking activation?
    private GameObject FindFirstInactiveAncestor(GameObject go)
    {
        var t = go != null ? go.transform : null;
        while (t != null)
        {
            if (!t.gameObject.activeSelf) return t.gameObject;
            t = t.parent;
        }
        return null;
    }

}