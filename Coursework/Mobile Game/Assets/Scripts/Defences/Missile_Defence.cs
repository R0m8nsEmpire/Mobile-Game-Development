using System.Collections.Generic;
using UnityEngine;

public class Missile_Defence : MonoBehaviour
{
    //How often to spawn missiles
    public float spawnInterval;
    //How many missiles to spawn
    private int amountToSpawn = 20;
    //Missile prefab
    [SerializeField] GameObject missilePrefab;
    public List<GameObject> missile = new List<GameObject>();
    private List<GameObject> targetedEnemies = new List<GameObject>();

    private void Start()
    {
        for (int i = 0; i < amountToSpawn; i++)
        {
            GameObject obj = Instantiate(missilePrefab);
            obj.transform.parent = transform;
            obj.transform.localPosition = Vector3.zero;
            obj.SetActive(false);
            missile.Add(obj);
        }
    }

    private void Update()
    {
        foreach (GameObject obj in missile)
        {
            if (obj.activeInHierarchy)
            {
                foreach (GameObject enemy in EnemySpawner.instance.enemyType1)
                {
                    if (enemy.activeInHierarchy && enemy.gameObject != targetedEnemies.Contains(enemy))
                    {
                        enemy.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
                    }
                }
            }
        }
    }
}
