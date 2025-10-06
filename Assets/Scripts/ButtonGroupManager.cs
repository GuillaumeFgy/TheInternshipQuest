// ButtonGroupManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonGroup
{
    public string groupName;
    public List<Button> buttons = new();
    public List<string> nextGroups = new(); // names of groups that unlock after completing this one
}

public class ButtonGroupManager : MonoBehaviour
{
    [SerializeField] private List<ButtonGroup> allGroups = new();
    private Dictionary<string, ButtonGroup> groupsByName = new();
    private HashSet<string> completedGroups = new();
    private HashSet<Button> clickedButtons = new();

    [SerializeField] private CameraMovement cameraMovement;

    void Awake()
    {
        foreach (var group in allGroups)
        {
            groupsByName[group.groupName] = group;

            foreach (var btn in group.buttons)
            {
                btn.gameObject.SetActive(false);
                btn.onClick.AddListener(() => OnButtonClicked(btn, group.groupName));
            }
        }

        if (allGroups.Count > 0)
            SetGroupActive(allGroups[0].groupName, true);
    }

    private void OnButtonClicked(Button button, string groupName)
    {
        // Hide permanently and mark clicked
        button.gameObject.SetActive(false);
        clickedButtons.Add(button);
        
        if (cameraMovement != null)
        {
            string key = button.gameObject.name; // e.g., "Computer", "Calendar", "Bed"
            cameraMovement.TryMoveTo(key);
        }

        // Check group completion
        var group = groupsByName[groupName];
        bool allClicked = true;
        foreach (var btn in group.buttons)
        {
            if (!clickedButtons.Contains(btn))
            {
                allClicked = false;
                break;
            }
        }

        if (allClicked)
        {
            completedGroups.Add(groupName);
            foreach (var next in group.nextGroups)
                SetGroupActive(next, true);
        }
    }

    private void SetGroupActive(string groupName, bool active)
    {
        if (!groupsByName.ContainsKey(groupName)) return;
        foreach (var btn in groupsByName[groupName].buttons)
            btn.gameObject.SetActive(active);
    }
}
