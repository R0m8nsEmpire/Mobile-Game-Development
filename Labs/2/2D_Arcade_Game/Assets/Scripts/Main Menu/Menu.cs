using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Menu")]
    //Fade to black
    [SerializeField] CanvasGroup fadeToBlack;
    //Highscore
    [SerializeField] TextMeshProUGUI highScoreText;

    [Header("Audio")]
    //Audio
    [SerializeField] AudioSource buttonClick;
    [SerializeField] AudioSource bkgMusic;
    private void Start()
    {
        fadeToBlack.gameObject.SetActive(true);
        fadeToBlack.alpha = 1.0f;
        StartCoroutine(FadeOutBlack());
        buttonClick = GetComponent<AudioSource>();
        //Get and set the HighScore Text.
        highScoreText.text = "HighScore: " + PlayerPrefs.GetFloat("HighScore");
    }
    public void QuitGame()
    {
        buttonClick.Play();
        Application.Quit();
    }
    public void StartGameSinglePlayer()
    {
        buttonClick.Play();
        fadeToBlack.alpha = 0;
        fadeToBlack.gameObject.SetActive(true);
        StartCoroutine(FadeToBlackSinglePlayer());
    }
    public void StartGameMultiPlayer()
    {
        buttonClick.Play();
        fadeToBlack.alpha = 0;
        fadeToBlack.gameObject.SetActive(true);
        StartCoroutine(FadeToBlackMultiPlayer());
    }
    private IEnumerator FadeToBlackSinglePlayer()
    {
        while (fadeToBlack.alpha < 1)
        {
            bkgMusic.volume -= Time.deltaTime / 2;
            fadeToBlack.alpha += Time.deltaTime / 2;
            yield return null;
        }
        SceneManager.LoadScene("Singleplayer");
        yield return null;
    }
    private IEnumerator FadeToBlackMultiPlayer()
    {
        while (fadeToBlack.alpha < 1)
        {
            bkgMusic.volume -= Time.deltaTime / 2;
            fadeToBlack.alpha += Time.deltaTime / 2;
            yield return null;
        }
        SceneManager.LoadScene("MultiPlayer");
        yield return null;
    }
    private IEnumerator FadeOutBlack()
    {
        yield return new WaitForSeconds(.5f);
        while (fadeToBlack.alpha > 0)
        {
            fadeToBlack.alpha -= Time.deltaTime / 2;
            yield return null;
        }
        fadeToBlack.gameObject.SetActive(false);
        yield return null;
    }
}
