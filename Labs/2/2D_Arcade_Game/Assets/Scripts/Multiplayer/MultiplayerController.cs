using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class MultiplayerController : MonoBehaviour
{
    [Header("Canvas")]
    //Game Canvas'
    [SerializeField] CanvasGroup gameOverCanvas;
    [SerializeField] CanvasGroup fadeToBlack;
    //Game Over corutine
    private Coroutine gameOverCoroutine;
    [Header("Time spent Alive")]
    //Time the player has survived
    public float timeSurvived;
    [Header("Game over text")]
    [SerializeField] TextMeshProUGUI timeSurvivedText;
    [SerializeField] TextMeshProUGUI whoWinsText;

    [Header("Audio")]
    private AudioSource bkgMusic;

    [Header("InputManager")]
    private PlayerInputManager inputManager;
    public static bool player1Alive;
    public static bool player2Alive;

    [Header("Starting Game Objects")]
    [SerializeField] GameObject[] startingGameObjects;
    [SerializeField] GameObject joinButtonText;

    private void Start()
    {
        //Get the PlayerInputManager component
        inputManager = GetComponent<PlayerInputManager>();
        //Set the Game Over canvas to be invisible at the start of the game
        gameOverCanvas.alpha = 0;
        //Fade the scene
        StartCoroutine(FadeOutBlack());
        gameOverCoroutine = null;
        //Reset time survived
        timeSurvived = 0;
        //Get the BKG Music
        bkgMusic = GetComponent<AudioSource>();

        //Disable starting game objects
        foreach (GameObject obj in startingGameObjects)
        {
            //Disable the object
            obj.SetActive(false);
        }

        //Set both players to alive at the start of the game
        player1Alive = true;
        player2Alive = true;

        
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
        //Check for a minimum of 2 players
        if (inputManager.playerCount == 2)
        {
            foreach (GameObject obj in startingGameObjects)
            {
                //Enable the object
                obj.SetActive(true);
            }
            //Disable the join button text
            joinButtonText.SetActive(false);
        }
        else
        {
            //If there are not 2 players then don't run the rest of the update
            timeSurvived = 0;
            return;
        }


        CheckGameOver();
        IncreaseTimeScale();
    }
    private void CheckGameOver()
    {
        if (!player1Alive || !player2Alive)
        {
            //Check who won
            if (!player1Alive && player2Alive)
            {
                whoWinsText.text = "Player 2 Wins!";
            }
            if (!player2Alive && player1Alive)
            {
                whoWinsText.text = "Player 1 Wins!";
            }
            if(!player1Alive && !player2Alive)
            {
                whoWinsText.text = "It's a Tie!";
            }
            //Start the game Over Corutine
            if (gameOverCoroutine == null)
            {
                gameOverCoroutine = StartCoroutine(HideGameShowEnd());
                //Set the Game Over text and time survived text
                timeSurvivedText.text = "Time Survived: " + Mathf.Round(timeSurvived).ToString() + "s";
            }
            //Disable starting game objects
            foreach (GameObject obj in startingGameObjects)
            {
                //Disable the object
                obj.SetActive(false);
            }
        }
        else
        {
            //Set the time survived to the unscaled time survived
            timeSurvived += Time.unscaledDeltaTime;
        }
    }
    private void IncreaseTimeScale()
    {
        //Slowly increase the speed of the entire game
        if (Time.timeScale < 2)
        {
            Time.timeScale += Time.unscaledDeltaTime / 120;
        }
        //Clamp the time scale between .5 and 2
        Time.timeScale = Mathf.Clamp(Time.timeScale, .5f, 2);
    }
    private IEnumerator HideGameShowEnd()
    {
        //Fade in the Game Over canvas
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