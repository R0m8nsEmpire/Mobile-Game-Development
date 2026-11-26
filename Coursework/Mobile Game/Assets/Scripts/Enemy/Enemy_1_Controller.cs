using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_1_Controller : MonoBehaviour
{
    public float spawnInterval;
    //Spawn an enemy from the inspector
    [ContextMenu("Spawn Enemy Type 1")]
    public void SpawnEnemy()
    {
        //Gen an enemy from the object pool
        GameObject enemy = EnemySpawner.instance.GetEnemyType1Objects();
        //Check to make sure it actually got one
        if(enemy != null)
        {
            //Spawn it randomly off screen
            float randomX = RandomSpawnPos(-EnemySpawner.instance.spawnDistanceX, -EnemySpawner.instance.spawnDistanceX,
                EnemySpawner.instance.spawnDistanceX, EnemySpawner.instance.spawnDistanceX);
            float randomY = RandomSpawnPos(-EnemySpawner.instance.spawnDistanceY, -EnemySpawner.instance.spawnDistanceY,
                EnemySpawner.instance.spawnDistanceY, EnemySpawner.instance.spawnDistanceY);
            enemy.transform.position = new Vector2(randomX, randomY);
            enemy.SetActive(true);
        }
    }
    private void Start()
    {
        //Start spawning enemies
        StartCoroutine(spawnEnemies());
    }
    void Update()
    {
        //Move all active enemies to 0 0, (the planets position)
        List<GameObject> enemies = EnemySpawner.instance.enemyType1;
        foreach(var enemy in enemies)
        {
            if (enemy.activeInHierarchy)
            {
                enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, new Vector3(0, 0, 0), Time.deltaTime / 2);
            }
        }
    }
    //Randomly spawn an enemy within these bounds
    private float RandomSpawnPos(float min1, float max1, float min2, float max2)
    {
        if(Random.value < 0.5f)
        {
            return Random.Range(min1, max1);
        }
        else
        {
            return Random.Range(min2, max2);
        }
    }
    //Spawn an enemy coroutine
    private IEnumerator spawnEnemies()
    {
        while (true)
        {
            //Spawn an enemy every 5 seconds
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
            yield return null;
        }
    }
}
