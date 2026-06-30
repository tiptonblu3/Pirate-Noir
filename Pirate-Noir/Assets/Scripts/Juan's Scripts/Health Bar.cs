using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HealthBar : MonoBehaviour
{
    // function connecting player health to UI 

    public Image healthBar;
    public Image EaseHealthBar;
    public GameObject HealthUI;
    public float lerpSpeed = 0.05f;

    [Header("Health Bar fade in and out for menus")]
    public float fadeDuration = 0.3f; // How many seconds the fade takes


    /*public int maxHealth = 100;
    public int health;*/
    public PlayerStats player; // to use the health values from that code
    public PauseManagement PauseManag;


    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = Object.FindAnyObjectByType<PlayerStats>(); // Get the PlayerStats component
        GameObject mainBarObj = GameObject.Find("HealthBar");
        GameObject easeBarObj = GameObject.Find("HurtBar");

        PauseManag = Object.FindAnyObjectByType<PauseManagement>();

        if (mainBarObj != null) healthBar = mainBarObj.GetComponent<Image>();
        if (easeBarObj != null) EaseHealthBar = easeBarObj.GetComponent<Image>();


        
        player.CurrentHealth = player.MaxHealth;

        healthBar.fillAmount = 1f;
        EaseHealthBar.fillAmount = 1f;

        
    }

    // Update is called once per frame
    void Update()
    {
        float targetFill = (float)player.CurrentHealth / player.MaxHealth;

        if (healthBar.fillAmount != targetFill)
        {
            healthBar.fillAmount = targetFill;
        }

        if (EaseHealthBar.fillAmount != healthBar.fillAmount)
        {
            EaseHealthBar.fillAmount = Mathf.Lerp(EaseHealthBar.fillAmount, targetFill, Time.deltaTime * lerpSpeed); // gives cool souls like effect when losing health
            // health lost will have an extra yellow bar, which will then disappear easing into the new current health.
        }
    }


    public IEnumerator FadeInHealth()
    {
        float elapsedTime = 0f;
        float startOpacity = PauseManag.HealthcanvasGroup.alpha; // Where are we starting from? (will be 0 here)
        float targetOpacity = 1f;               // Desired value

        // Keep looping as long as we haven't reached our target duration
        while (elapsedTime < fadeDuration)
        {
            // Add the time passed since the last frame to see if its passed the cooldown
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate our progress percentage (between 0.0 and 1.0)
            float percentage = elapsedTime / fadeDuration;

            // Set the opacity based on that progress
            PauseManag.HealthcanvasGroup.alpha = Mathf.Lerp(startOpacity, targetOpacity, percentage);

            // Wait for the very next frame before continuing the loop
            yield return null;
        }

        // This is to make sure it doesn't do any fancy decimils and is a perfect int at the end of the fade
    }
    public IEnumerator FadeOutHealth()
    {
        float elapsedTime = 0f;
        float startOpacity = PauseManag.HealthcanvasGroup.alpha; // Where are we starting from? (will be 0 here)
        float targetOpacity = 0f;               // Desired value

        // Keep looping as long as we haven't reached our target duration
        while (elapsedTime < fadeDuration)
        {
            // Add the time passed since the last frame to see if its passed the cooldown
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate our progress percentage (between 0.0 and 1.0)
            float percentage = elapsedTime / fadeDuration;

            // Set the opacity based on that progress
            PauseManag.HealthcanvasGroup.alpha = Mathf.Lerp(startOpacity, targetOpacity, percentage);

            // Wait for the very next frame before continuing the loop
            yield return null;
        };

        // This is to make sure it doesn't do any fancy decimils and is a perfect int at the end of the fade
        PauseManag.HealthcanvasGroup.alpha = targetOpacity;
    }


}