using UnityEngine;

public class Enemy_Collision : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Missile_Spawner.instance.targetedEnemies.Contains(gameObject))
        {
            Missile_Spawner.instance.targetedEnemies.Remove(gameObject);
        }
        //If the player hits something then disable it
        gameObject.SetActive(false);
    }
}
