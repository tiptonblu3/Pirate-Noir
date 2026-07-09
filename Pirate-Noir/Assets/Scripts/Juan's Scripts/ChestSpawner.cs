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
        numberOfChests = Random.Range (0,5);
        NavMeshHit hit;

        for(int i = 0; i < numberOfChests; i++)
        {
           if (NavMesh.SamplePosition(position, out hit, 10f, areaMask))
            {
                Instantiate(chest, hit.position, Quaternion.identity);
            } 
        }
        
    }
}
