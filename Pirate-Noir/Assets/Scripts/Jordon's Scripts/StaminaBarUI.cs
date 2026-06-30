using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class StaminaBarUI : MonoBehaviour
{
    [Header("Stamina Bar UI")]
    public RectTransform staminaBar; //to shrink the bar when stamina is being lost
    public PlayerStats playerStats; // Reference to the PlayerStats component to access stamina values
    public PauseManagement PauseManag; // Reference to the PauseManagement component to check if the game is paused
    public float fadeSpeed = 5f; // How long it takes for the sprint bar to fade in and out

    [Header("Stamina Bar fade in and out for menus")]
    public float fadeDuration = 0.3f; // How many seconds the fade takes
    private bool isMenuFading = false;


    [Header("Transparency")]
    public CanvasGroup vis; //to turn off and on depending 
    
    void Start()
    {
        playerStats = Object.FindAnyObjectByType<PlayerStats>(); // Get the PlayerStats component
        PauseManag = Object.FindAnyObjectByType<PauseManagement>(); // Get the PauseManagement component
        vis = GetComponent<CanvasGroup>(); // Get the canvasgroup component for sprint bar visibility control
        GameObject StaminaBar = GameObject.Find("SprintBar"); // Get the stamina bar component 
        if (StaminaBar != null) staminaBar = StaminaBar.GetComponent<RectTransform>(); // check if its there
        vis.alpha = 0f; // Start with the stamina bar invisible
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (playerStats.MaxStamina > 0)
        {
            float targetXScale = playerStats.CurrentStamina / playerStats.MaxStamina;
            if (targetXScale > 0f) 
            {
                staminaBar.localScale = new Vector3(targetXScale, 1f, 1f);
            } 
        }
        if (playerStats.CurrentStamina < 20f) // If stamina is below 20%, make the bar Grey
        {
            staminaBar.GetComponent<Image>().color = Color.grey;
        }
        else // Otherwise, keep it White
        {
            staminaBar.GetComponent<Image>().color = Color.white;
        }


        if (!isMenuFading)
        {
            float targetAlpha = (playerStats.CurrentStamina < playerStats.MaxStamina && PauseManag.GameIsPaused == false) ? 1f : 0f;
            vis.alpha = Mathf.MoveTowards(vis.alpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
        }
    }

    public IEnumerator FadeInSprint()
    {
        isMenuFading = true;
        float elapsedTime = 0f;
        float startOpacity = vis.alpha; // Where are we starting from? (will be 0 here)
        float targetOpacity = 1f;               // Desired value

        // Keep looping as long as we haven't reached our target duration
        while (elapsedTime < fadeDuration)
        {
            // Add the time passed since the last frame to see if its passed the cooldown
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate our progress percentage (between 0.0 and 1.0)
            float percentage = elapsedTime / fadeDuration;

            // Set the opacity based on that progress
            vis.alpha = Mathf.Lerp(startOpacity, targetOpacity, percentage);

            // Wait for the very next frame before continuing the loop
            yield return null;
        }

        // This is to make sure it doesn't do any fancy decimils and is a perfect int at the end of the fade
        vis.alpha = targetOpacity;

        isMenuFading = false;

    }
    public IEnumerator FadeOutSprint()
    {
        isMenuFading = true;
        
        float elapsedTime = 0f;
        float startOpacity = vis.alpha; // Where are we starting from? (will be 0 here)
        float targetOpacity = 0f;               // Desired value

        // Keep looping as long as we haven't reached our target duration
        while (elapsedTime < fadeDuration)
        {
            // Add the time passed since the last frame to see if its passed the cooldown
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate our progress percentage (between 0.0 and 1.0)
            float percentage = elapsedTime / fadeDuration;

            // Set the opacity based on that progress
            vis.alpha = Mathf.Lerp(startOpacity, targetOpacity, percentage);

            // Wait for the very next frame before continuing the loop
            yield return null;
        };

        // This is to make sure it doesn't do any fancy decimils and is a perfect int at the end of the fade
        vis.alpha = targetOpacity;

        isMenuFading = false;
        
    }


}
