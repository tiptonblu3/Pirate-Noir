using UnityEngine;
// Nav-Mesh
using UnityEngine.AI;

public class RandomGenerator : MonoBehaviour
{

#region Inspector

///
/// Empty space to organize region headers
///


#region Player

    [Header("")]
    [Header("Player Target")]
    [Header("")]
    public Transform playerTransform;

#endregion

#region Loot

    [Header("")]
    [Header("Loot Settings")]
    [Header("")]
    [Tooltip("Loot chests or items prefabs go here.")]
    public GameObject[] lootPrefabs;
    // Range of loot to spawn.
    public int minLootCount = 2;
    public int maxLootCount = 6;
    // Plan on changing this to change the rotation bassed off the nearest wall
    [Tooltip("The Z-axis rotation angles you want your loot to face (e.g., 0 = North, 90 = East, 180 = South, 270 = West).")]
    public float targetYRotation = 0f;
    [Header("")]
    [Header("Loot Snapping")]
    [Header("")]
    [Tooltip("Set this to the layers where loot can be placed. Layers: Floor, Walkable")]
    public LayerMask floorLayer;
    [Tooltip("Slight upward adjustment to pull the loot out of the ground if its pivot point is centered.")]
    public float lootYOffset = 0.5f;

#endregion

#region Enemy

    [Header("")]
    [Header("Enemy Prefabs")]
    [Header("")]
    [Tooltip("Normal, base enemy prefab.")]
    public GameObject enemyEasy;
    [Tooltip("Ranged enemy prefab.")]
    public GameObject enemyMedium;
    [Tooltip("Big enemy prefab.")]
    public GameObject enemyHard;
    [Header("")]
    [Header("Spawn Area Bounds")]
    [Header("")]
    public Vector3 minBounds;
    public Vector3 maxBounds;
    [Header("")]
    [Header("Enemy NavMesh Settings")]
    [Header("")]
    [Tooltip("How far from the random point Unity will search for a valid NavMesh floor.")]
    public float maxNavMeshSearchRange = 5f;
    [Header("")]
    [Header("Anti-Clipping Settings")]
    [Header("")]
    [Tooltip("Include Environment, Walls, Obstacles, and Enemies here. Layers: Player, Enemy, Stairs, Water, NonWalkable. ")]
    public LayerMask clippingObstacleLayers;
    [Tooltip("Size of loot item (Width, Height, Depth).")]
    public Vector3 lootPhysicalSize = new Vector3(1.3f, 0.9f, 0.92f);
    [Tooltip("Maximum attempts the loop will make to find a clear spot before giving up on a piece of loot.")]
    public int maxSpawnAttempts = 3;

#endregion

#region References

    // On the final ship, we want to spawn more enemies.
    // This line will get the value of the finalPlatform variable from the PlatformProgression script, which is true if this is the final ship.
    [Header("")]
    [Header("References")]
    [Header("")]
    [SerializeField] private PlatformProgression finalPlatform;

#endregion
#endregion

#region Variables

    // There will always be at least 4 enemies
    private int minEnemies = 4;

#endregion
#region Start

    private void Start()
    {
        // Make sure finalPlatform is assigned in the inspector
        if (finalPlatform == null)
        {
            Debug.LogError("finalPlatform reference is not assigned in the inspector. Assign it in the Enemy Generator Manager GameObject.");
            return;
        }

        // Minimum enemies increases to 6 on the last ship
        if (finalPlatform.FinalPlatform)
        {
            minEnemies += 2;
        }
        // Will add a future addition to spawn max 8 enemies when on the first ship, and max 10 enemies after.
        // Randomly spawn between minEnemies and 10 enemies
        int enemiesToSpawn = Random.Range(minEnemies, 11);
        Debug.Log("Spawning " + enemiesToSpawn + " enemies");

        // Spawning Loot
        // maxLootCount is exclusive
        int lootToSpawn = Random.Range(minLootCount, maxLootCount + 1);
        Debug.Log("Spawning " + lootToSpawn + " loot items");
        SpawnLoot(lootToSpawn);

        // Call the method to spawn enemies
        SpawnEnemies(enemiesToSpawn);
    }
#endregion

#region Enemy Spawning

    private void SpawnEnemies(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            // Calculate a random coordinate
            float x = Random.Range(minBounds.x, maxBounds.x);
            float y = Random.Range(minBounds.y, maxBounds.y);
            float z = Random.Range(minBounds.z, maxBounds.z);
            Vector3 rawRandomPosition = new Vector3(x, y, z);

            // Get NavMesh position for the random point
            if (TryGetNavMeshPosition(rawRandomPosition, out Vector3 validNavMeshPosition))
            {
                if (enemyEasy != null)
                {
                    // Spawn the enemy
                    GameObject spawnedEnemy = Instantiate(enemyEasy, validNavMeshPosition, Random.rotation);

                    // Grab the Enemy script component off the newly spawned enemy
                    Enemy enemyScript = spawnedEnemy.GetComponent<Enemy>();

                    if (enemyScript != null)
                    {
                        // Add the player reference automatically to the enemy in the inspector
                        enemyScript.Player = playerTransform;
                    }
                }
            }
            else
            {
                // If the spawn point was outside the map, run the loop iteration again
                Debug.LogWarning("Generated point " + rawRandomPosition + " was too far from the NavMesh. Skipping or retrying.");
            }
        }
    }
    private bool TryGetNavMeshPosition(Vector3 targetPosition, out Vector3 finalPosition)
    {
        // NavMeshHit holds the data of the found location
        NavMeshHit hit;

        // NavMesh.AllAreas tells Unity to look at any walkable surface type
        if (NavMesh.SamplePosition(targetPosition, out hit, maxNavMeshSearchRange, NavMesh.AllAreas))
        {
            // Return the coordinates found
            finalPosition = hit.position;
            return true;
        }

        // Return the coordinates as zero if no valid position was found, and return false to indicate failure
        finalPosition = Vector3.zero;
        return false;
    }
#endregion

#region Loot Spawning

    private void SpawnLoot(int amount)
    {
        // Check if lootPrefabs array is empty or null
        if (lootPrefabs == null || lootPrefabs.Length == 0)
        {
            Debug.LogWarning("No loot prefabs assigned in the Loot Prefabs array!");
            return;
        }

        // Loop to spawn the specified amount of loot
        for (int i = 0; i < amount; i++)
        {
            bool successfullySpawned = false;
            int attempts = 0;

            // Find a valid spawn position for the loot
            while (!successfullySpawned && attempts < maxSpawnAttempts)
            {
                // Increment the attempts counter
                attempts++;

                // Generate random X and Z coordinates within the defined bounds
                float x = Random.Range(minBounds.x, maxBounds.x);
                float z = Random.Range(minBounds.z, maxBounds.z);

                // Start the raycast from a point above the maximum Y bound to ensure it hits the floor
                float highYStart = maxBounds.y + 20f;
                Vector3 raycastOrigin = new Vector3(x, highYStart, z);
                RaycastHit hit;
                // Calculate the maximum distance for the raycast to ensure it reaches the floor
                float maxRayDistance = (highYStart - minBounds.y) + 30f;

                // Perform the raycast to find the floor position
                if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, maxRayDistance, floorLayer))
                {
                    Vector3 calculatedPosition = hit.point;
                    // Adjust the Y position to account for the loot's offset
                    calculatedPosition.y += lootYOffset;
                    // Divide the size by 2 because OverlapBox checks extents(half-extents from the center)
                    Vector3 boxHalfExtents = lootPhysicalSize / 2f;
                    Quaternion specificRotation = Quaternion.Euler(0f, targetYRotation, 0f);

                    // This checks a physical box footprint before spawning
                    Collider[] clippingColliders = Physics.OverlapBox(
                        calculatedPosition,
                        boxHalfExtents,
                        specificRotation,
                        clippingObstacleLayers
                    );

                    // If the array length is 0, the space is completely clear of walls or obstacles
                    if (clippingColliders.Length == 0)
                    {
                        int randomLootIndex = Random.Range(0, lootPrefabs.Length);
                        GameObject chosenLootPrefab = lootPrefabs[randomLootIndex];

                        if (chosenLootPrefab != null)
                        {
                            // Loot spawned
                            Instantiate(chosenLootPrefab, calculatedPosition, specificRotation);
                            successfullySpawned = true;
                        }
                    }
                }
            }

            if (!successfullySpawned)
            {
                Debug.LogWarning($"Could not find a clip-free spot for loot item {i} after {maxSpawnAttempts} attempts.");
            }
        }
    }

}
#endregion
