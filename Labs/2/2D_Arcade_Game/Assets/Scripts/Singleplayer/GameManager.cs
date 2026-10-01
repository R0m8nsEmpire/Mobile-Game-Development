using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Canvas")]
    //Game Canvas'
    [SerializeField] CanvasGroup gameOverCanvas;
    [SerializeField] CanvasGroup gameUICanvas;
    [SerializeField] CanvasGroup fadeToBlack;
    //Game Over corutine
    private Coroutine gameOverCoroutine;
    [Header("Time spent Alive")]
    //Time the player has survived
    public float timeSurvived;
    [Header("Time survived and player score text")]
    [SerializeField] TextMeshProUGUI timeSurvivedText;
    [SerializeField] TextMeshProUGUI playerScoreText;

    [Header("Audio")]
    private AudioSource bkgMusic;
    private void Start()
    {
        //Set the Game Over canvas to be invisible at the start of the game
        gameOverCanvas.alpha = 0;
        gameUICanvas.alpha = 1;
        //Fade the scene
        StartCoroutine(FadeOutBlack());
        gameOverCoroutine = null;
        //Reset time survived
        timeSurvived = 0;
        //Get the BKG Music
        bkgMusic = GetComponent<AudioSource>();
    }
    private IEnumerator FadeOutBlack()
    {
        //Fade out of black
        yield return new WaitForSeconds(.5f);
        //Repeat this until the canvas is invisible
        while (fadeToBlack.alpha > 0)
        {
            fadeToBlack.alpha -= Time.deltaTime / 2;
            yield return null;
        }
        //Disable the fade to black once it has disapeared
        fadeToBlack.gameObject.SetActive(false);
        yield return null;
    }
    private void Update()
    {
        if (PlayerMovement.playerLives <= 0)
        {
            //Start the game Over Corutine
            if(gameOverCoroutine == null)
            {
                gameOverCoroutine = StartCoroutine(HideGameShowEnd());
            }
            //Set the Game Over text and time survived text
            timeSurvivedText.text = "Time Survived: " + Mathf.Round(timeSurvived).ToString() + "s";
            playerScoreText.text = "Final Score: " + PlayerScore.playerScore;
        }
        else
        {
            //Set the time survived to the unscaled time survived
            //So when a player picks up a powerup the speed change doesnt change it
            timeSurvived += Time.unscaledDeltaTime;
        }   
    }
    private IEnumerator HideGameShowEnd()
    {
        //Fade in the Game Over canvas and fade out the Game UI canvas
        while (gameUICanvas.alpha != 0)
        {
            gameUICanvas.alpha -= Time.deltaTime / 2;
            yield return null;
        }
        while (gameOverCanvas.alpha != 1)
        {
            gameOverCanvas.alpha += Time.deltaTime / 5;
            yield return null;
        }
        StartCoroutine(ResetGame());
        yield return null;
    }
    private IEnumerator ResetGame()
    {
        //Restart the game / fade to black
        yield return new WaitForSeconds(5);
        fadeToBlack.gameObject.SetActive(true);
        while (fadeToBlack.alpha < 1)
        {
            bkgMusic.volume -= Time.deltaTime / 2;
            fadeToBlack.alpha += Time.deltaTime / 2;
            yield return null;
        }
        //return the player to the main menu
        SceneManager.LoadScene("MainMenuScene");
        yield return null;
    }
}
