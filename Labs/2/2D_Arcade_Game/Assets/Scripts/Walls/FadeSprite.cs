using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeSprite : MonoBehaviour
{
    public Coroutine fading;
    // Coroutine to fade out the sprite and destroy the GameObject
    private void Update()
    {
        //Check if the player is in the MultiPlayer Scene
        Scene currentScene = SceneManager.GetActiveScene();
        if ((MultiplayerController.player1Alive == false || MultiplayerController.player2Alive == false) && currentScene.name == "MultiPlayer")
        {
            if(fading == null)
            {
                fading = StartCoroutine(FadeAndDestroy());
            }
        }
    }
    public IEnumerator FadeAndDestroy()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        //While the alpha is higher than 0
        while (sprite.color.a > 0)
        {
            //Decrease the alpha value of a new sprite color
            Color tempColor = sprite.color;
            tempColor.a -= 0.01f;
            //Apply the new color to the sprite
            sprite.color = tempColor;
            yield return null;
        }
        //Destroy the GameObject after fading
        if(transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }
        else
        {
            yield break;
        }
    }
}
