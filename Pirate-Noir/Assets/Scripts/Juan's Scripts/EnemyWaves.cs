using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyWaves : MonoBehaviour
{
    // for all intents and purposes this is the spawner
    public Transform Player;
    public float Distance;


    public float SpawnDistance = 10f; // distance at which the enemy waves will start
    public float SpawnDistanceSqr; // squared distance for optimization, this will probably change in later versions
    public int roundNumber = 3;
    public int enemiesOnRound = 30;
    
    public int ActiveEnemies = 0;
    public int enemiesPerWave = 10;
    public int poolSize = 10;
    private int nextEnemyIndex = 0;
    
    public GameObject regularEnemy;
    public GameObject rangedEnemy;
    public GameObject bigEnemy;
    public List<GameObject> enemyWaves = new List<GameObject>();

    public int spawnLocationNumber;
    public Transform spawn1;
    public Transform spawn2; 
    public float spawnWait = 0.5f;

    public int currentRound = 0;
    
    public bool wavesCompleted = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").transform; // finds the player object and sets it to the player variable
        SpawnDistanceSqr = SpawnDistance * SpawnDistance; // calculate the squared distance for optimization

        for (int i = 0; i < poolSize; i++)
        {
            int enemyType = Random.Range(0, 3); // determine which type of enemy to spawn, this will probably change in later versions

            if (enemyType == 0)
            {
                GameObject obj = Instantiate(regularEnemy);
                obj.SetActive(false);
                enemyWaves.Add(obj); // adds enemy to the list
            }
            else if (enemyType == 1)
            {
                GameObject obj2 = Instantiate(rangedEnemy);
                obj2.SetActive(false);
                enemyWaves.Add(obj2);
            }
            else if (enemyType == 2)
            {
                GameObject obj3 = Instantiate(bigEnemy);
                obj3.SetActive(false);
                enemyWaves.Add(obj3);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Distance = (this.transform.position - Player.position).sqrMagnitude; //get the distance from player, might have a more optimized version later.

        if (Distance < SpawnDistanceSqr)
        {
            WavesProcess();
        }
    }

    public GameObject GetPooledEnemy()
    {
        for (int i = 0; i < enemyWaves.Count; i++)
        {
            int index = (nextEnemyIndex + i) % enemyWaves.Count;
            if (!enemyWaves[index].activeInHierarchy)
            {
                nextEnemyIndex = (index + i) % enemyWaves.Count;
                return enemyWaves[index];
            }
        }
        return null; // if no pooled object is available, return null, this will probably change in later versions
    }

    public void WavesProcess() //spawns the waves of enemies
    {
        if (ActiveEnemies > 0) // if not all enemies have been defeated, the next wave does not start
        {
            return;
        }

        if(currentRound >= roundNumber) // stops waves from continuing if they are equal to the selected number of rounds listed.
        {
            wavesCompleted = true;
            return;
        }

        StartCoroutine(SpawnWave());
        currentRound++;
    } 

    public IEnumerator SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            spawnLocationNumber = Random.Range(0,2);
            GameObject enemy = GetPooledEnemy();
            if (enemy != null)
            {
                if (spawnLocationNumber == 0)
                {
                    enemy.transform.position = spawn1.transform.position; // spawn the enemy at the spawner's position, this will probably change in later versions
                }
                else if (spawnLocationNumber == 1)
                {
                    enemy.transform.position = spawn2.transform.position; // spawn the enemy at the spawner's position, this will probably change in later versions
                }
                enemy.SetActive(true);
                ActiveEnemies++;
            }

            yield return new WaitForSeconds(spawnWait); // gives a little delay between enemies spawning.
        }
    }
}
