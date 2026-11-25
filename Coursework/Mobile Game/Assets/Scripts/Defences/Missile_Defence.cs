using System.Collections.Generic;
using UnityEngine;

public class Missile_Defence : MonoBehaviour
{
    public GameObject target;
    private void Update()
    {
        if(target != null || target.activeInHierarchy)
        {
            //Move towards the target
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime / 2);
            //Rotate towards the target
            Vector2 direction = (target.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));
        }
        else
        {
            //If it has no target then get a new one
            target = Missile_Spawner.instance.GetTarget();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //If hits the player then disable everything
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //remove the target from the targeted enemies list
            Missile_Spawner.targetedEnemies.Remove(collision.gameObject);
            collision.gameObject.SetActive(false);
            target = null;
            gameObject.SetActive(false);
        }
    }
}
