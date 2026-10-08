using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    [Header("Player Speed variables")]
    //Player speed variable
    public float playerSpeed = 5;
    //public static int Player Lives
    public static int playerLives = 3;
    //Player glide speed. The higher the value the less Glide
    [Range(1f, 10f)] public float playerGlideSpeed = 1;

    //Wall speed float is the player is moving horizontal
    public static float playerMovingWallSpeed = 0;
    //background wall speed
    public static float backgroundWallSpeed = 1;

    [Header("Player Life Images")]
    //Player life images
    [SerializeField] GameObject lifeImage1;
    [SerializeField] GameObject lifeImage2;
    [SerializeField] GameObject lifeImage3;

    //Player Input
    InputAction moveAction;

    //Rigidbiody2D component
    private Rigidbody2D rb;
    
    //Thrust Particle
    [SerializeField] ParticleSystem playerThrustEffect;

    //Animator Component
    private Animator animator;

    [Header("Audio")]
    //Audio Source Component for an explosion Sound Effect
    [SerializeField] AudioSource playerHitSound;
    [SerializeField] AudioClip explosionClip;
    [SerializeField] AudioClip hitClip;



    private void Start()
    {
        //Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        //Get the input action called "Move"
        moveAction = InputSystem.actions.FindAction("Move");
        //Reset Lives to 3
        playerLives = 3;
        //Start the player thrust
        playerThrustEffect.Play();
        //Get the Animator Component
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        //Call functions
        Movement();
        PlayerWallSpeed();
        lives();
        ToggleThrust();
    }
    private void Movement()
    {
        //Read the Move input action
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        //Calculate the target velocity
        Vector2 targetVelocity = moveInput * playerSpeed;

        //chage the players velocity basd on the movement action
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.unscaledDeltaTime * playerGlideSpeed);
    }


    private void PlayerWallSpeed()
    {
        //Compare if the player is moving forwards or backwards
        if (rb.linearVelocityX > .5f)
        {
            //If the player is moving forward then increase the walls' speed and the background speed
            playerMovingWallSpeed = Mathf.Lerp(playerMovingWallSpeed, 1, Time.unscaledDeltaTime * playerGlideSpeed);
            backgroundWallSpeed = Mathf.Lerp(backgroundWallSpeed, 1.5f, Time.unscaledDeltaTime * playerGlideSpeed);
        }
        else if (rb.linearVelocityX < -.5f)
        {
            //If the player is moving backwards then decrease the walls' speed and the background speed
            playerMovingWallSpeed = Mathf.Lerp(playerMovingWallSpeed, -1, Time.unscaledDeltaTime * playerGlideSpeed);
            backgroundWallSpeed = Mathf.Lerp(backgroundWallSpeed, .5f, Time.unscaledDeltaTime * playerGlideSpeed);
        }
        else
        {
            //If the player isn't moving then set the speed to 0 and normal background speed
            playerMovingWallSpeed = Mathf.Lerp(playerMovingWallSpeed, 0, Time.unscaledDeltaTime * playerGlideSpeed);
            backgroundWallSpeed = Mathf.Lerp(backgroundWallSpeed, 1, Time.unscaledDeltaTime * playerGlideSpeed);
        }
    }
    private void ToggleThrust()
    {
        //Read If the player is applying input
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        //Get the main settings of the particle system
        var main = playerThrustEffect.main;
        //If the player is moving right or left, set the speed of the particles
        if (moveInput.sqrMagnitude > 0 || moveInput.sqrMagnitude < 0)
        {
            main.startSpeed = Mathf.Lerp(main.startSpeed.constant, 15, Time.unscaledDeltaTime * playerGlideSpeed);
        }
        else
        {
            main.startSpeed = Mathf.Lerp(main.startSpeed.constant, 0, Time.unscaledDeltaTime * playerGlideSpeed);
        }
    }
    private void lives()
    {
        //Show how many lives the player has
        if (playerLives == 3)
        {
            //Enable all life images
            lifeImage1.SetActive(true);
            lifeImage2.SetActive(true);
            lifeImage3.SetActive(true);
        }
        else if(playerLives == 2)
        {
            //Disable the third life image
            lifeImage3.SetActive(false);
            //Enable the first and second life images
            lifeImage1.SetActive(true);
            lifeImage2.SetActive(true);
        }
        else if (playerLives == 1)
        {
            //Disable the second life image
            lifeImage2.SetActive(false);
            //Enable the first life image
            lifeImage1.SetActive(true);
        }
        else if (playerLives == 0)
        {
            //Disable the first life image
            lifeImage1.SetActive(false);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Compare if the player is hitting a wall
        if (collision.CompareTag("Walls"))
        {
            //If it is remove a player life
            playerLives--;
            //If the player hits 0 lives
            if (playerLives == 0)
            {
                //Set the players speed to 0 so they can't move
                playerSpeed = 0;
                playerThrustEffect.Stop();
                //Play the explosion animation
                animator.SetTrigger("Explode");
                //Play the explosion Audio
                playerHitSound.clip = explosionClip;
                playerHitSound.Play();
                Time.timeScale = 1;
            }
            else
            {
                //Play the Hit Audio if the player has more than 0 lives
                playerHitSound.clip = hitClip;
                playerHitSound.Play();
            }
        }
    }
}
