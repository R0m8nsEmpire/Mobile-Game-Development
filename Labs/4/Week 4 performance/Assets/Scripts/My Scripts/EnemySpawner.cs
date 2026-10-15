using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public float spawnInterval = 2.0f;
    private float spawnTimer = 0f;

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;

            // ==========================================
            // UNOPTIMISED SIN #1: ENGINE SCREEN SCANNING
            // ==========================================
            // Instead of caching camera bounds once at startup, we look up the Main Camera by string 
            // and perform screen-to-world projections every time a timer ticks.
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj != null)
            {
                Camera cam = camObj.GetComponent<Camera>();

                // Calculate the exact right-hand edge of the screen in world space coordinates
                Vector3 rightEdgeScreen = new Vector3(Screen.width + 100f, 0f, 0f);
                float spawnX = cam.ScreenToWorldPoint(rightEdgeScreen).x;

                // UNOPTIMISED SIN #2: DYNAMIC CALCULATION OF VERTICAL BOUNDARIES
                // Calculate the world space positions for the absolute bottom and top edges of your view
                float spawnYMin = cam.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).y;
                float spawnYMax = cam.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)).y;

                // Pick a random Y position between the bottom and top boundaries
                float randomSpawnY = Random.Range(spawnYMin, spawnYMax);

                // Create the final coordinate vector
                Vector3 dynamicSpawnPosition = new Vector3(spawnX, randomSpawnY, 0f);

                // ==========================================
                // UNOPTIMISED SIN #3: OBJECT CHURNING & GC CACHE BREAKS
                // ==========================================
                // Creates memory clutter by instantiating directly onto the heap without an Object Pool.
                GameObject newEnemy = Instantiate(enemyPrefab, dynamicSpawnPosition, Quaternion.identity);

                // Allocates temporary strings on the garbage collector timeline every single spawn.
                newEnemy.name = "Enemy_Clone_" + Random.Range(0, 100000);
            }
        }
    }
}
