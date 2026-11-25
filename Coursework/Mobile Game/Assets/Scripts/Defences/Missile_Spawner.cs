using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
public class Missile_Spawner : MonoBehaviour
{
    //Make an instance
    static public Missile_Spawner instance;
    //How often to spawn missiles
    public float spawnInterval;
    //How many missiles to spawn
    private int amountToSpawn = 20;
    //Missile prefab
    [SerializeField] GameObject missilePrefab;
    public List<GameObject> missile = new List<GameObject>();
    static public List<GameObject> targetedEnemies = new List<GameObject>();
    //Places for missiles to spawn
    [SerializeField] Transform[] placesToSpawn;
    //Spawn missiles Coroutine
    public Coroutine spawnMissilesCoroutine;
    private void Awake()
    {
        //Make this an instance
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
        //Object pool missiles
        for (int i = 0; i < amountToSpawn; i++)
        {
            GameObject obj = Instantiate(missilePrefab);
            obj.transform.parent = transform;
            obj.transform.localPosition = Vector3.zero;
            obj.SetActive(false);
            missile.Add(obj);
        }
    }
    public void StartSpawningMissiles()
    {
        Debug.Log("Start spawning missiles");
        //Start the spawning coroutine
        if(spawnMissilesCoroutine == null)
        {
            spawnMissilesCoroutine = StartCoroutine(SpawnMissiles());
        }
    }
    IEnumerator SpawnMissiles()
    {
        //Always repeat
        while (true)
        {
            //Wait for a set amount of time
            yield return new WaitForSeconds(spawnInterval);
            //Make a local gameobject for the enemy target
            GameObject target = GetTarget();

            //Check if the target is not null
            if (target != null)
            {
                //Mark the target as now being targeted
                targetedEnemies.Add(target.gameObject);
                //Get a missile from the object pool
                GameObject missile = GetMissile();
                //Set the position to a random spawn pos
                missile.transform.localPosition = GetSpawnLocation().localPosition;
                //Give the missile a target and enable it
                missile.GetComponent<Missile_Defence>().target = target;
                missile.SetActive(true);
            }
            yield return null;
        }
    }
    //Get a missile from the object pool
    private GameObject GetMissile()
    {
        for (int i = 0; i < missile.Count; i++)
        {
            if (!missile[i].activeInHierarchy)
            {
                return missile[i];
            }
        }
        return null;
    }
    //Get a random spawn pos from the array of places to spawn from
    private Transform GetSpawnLocation()
    {
        int rand = Random.Range(0, placesToSpawn.Length);
        Transform spawnHere = placesToSpawn[rand];
        return spawnHere;
    }
    //Get a target
    public GameObject GetTarget()
    {
        for (int i = 0; i < EnemySpawner.instance.enemyType1.Count; i++)
        {
            if (EnemySpawner.instance.enemyType1[i].activeInHierarchy && !targetedEnemies.Contains(EnemySpawner.instance.enemyType1[i]))
            {
                return EnemySpawner.instance.enemyType1[i];
            }
        }
        return null;
    }
}