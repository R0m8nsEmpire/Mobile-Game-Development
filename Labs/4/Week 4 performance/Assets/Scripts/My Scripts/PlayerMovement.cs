using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public InputActionAsset inputAsset;

    [Range(1f, 20f)]
    public float movementSpeed = 5f;

    private void OnEnable()
    {
        if (inputAsset != null)
        {
            var moveMap = inputAsset.FindActionMap("Player");
            if (moveMap != null) moveMap.Enable();
        }
    }

    private void OnDisable()
    {
        if (inputAsset != null)
        {
            var moveMap = inputAsset.FindActionMap("Player");
            if (moveMap != null) moveMap.Disable();
        }
    }

    void Update()
    {
        // ==========================================
        // UNOPTIMISED SIN #1: REPEATED LOCAL COMPONENTS
        // ==========================================
        // Instead of caching this in Start(), we force Unity's internal engine backend to look up
        // and fetch the component on this GameObject 60+ times a second. It fixes the global bug 
        // but remains incredibly inefficient.
        Rigidbody2D rb2D = GetComponent<Rigidbody2D>();

        if (rb2D == null || inputAsset == null) return;

        // UNOPTIMISED SIN #2: STRING LOOKUPS IN UPDATE
        InputActionMap moveMap = inputAsset.FindActionMap("Player");
        InputAction moveAction = moveMap != null ? moveMap.FindAction("Move") : null;

        if (moveAction == null) return;

        // UNOPTIMISED SIN #3: MEMORY BOXING
        object rawVector = moveAction.ReadValueAsObject();

        float inputX = 0f;
        float inputY = 0f;

        if (rawVector != null && rawVector is Vector2)
        {
            Vector2 defaultVector = (Vector2)rawVector;
            inputX = defaultVector.x;
            inputY = defaultVector.y;
        }

        // UNOPTIMISED SIN #4: GARBAGE COLLECTOR HELL
        string debugInput = string.Format("Current Input: ({0}, {1}) | Frame Time: {2}", inputX, inputY, Time.deltaTime);
        Debug.Log(debugInput);

        // UNOPTIMISED SIN #5: OVERKILL 3D MATRIX MATH
        Matrix4x4 matrix = transform.localToWorldMatrix;
        Vector4 posVector = matrix.GetColumn(3);

        posVector.x += inputX * movementSpeed * Time.deltaTime;
        posVector.y += inputY * movementSpeed * Time.deltaTime;

        // UNOPTIMISED SIN #6: BREAKING PHYSICS LOOPS
        rb2D.position = new Vector2(posVector.x, posVector.y);
    }
}
