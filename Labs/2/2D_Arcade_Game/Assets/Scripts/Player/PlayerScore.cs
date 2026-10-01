using TMPro;
using UnityEngine;
public class PlayerScore : MonoBehaviour
{
    [Header("Score")]
    //public static score so it can be accessed from anywhere
    public static float playerScore = 0;
    //Player score
    [SerializeField] TextMeshProUGUI playerScoreText;
    [SerializeField] TextMeshProUGUI highscoreText;

    [Header("Audio")]
    [SerializeField] AudioSource playerScoreSound;
    [SerializeField] AudioClip playerScoreClip;
    private void Start()
    {
        //reset the players score on start
        playerScore = 0;
        //Update the score on the games start
        UpdateScore();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Points"))
        {
            //If the player hits the score then add points
            playerScore++;
            //Destroy the point collider so the player can't repeatedly go through it
            Destroy(collision.gameObject);
            //Play the point sound
            playerScoreSound.clip = playerScoreClip;
            playerScoreSound.Play();
            //Set the high score if the regular score is higher than the high score
            if(playerScore >= PlayerPrefs.GetFloat("HighScore"))
            {
                PlayerPrefs.SetFloat("HighScore", playerScore);
            }
            //Update the score
            UpdateScore();
        }
        if (collision.CompareTag("Walls"))
        {
            //If the player hits a wall make the wall go transparent and destroy itself
            collision.GetComponentInParent<WallMovement>().GoTransparentAndDestroySelf();
        }
    }

    private void UpdateScore()
    {
        //Set the On screen score text to the players current score
        playerScoreText.text = "Score: " + playerScore;
        highscoreText.text = "Highscore: " + PlayerPrefs.GetFloat("HighScore");
    }
}