using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
    public InputActionAsset inputAsset;

    [Range(1f, 20f)]
    public float movementSpeed = 5f;

    private Rigidbody2D rb2D;

    InputActionMap moveMap;

    private Vector2 movementInput;

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }
    private void OnEnable()
    {
        if (inputAsset != null)
        {
            moveMap = inputAsset.FindActionMap("Player");
            if (moveMap != null) moveMap.Enable();
        }
    }
    private void OnDisable()
    {
        if (inputAsset != null)
        {
            moveMap = inputAsset.FindActionMap("Player");
            if (moveMap != null) moveMap.Disable();
        }
    }

    public void MovementInput(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
    }
    void Update()
    {
        rb2D.linearVelocity = movementInput * movementSpeed;
    }
}
