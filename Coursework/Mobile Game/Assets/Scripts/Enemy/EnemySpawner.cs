using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    //Singleton instance
    public static EnemySpawner instance;
    //How many enemies of each to spawn
    private int amountToSpawn = 20;
    //Enemy type 1
    [SerializeField] GameObject enemy1Prefab;
    public List<GameObject> enemyType1 = new List<GameObject>();
    //Camera game object
    public GameObject camObj;
    //Screen Size
    [HideInInspector]
    public float spawnDistanceX = 6;
    public float spawnDistanceY = 6;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {

        for (int i = 0; i < amountToSpawn; i++)
        {
            GameObject obj = Instantiate(enemy1Prefab);
            obj.SetActive(false);
            enemyType1.Add(obj);
        }

        Camera cam = camObj.GetComponent<Camera>();
        spawnDistanceY = cam.orthographicSize + 1;
        spawnDistanceX = (cam.orthographicSize * cam.aspect) + 1;
    }
    public GameObject GetEnemyType1Objects()
    {
        for (int i = 0; i < enemyType1.Count; i++)
        {
            if (!enemyType1[i].activeInHierarchy)
            {
                return enemyType1[i];
            }
        }
        return null;
    }

}
