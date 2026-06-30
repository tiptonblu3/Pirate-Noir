using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class WinTrigger : MonoBehaviour
{

    [SerializeField] private GameObject winScreen;
    
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private MonoBehaviour playerController;

    private bool hasWon = false;

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
            winScreen.SetActive(true);
            // Disable player input
            playerInput.enabled = false;
            // Diable player controller
            playerController.enabled = false;
            // Unlock mouse
            Cursor.lockState = CursorLockMode.None;
            // Show mouse
            Cursor.visible = true;

            // Pause game
            Time.timeScale = 0f;
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}