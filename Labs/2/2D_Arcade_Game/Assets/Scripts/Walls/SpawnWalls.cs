using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnWalls : MonoBehaviour
{
    [Header("Spawning paramaters")]
    //spawn time wait
    public float spawnTime = 10;
    //Minimum and Maximum wall spawn height
    public float maxYHeight = 3.5f, minYHeight = -3.5f;
    //Wall prefabs
    [SerializeField] private GameObject[] walls;
    //WallSpeed
    public static float wallSpeed;

    [Header("Powerup paramaters")]
    //Powerup GameObject prefab
    [SerializeField] private GameObject timePowerup, lifePowerup;
    //Chance of spawning powerup
    public int powerupChance = 10;

    private void Start()
    {
        //Start spawning walls
        StartCoroutine(spawnWalls());
        StartCoroutine(spawnPowerups());
        wallSpeed = 5;
    }

    private IEnumerator spawnPowerups()
    {
        //Wait half of the spawn time so powerups spawn in between walls
        yield return new WaitForSeconds(spawnTime / 2);
        //Repeat this
        while (true)
        {
            yield return new WaitForSeconds(spawnTime);
            //If the player is dead stop the function
            if (PlayerMovement.playerLives <= 0 || (timePowerup == null && lifePowerup == null))
            {
                break;
            }
            //set the spawner position to a random value within the min and max Y values
            transform.position = new Vector3(transform.position.x, Random.Range(minYHeight, maxYHeight), 0);
            //pick a random number within a specific range if it is 2 then spawn a powerup
            int spawnPowerup = Random.Range(0, powerupChance);
            if(spawnPowerup == 1)
            {
                //Spawn in the powerup and apply paramaters to it
                GameObject powerupObject = Instantiate(timePowerup, transform.position, Quaternion.identity);
                powerupObject.GetComponent<Rigidbody2D>().linearVelocityX = -wallSpeed;
            }
            if (spawnPowerup == 2)
            {
                //Spawn in the powerup and apply paramaters to it
                GameObject powerupObject = Instantiate(lifePowerup, transform.position, Quaternion.identity);
                powerupObject.GetComponent<Rigidbody2D>().linearVelocityX = -wallSpeed;
            }
            yield return null;
        }
    }

    private IEnumerator spawnWalls()
    {
        //repeat this 
        while (true)
        {
            //wait a specific amount of time
            yield return new WaitForSeconds(spawnTime);
            //If the player is out of lives stop spawning walls
            Scene currentScene = SceneManager.GetActiveScene();
            if (PlayerMovement.playerLives <= 0 && currentScene.name == "SinglePlayer")
            {
                break;
            }
            else if (!MultiplayerController.player1Alive && !MultiplayerController.player2Alive && currentScene.name == "Multiplayer")
            {
                break;
            }
            //set the spawner position to a random value within the min and max Y values
            transform.position = new Vector3(transform.position.x, Random.Range(minYHeight, maxYHeight), 0);
            //Pick a random wall from the array
            int randomWallNumber = Random.Range(0, walls.Length);
            GameObject wallToSpawn = walls[randomWallNumber];
            //spawn in a wall at this position
            GameObject wall = Instantiate(wallToSpawn, transform.position, Quaternion.identity);
            //Set the wall speed of the wall to This wall speed
            WallMovement.wallSpeed = wallSpeed;
            yield return null;
        }
    }
}