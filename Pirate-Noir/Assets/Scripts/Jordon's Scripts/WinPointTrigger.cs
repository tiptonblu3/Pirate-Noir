using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class WinPointTrigger : MonoBehaviour
{
    [SerializeField] private WinEndManag win;
    

    private bool hasWon = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        // Stops the player from triggering the win screen multiple times.
        if (hasWon)
        {
            return;
        }

        // Only the player can activate the win trigger
        if (other.CompareTag("Player"))
        {
            // The player has won the game and triggered the win screen
            hasWon = true;
            // Show Win UI
            win.WinGameFunc();
        }
    }
}
