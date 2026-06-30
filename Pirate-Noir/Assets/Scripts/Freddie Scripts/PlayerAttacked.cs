using UnityEngine;

public class PlayerAttacked : MonoBehaviour, IInteractable
{
    public Enemy Enemy;
    public PlayerStats Stats;
    public GameObject Player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Enemy = GetComponent<Enemy>(); // Get the Enemy component from the current GameObject
        Player = GameObject.FindGameObjectWithTag("Player"); // Find the player GameObject by tag
        Stats = Player.GetComponent<PlayerStats>(); // Get the PlayerStats component from the player Game
    }

    public void Interact()
    {
        Debug.Log("Player Hit!"); // Log interaction for debugging
        Enemy.health -= Stats.AttackPower; // Reduce enemy health by player's attack power
        Debug.Log($"Enemy Health: {Enemy.health}"); // Log enemy health for debugging
        
        /*if (Enemy.health <= 0) // Check if the enemy is defeated
        {
            Debug.Log("Enemy Defeated!"); // Log enemy defeat for debugging
            Destroy(Enemy.gameObject); // Destroy the enemy GameObject
        }*/
    }
}
