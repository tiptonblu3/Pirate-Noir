using UnityEngine;

public class BreakableOBJ : MonoBehaviour, IInteractable
{
    public float Health = 20f; // Health of the object
    public PlayerStats Stats;
    public GameObject Player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player"); // Find the player GameObject by tag
        Stats = Player.GetComponent<PlayerStats>(); // Get the PlayerStats component from the player Game
    }

    public void Interact()
    {
        Debug.Log("Player Hit!"); // Log interaction for debugging
        Health -= Stats.AttackPower; // Reduce Object health by player's attack power
        Debug.Log($"Object Health: {Health}"); // Log object health for debugging
        if (Health <= 0) // Check if the object is defeated
        {
            Debug.Log("Object Defeated!"); // Log enemy defeat for debugging
            Destroy(this.gameObject); // Destroy the GameObject
        }
    }
}
