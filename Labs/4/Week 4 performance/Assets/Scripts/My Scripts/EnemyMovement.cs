using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Range(1f, 20f)]
    public float moveSpeed = 3f;

    void Update()
    {
        // ==========================================
        // UNOPTIMISED SIN #1: REPEATED LOCAL COMPONENTS
        // ==========================================
        // Fixed to fetch *this specific* enemy's Rigidbody, preventing them from freezing 
        // when new clones spawn, while still wasting CPU cycles on unnecessary lookups.
        Rigidbody2D rb2D = GetComponent<Rigidbody2D>();

        if (rb2D != null)
        {
            // UNOPTIMISED SIN #2: MATRIX MATH OVERKILL
            Matrix4x4 matrix = transform.localToWorldMatrix;
            Vector4 currentPos = matrix.GetColumn(3);

            currentPos.x -= moveSpeed * Time.deltaTime;

            rb2D.position = new Vector2(currentPos.x, currentPos.y);
        }

        // UNOPTIMISED SIN #3: SCREEN BOUNDS SCENE CHECK
        GameObject camObj = GameObject.Find("Main Camera");
        if (camObj != null)
        {
            Camera cam = camObj.GetComponent<Camera>();
            Vector3 screenPos = cam.WorldToScreenPoint(transform.position);

            if (screenPos.x < -100f)
            {
                Destroy(gameObject);
            }
        }
    }
}
