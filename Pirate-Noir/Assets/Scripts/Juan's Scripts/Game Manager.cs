using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject GameOverScreen;
    public PlayerStats player; // to use the health values from that code

    public float restartDelay = 3f; // delay before restarting the game after game over, this will probably change in later versions



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player.CurrentHealth <= 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        GameOverScreen.SetActive(true); // when the player dies, the game over screen will pop up, this will probably change in later versions
        

        StartCoroutine(RestartGame()); // will restart the game after a certain amount of time, this will probably change in later versions

    }

    public IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(restartDelay);
        
        Time.timeScale = 1f; // unpause the game, this will probably change in later versions
        GameOverScreen.SetActive(false); // hide the game over screen, this will probably change
        player.CurrentHealth = player.MaxHealth; // reset player health, this will probably change in later versions

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // reload the current scene, this will probably change in later versions

    }
}
