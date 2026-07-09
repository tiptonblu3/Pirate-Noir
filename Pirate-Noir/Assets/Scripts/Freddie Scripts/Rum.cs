using UnityEngine;

public class Rum : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.Rum++; // Increment the player's rum count
                Debug.Log("Player picked up rum! Total rum: " + playerStats.Rum);
                Destroy(gameObject); // Destroy the rum object after pickup
            }
        }
    }
}
