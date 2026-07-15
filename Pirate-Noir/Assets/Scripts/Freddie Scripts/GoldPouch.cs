using UnityEngine;

public class GoldPouch : MonoBehaviour
{
    public int GoldAmount = 100; // Amount of gold in the pouch
    public int GoldRangeMin = 50; // Minimum amount of gold in the pouch
    public int GoldRangeMax = 250; // Maximum amount of gold in the pouch

    void Start()
    {
        // Assign a random gold amount within the specified range
        GoldAmount = Random.Range(GoldRangeMin, GoldRangeMax + 1);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.Gold += GoldAmount; // Increment the player's gold count
                Debug.Log("Player picked up a gold pouch! Total gold: " + playerStats.Gold);
                Destroy(gameObject); // Destroy the gold pouch object after pickup
            }
        }
    }
}
