using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    private PlayerStats Stats;
    private GameObject Player;

    [Header("Auto Assigned Chest Settings")]
    public int Points;

    [Header("Locked Chest Settings")]
    public bool Locked = false; // Indicates if the chest is locked
    public int GoldAmountLockedMin = 500; // Minimum amount of gold in the chest if it's locked
    public int GoldAmountLockedMax = 2000; // Maximum amount of gold in the chest if it's locked
    [Header("Unlocked Chest Settings")]
    public int GoldAmountMin = 100; // Minimum amount of gold in the chest
    public int GoldAmountMax = 500; // Maximum amount of gold in the chest

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player"); // Find the player GameObject by tag
        Stats = Player.GetComponent<PlayerStats>(); // Get the PlayerStats component from the player Game

        if (Locked)
        {
            Points = Random.Range(GoldAmountLockedMin, GoldAmountLockedMax + 1); // Assign a random point value between the locked gold amounts
        }
        else
        {
            Points = Random.Range(GoldAmountMin, GoldAmountMax + 1); // Assign a random point value between the unlocked gold amounts
        }
    }

    public void Interact()
    {
        if (Locked)
        {
            Debug.Log("Chest is locked. Player needs a key to open it.");
            if (Stats.Keys > 0)
            {
                Stats.Keys--; // Use a key to unlock the chest
                Locked = false; // Unlock the chest
                Debug.Log("Chest unlocked! Player used a key.");
                Stats.Gold += Points; // Add the points to the player's gold
                gameObject.SetActive(false); // disable this game object after interaction
            }
            else
            {
                Debug.Log("Player does not have any keys to unlock the chest.");
            }
        }
        else
        {
            Debug.Log("Chest interacted with!");
            Stats.Gold += Points; // Add the points to the player's gold
            gameObject.SetActive(false); // disable this game object after interaction
        }
    }
}
