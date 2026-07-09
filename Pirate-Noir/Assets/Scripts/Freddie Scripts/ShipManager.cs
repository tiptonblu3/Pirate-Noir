using UnityEngine;
using System.Collections;

public class ShipManager : MonoBehaviour
{
    public EnemyWaves EnemyWaves;  // Reference to the EnemyWaves script
    public ShipSink ShipSink;  // Reference to the ShipSink script
    public GameObject NextShip;  // Reference to the ship prefab

    public float SinkDelay = 12f;  // Delay before sinking the ship
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (EnemyWaves.wavesCompleted)
        {
            // Enable the next ship prefab when the wave is cleared
            StartCoroutine(SpawnNextShip());
            StartCoroutine(SinkShip());
        }
    }

    IEnumerator SpawnNextShip()
    {
        // Wait for 2 seconds before spawning the next ship
        yield return new WaitForSeconds(2f);

        // Enable the next ship prefab
        NextShip.SetActive(true);
    }
    IEnumerator SinkShip()
    {
        // Wait for 2 seconds before spawning the next ship
        yield return new WaitForSeconds(SinkDelay);   

        // Sink this ship
        ShipSink.ShouldSink = true;
    }
}
