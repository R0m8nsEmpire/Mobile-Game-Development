using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Range(1f, 20f)]
    public float moveSpeed = 3f;
    Rigidbody2D rb2D;
    private Camera cam;
    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        GameObject camObj = GameObject.Find("Main Camera");
        cam = camObj.GetComponent<Camera>();
    }
    void Update()
    {
        if (rb2D != null)
        {
            rb2D.linearVelocityX = -moveSpeed;
        }
        Vector3 screenPos = cam.WorldToScreenPoint(transform.position);
        if (screenPos.x < -100f)
        {
            Destroy(gameObject);
        }
    }
}
