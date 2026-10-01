using System.Collections;
using UnityEngine;
public class PlayerPowerups : MonoBehaviour
{
    private Coroutine speedCorutine;
    private GameObject powerup;

    [Header("Audio")]
    //Powerup Sound
    [SerializeField] AudioSource powerupSound;
    [SerializeField] AudioClip powerupSoundClip;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Check if the player is colliding with a powerup
        if (collision.CompareTag("Speed Powerup"))
        {
            //If the player isn't already running a powerup then continue
            if(speedCorutine == null)
            {
                //Set the powerup Object to what the player collided with
                powerup = collision.gameObject;
                //start and set the coroutines
                speedCorutine = StartCoroutine(SpeedPowerup());
                //Shrink the Powerup
                StartCoroutine(shrinkObject());
                //Disable the collider so the player can't interact with it again
                powerup.GetComponent<CircleCollider2D>().enabled = false;
                //Play the powerup sound
                powerupSound.clip = powerupSoundClip;
                powerupSound.Play();
            }
        }
        //Check if the player is colliding with a life powerup
        if (collision.CompareTag("Life"))
        {
            if(PlayerMovement.playerLives < 3)
            {
                //Increase the player lives by 1
                PlayerMovement.playerLives += 1;
                //Set the powerup Object to what the player collided with
                powerup = collision.gameObject;
                //Shrink the Powerup
                StartCoroutine(shrinkObject());
                //Disable the collider so the player can't interact with it again
                powerup.GetComponent<CircleCollider2D>().enabled = false;
                //Play the powerup sound
                powerupSound.clip = powerupSoundClip;
                powerupSound.Play();
            }
        }
    }

    IEnumerator shrinkObject()
    {
        //Set a new target scale to 0
        Vector3 targetScale = Vector3.zero;
        while (true)
        {
            //if the player already has a powerup don't continue
            if(powerup == null)
            {
                break;
            }
            //If the powerup scale isn't 0 then shrink it
            if(powerup.transform.localScale != Vector3.zero)
            {
                //Lerp the scale to the target scale
                powerup.transform.localScale = Vector3.Lerp(powerup.transform.localScale, targetScale, Time.deltaTime * 5f);
                yield return null;
            }
            //If the scale is equal to 0 then destroy it
            if(powerup != null)
            {
                if (powerup.transform.localScale == Vector3.zero)
                {
                    Destroy(powerup.gameObject);
                    yield return null;
                }
                yield return null;
            }
            yield return null;
        }
    }
    
    IEnumerator SpeedPowerup()
    {
        //Set the time of the game to half to account for 
        Time.timeScale /= 2;
        //Set the player speed to double to counter for the decreased game speed
        GetComponent<PlayerMovement>().playerSpeed *= 2;
        //Wait 5 seconds (10 because the game speed is half)
        yield return new WaitForSecondsRealtime(7.5f);
        //Return everything to normal
        Time.timeScale *= 2;
        GetComponent<PlayerMovement>().playerSpeed /= 2;
        speedCorutine = null;
    }

    private void Update()
    {
        //Check if the speed corutine is running
        if (speedCorutine == null)
        {
            //Slowly increase the speed of the entire game
            if(Time.timeScale < 2)
            {
                Time.timeScale += Time.unscaledDeltaTime / 120;
            }
            //Clamp the time scale between .5 and 2
            Time.timeScale = Mathf.Clamp(Time.timeScale, .5f, 2);
        }
    }
}
