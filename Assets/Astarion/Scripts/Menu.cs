using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject label;
    [SerializeField] private Text text;
    private bool menuDisplayed = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowMenu("");
        }
    }

    void Awake()
    {
        label.SetActive(false);
    }

    public void ShowMenu(string type) 
    {
        if (menuDisplayed) 
        {
            label.SetActive(false);
            menuDisplayed = false;
            return;
        }
        text.text = type;
        label.SetActive(true);
        menuDisplayed= true;
    }

    public void QuitGame() 
    {
        SceneManager.LoadScene("HouseScene2");
    }

    public void RestartGame() 
    {
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

}
