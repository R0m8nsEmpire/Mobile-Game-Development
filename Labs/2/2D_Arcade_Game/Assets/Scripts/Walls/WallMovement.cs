using UnityEngine;
using UnityEngine.SceneManagement;

public class WallMovement : MonoBehaviour
{
    [Header("Wall Speed")]
    //public variable to set the walls speed
    public static float wallSpeed = 5;

    private void Update()
    {
        //Compare if the walls position is less than -10
        if(transform.position.x <= -25)
        {
            //Destroy the Wall if it is passed -10
            Destroy(gameObject);
        }

        //Get an array of all of the RigidBody2D's attached to the GameObject
        Rigidbody2D[] rb = GetComponentsInChildren<Rigidbody2D>();

        //On each of the RigidBody2D's set their linear velocity
        foreach (Rigidbody2D rb2 in rb)
        {
            //Set the rigidbody linearVelocityX speed, and adjust it depending on the players movement
            rb2.linearVelocityX = -wallSpeed - PlayerMovement.playerMovingWallSpeed;
        }
        //Get the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        //If the player is out of lives delete the remaining walls
        if (PlayerMovement.playerLives <= 0 && currentScene.name == "SinglePlayer")
        {
            GoTransparentAndDestroySelf();
        }
        //If either player is dead in multiplayer delete the remaining walls
        else if ((!MultiplayerController.player1Alive || !MultiplayerController.player2Alive) && currentScene.name == "Multiplayer")
        {
            GoTransparentAndDestroySelf();
        }
    }

    public void GoTransparentAndDestroySelf()
    {
        //Get every script attched to the gameobject children called FadeSprite
        FadeSprite[] fadeSprite = GetComponentsInChildren<FadeSprite>();
        //Do this for every FadeSprite script
        foreach (FadeSprite fs in fadeSprite)
        {
            //If the gameobject isn't fading
            if(fs.fading == null)
            {
                //Start fading the gameObject
                fs.fading = fs.StartCoroutine(fs.FadeAndDestroy());
            }
        }

    }
}