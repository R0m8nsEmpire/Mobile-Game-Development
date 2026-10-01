using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public float movingSpeed;
    [SerializeField] Renderer backgroundRenderer;

    private void Update()
    {
        //Scroll the background texture based on the moving speed and PlayerMovement background wall speed
        backgroundRenderer.material.mainTextureOffset += new Vector2((movingSpeed * PlayerMovement.backgroundWallSpeed) * Time.deltaTime, 0);
    }
}
