using UnityEngine;

public class Enemy_Collision : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //If the player hits something then disable it
        gameObject.SetActive(false);
    }
}
