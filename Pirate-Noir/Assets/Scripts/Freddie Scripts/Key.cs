using UnityEngine;

public class Key : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.Keys++; // Increment the player's key count
                Debug.Log("Player picked up a key! Total keys: " + playerStats.Keys);
                Destroy(gameObject); // Destroy the key object after pickup
            }
        }
    }
}
