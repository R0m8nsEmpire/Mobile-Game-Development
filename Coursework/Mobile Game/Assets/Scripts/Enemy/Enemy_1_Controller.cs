using UnityEngine;
using System.Collections.Generic;

public class Enemy_1_Controller : MonoBehaviour
{
    public float spawnDistanceMin = 6;
    public float spawnDistanceMax = 6;
    [ContextMenu("Spawn Enemy Type 1")]
    public void SpawnEnemy()
    {
        GameObject enemy = EnemySpawner.instance.GetEnemyType1Objects();
        if(enemy != null)
        {
            float randomX = RandomSpawnPos(-spawnDistanceMin, -spawnDistanceMax, spawnDistanceMin, spawnDistanceMax);
            float randomY = RandomSpawnPos(-spawnDistanceMin, -spawnDistanceMax, spawnDistanceMin, spawnDistanceMax);
            enemy.transform.position = new Vector2(randomX, randomY);
            enemy.SetActive(true);
        }
    }
    void Update()
    {
        List<GameObject> enemies = EnemySpawner.instance.enemyType1;
        foreach(var enemy in enemies)
        {
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, new Vector3(0, 0, 0), 0.01f);
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
}
