using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_1_Controller : MonoBehaviour
{

    [ContextMenu("Spawn Enemy Type 1")]
    public void SpawnEnemy()
    {
        GameObject enemy = EnemySpawner.instance.GetEnemyType1Objects();
        if(enemy != null)
        {
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
        StartCoroutine(spawnEnemies());
    }
    void Update()
    {
        List<GameObject> enemies = EnemySpawner.instance.enemyType1;
        foreach(var enemy in enemies)
        {
            if (enemy.activeInHierarchy)
            {
                enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, new Vector3(0, 0, 0), Time.deltaTime / 2);
            }
        }
    }
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

    private IEnumerator spawnEnemies()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(5);
            yield return null;
        }
    }
}
