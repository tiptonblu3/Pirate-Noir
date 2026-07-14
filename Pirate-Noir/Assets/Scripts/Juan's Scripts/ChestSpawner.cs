using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class ChestSpawner : MonoBehaviour
{
    public GameObject chest;
    public int numberOfChests;
    public bool spawn;
    
    public EnemyWaves enemyWaves;

    public Transform player;

    public float distance;

    public int areaMask = NavMesh.AllAreas;

    public Vector3 spawnAreaSize = new Vector3(20f, 0f, 20f); //defines an area of a specific size around the ship

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Distance = (this.transform.position - player.position).sqrMagnitude; // checks distance 

        if(enemyWaves.wavesCompleted && !spawn) // if enemy waves are defeated and items haven't spawned
        {
            spawn = true; // spawn is true, prevents constant checking and running of the program
            SpawnChests(transform.position); //spawns chests
        }
    }

    public void SpawnChests(Vector3 position)
    {
        numberOfChests = Random.Range (1,5);
        NavMeshHit hit;

        for(int i = 0; i < numberOfChests; i++)
        {
           Vector3 randomPoint = transform.position + new Vector3(Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f), 0, Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f));
           // instead of making the chest be spawned by the transform position, make the x and z positions be randomized so it can spawn around different parts of the ship.
           if (NavMesh.SamplePosition(randomPoint, out hit, 10f, areaMask))
            {
                Instantiate(chest, hit.position, Quaternion.identity);
            } 
        }
        
    }
}
