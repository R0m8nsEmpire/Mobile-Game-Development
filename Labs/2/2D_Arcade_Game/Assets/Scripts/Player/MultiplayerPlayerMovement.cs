using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerPlayerMovement : MonoBehaviour
{
    [Header("Player Speed variables")]
    //Player speed variable
    public float playerSpeed = 5;
    //Player glide speed. The higher the value the less Glide
    [Range(1f, 10f)] public float playerGlideSpeed = 1;

    //Wall speed float is the player is moving horizontal
    public static float playerMovingWallSpeed = 0;
    //background wall speed
    public static float backgroundWallSpeed = 1;

    //Player Input
    InputAction moveAction;
    PlayerInput playerInput;

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


    private void Start()
    {
        //Get the PlayerInput component
        playerInput = GetComponent<PlayerInput>();
        //Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        //Get the input action called "Move"
        moveAction = playerInput.actions["Move"];
        //Start the player thrust
        playerThrustEffect.Play();
        //Get the Animator Component
        animator = GetComponent<Animator>();
        //Check if player 1 or player 2 and set different colors
        if(playerInput.playerIndex != 0)
        {
            //Set player 2 color to red
            GetComponent<SpriteRenderer>().color = Color.red;
            MultiplayerController.player2Alive = true;
        }
        else
        {
            MultiplayerController.player1Alive = true;
        }
    }

    private void Update()
    {
        //Call functions
        Movement();
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
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Compare if the player is hitting a wall
        if (collision.CompareTag("Walls"))
        {
            //Reset Color
            GetComponent<SpriteRenderer>().color = Color.white;
            //Set the players speed to 0 so they can't move
            playerSpeed = 0;
            playerThrustEffect.Stop();
            //Play the explosion animation
            animator.SetTrigger("Explode");
            //Play the explosion Audio
            playerHitSound.clip = explosionClip;
            playerHitSound.Play();


            if(playerInput.playerIndex == 0)
            {
                MultiplayerController.player1Alive = false;
            }
            else
            {
                MultiplayerController.player2Alive = false;
            }
        }
    }
}
