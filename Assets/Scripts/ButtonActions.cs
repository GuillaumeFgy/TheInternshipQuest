using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonActions : MonoBehaviour
{

    [SerializeField] private GameObject steam;
    [SerializeField] private GameObject charSheet;
    [SerializeField] private DialogueTagPlayer dialogue;
    [SerializeField] private AudioClip HellerMusic;
    [SerializeField] private AudioClip bg3Music;
    [SerializeField] private AudioClip cat1;
    [SerializeField] private AudioClip cat2;
    [SerializeField] private AudioClip cat3;
    [SerializeField] private GameObject secondCatButton;

    private bool hasbeenpet = false;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "HouseScene") { charSheet.SetActive(true); }
        steam.SetActive(false);
    }

    public void PetTheCat() 
    {
        AudioManager.I.PlaySFX(cat1);
        secondCatButton.SetActive(true);
        dialogue.PlayTag("Cat");
    }

    public void PetAgain() 
    {
        dialogue.PlayTag("Cat");
        if (!hasbeenpet) 
        {
            AudioManager.I.PlaySFX(cat2);
            hasbeenpet = true;
            return;
        }
        AudioManager.I.PlaySFX(cat3);
        secondCatButton.SetActive(false);
    }

    public void GoToComputer() {
        secondCatButton.SetActive(false) ;
    }

    public void startComputer() {
        steam.SetActive(true);
        StartCoroutine(DoWithDelay(5f, () =>
        {
           dialogue.PlayTag("Steam");
        }));
        dialogue.PlayTag("Steam");

    }

    public void CloseSheet() 
    {
        charSheet.SetActive(false);
    }

    public void launchGame() 
    {
        SceneManager.LoadScene("AstarionRace");
    }

    public void PlayPiano()
    {
        StartCoroutine(DoWithDelay(2f, () =>
        {
            AudioManager.I.PlayMusic(HellerMusic);
            AudioManager.I.SetMusicVolume(0.2f);
        }));

        dialogue.PlayTag("Piano");
    }

    public void CheckCalendar() 
    {
        AudioManager.I.CrossfadeMusic(bg3Music);
        AudioManager.I.SetMusicVolume(0.2f);
        dialogue.PlayTag("Calendar");

    }

    public void Exit()
    {
        StartCoroutine(DoWithDelay(7f, () =>
        {
            SceneManager.LoadScene("Map");
        }));
        dialogue.PlayTag("Exit");

    }

    private IEnumerator DoWithDelay(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
}
