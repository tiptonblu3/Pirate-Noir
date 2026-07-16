using UnityEngine;
using System.Collections;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using TMPro;


public class PauseManagement : MonoBehaviour
{
    [Header("Variables")]
    public bool GameIsPaused = false;
    public bool InSettings = false;
    public string SceneToLoad;

    #region === Player Movement ===]
    public PlayerMovement Movement; // Reference to the PlayerMovement component ~F
    public WinEndManag WinEndManag; // Reference to the WinEndManag component 
    #endregion
    
    #region === Ui Transition Pieces ===
    [Header("Pause Ui Fade in & out")]
    public CanvasGroup canvasGroup; //this will be used to fade in and fade out the pause ui
    public CanvasGroup HealthcanvasGroup; //this will be used to fade in and fade out the pause ui

    public StaminaBarUI stambar;
    public HealthBar healthbar; // Reference to the HealthBar component 


    public float fadeDuration = 0.3f; // How many seconds the fade takes
    #endregion

    #region === UI Top & Bottom Pieces ===
    // variable for each height
    [Header("Framing Pieces")]
    public RectTransform TopImage;
    public RectTransform BottomImage;
    public float TOTPH = 223f; //"Target Open Top Pause Height" for the top piece
    public float TOBPH = -223f; //"Target Open Bottom Pause Height" for the bottom piece
    public float TCTPH = 56f; //"Target Closed Top Pause Height" for the top piece
    public float TCBPH = -56f; //"Target Closed Bottom Pause Height" for the bottom piece
    public float TTSH = 330f; //"Target Top Settings Height" for the bottom piece in settings mode
    public float TBSH = -330f; //"Target Bottom Settings Height" for the bottom piece in settings mode


    public GameObject PauseUI;
    public GameObject SettingsUI;
    public GameObject Crosshair;
    public GameObject firstSelectedPauseButton;
    public GameObject firstSelectedSettingsButton;
    
    [Header("Text UI")]
    public PlayerStats playerStats; 
    public TextMeshProUGUI RumUI;
    public TextMeshProUGUI GoldUI;
    public TextMeshProUGUI KeyUI;
    


    [Space(10)]
    [Header("Audio Settings")]

    private static readonly string masterVolumePref = "MasterVolumePref";
    private static readonly string musicVolumePref = "MusicVolumePref";
    private static readonly string sfxVolumePref = "SoundEffectsVolumePref";

    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxSlider;

    private float masterVolume;
    private float musicVolume;
    private float sfxVolume;

    public AudioMixer audioMixer;

    #endregion

    public void Update()
    {
        RumUI.text = $"Rum: {playerStats.Rum}";
        GoldUI.text = $"Gold: {playerStats.Gold}";
        KeyUI.text = $"Keys: {playerStats.Keys}";
    }
    private void OnValidate()
    {
        #if UNITY_EDITOR
        if (audioMixer == null)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("MasterVolume t:AudioMixer");

            if (guids.Length > 0)
            {
                // Convert the string id to a file path 
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                
                // using the previously setup path load the actual file and assign it to the inspector
                audioMixer = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Audio.AudioMixer>(path);
                
                // tell Unity to save this change in the scene/prefab
                UnityEditor.EditorUtility.SetDirty(this);
                
                Debug.Log($"Successfully auto-assigned Audio Mixer from: {path}", this);
            }
        }
        #endif
    }

    void Start()
    {
        #region === Auto Assigning ===

        PauseUI = GameObject.Find("PauseUI"); // Get the PauseUI Gameobject component 
        SettingsUI = GameObject.Find("Settings"); // Get the Settings Gameobject component 

        
        Movement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>(); // Get the PlayerMovement component from the player Gameobject ~F
        
        GameObject winend = GameObject.Find("UI"); // Get the canvas group from the gameobject PauseMenu 
            if (winend != null) WinEndManag = winend.GetComponent<WinEndManag>();

        playerStats = Object.FindAnyObjectByType<PlayerStats>(); // reference to the player's stats script to modify health
        // public PlayerStats playerStats;

        GameObject canvgroup = GameObject.Find("PauseMenu"); // Get the canvas group from the gameobject PauseMenu 
            if (canvgroup != null) canvasGroup = canvgroup.GetComponent<CanvasGroup>();

        GameObject healcanvGroup = GameObject.Find("HealthBarParent"); // Get the canvas group from the gameobject PauseMenu 
            if (healcanvGroup != null) HealthcanvasGroup = healcanvGroup.GetComponent<CanvasGroup>();

        GameObject TopImg = GameObject.Find("Top"); // Get the Top Image Rect component from the gameobject Top
            if (TopImg != null) TopImage = TopImg.GetComponent<RectTransform>(); // check if its there
        GameObject BottomImg = GameObject.Find("Bottom"); // Get the Bottom Image Rect component from the gameobject Bottom
            if (BottomImg != null) BottomImage = BottomImg.GetComponent<RectTransform>(); // check if its there


        GameObject MasVolume = GameObject.Find("Master Volume"); // Get the Master Volume Slider component 
            if (MasVolume != null) masterVolumeSlider = MasVolume.GetComponent<Slider>(); // check if its there
        GameObject MusicSlider = GameObject.Find("Music Volume"); // Get the Music Volume Slider component 
            if (MusicSlider != null) musicVolumeSlider = MusicSlider.GetComponent<Slider>(); // check if its there
        GameObject sfx = GameObject.Find("SFX Volume"); // Get the SFX Slider component 
            if (sfx != null) sfxSlider = sfx.GetComponent<Slider>(); // check if its there


        stambar = Object.FindAnyObjectByType<StaminaBarUI>(); // reference to the player's stats script to modify health
        healthbar = Object.FindAnyObjectByType<HealthBar>(); // reference to the player's stats script to modify health


        if (PauseUI != null) PauseUI.SetActive(false);
        if (SettingsUI != null) SettingsUI.SetActive(false);

        #endregion

           

            masterVolume = PlayerPrefs.GetFloat(masterVolumePref);
            musicVolume = PlayerPrefs.GetFloat(musicVolumePref);
            sfxVolume = PlayerPrefs.GetFloat(sfxVolumePref);

            // then saves the value to the sliders -
                if (masterVolumeSlider != null && audioMixer != null)
                {
                    masterVolumeSlider.value = masterVolume;
                }
                    if (musicVolumeSlider != null && audioMixer != null)
                    {
                        musicVolumeSlider.value = musicVolume;
                    }
                        if (sfxSlider != null && audioMixer != null)
                        {
                            sfxSlider.value = sfxVolume;
                        }
            // and applies it to the AudioMixer
            SetMasterVolume(masterVolume);
            SetBGMVolume(musicVolume);
            SetSFXVolume(sfxVolume);
    }

    

    #region === Pause Buttons ===

    public void Pause()
    {
        //transition to turn on opacity and allow the ui to appear for the pause area
        //move the images from the closed position to the open position for the pause area 
        //Check if images are there and make the text and buttons appear

        if (WinEndManag.EndGameState) //if the player has won or lost, don't allow them to pause the game
        {
            return;
        }
        
        StartCoroutine(stambar.FadeOutSprint());
        StartCoroutine(healthbar.FadeOutHealth());
        Crosshair.SetActive(false);


        Cursor.lockState = CursorLockMode.None; //unlock cursor so the player can click on the buttons
        Cursor.visible = true;


        StartCoroutine(FadeIn());
        Time.timeScale = 0f;
        //AudioListener.pause = false; (this is for later when we implement audio)
        StartCoroutine(MoveTop());
        StartCoroutine(MoveBottom());
        
        GameIsPaused = true;
        Movement.CanMove = false;  //Don't allow the player to move when they pause the game ~F
        
        Movement.PlayerActionAudio.Stop();
        Movement.PlayerFootstepAudio.Stop();

        EventSystem.current.SetSelectedGameObject(null); // Clear the current selected GameObject
        EventSystem.current.SetSelectedGameObject(firstSelectedPauseButton); // Set the first selected button
        
    }

    public void Resume()
    {
        //Make the text and buttons dissappear
        //move the images from the open position to the closed position for the pause area 
        //transition to turn off opacity and make the ui dissappear for the pause area

        UpdateSound();

        Cursor.lockState = CursorLockMode.Locked; //unlock cursor so the player can click on the buttons
        Cursor.visible = false;

        PauseUI.SetActive(false);
        Crosshair.SetActive(true);
        
        Time.timeScale = 1f;

        StartCoroutine(CloseTop());
        StartCoroutine(CloseBottom());

        GameIsPaused = false;


        StartCoroutine(FadeOut());


        StartCoroutine(healthbar.FadeInHealth());

        Movement.CanMove = true;  //allow the player to move again when they unpause the game ~F

        EventSystem.current.SetSelectedGameObject(null); // Clear the current selected GameObject



    }
    public void Options()
    {
        //transition to turn on opacity and allow the ui to appear for the pause area
        //move the images from the closed position to the open position for the pause area 
        //Check if images are there and make the text and buttons appear
        

        InSettings = true;
        StartCoroutine(MoveTop());
        StartCoroutine(MoveBottom());
        EventSystem.current.SetSelectedGameObject(null); // Clear the current selected GameObject
        EventSystem.current.SetSelectedGameObject(firstSelectedSettingsButton); // Set the first selected button
    }

    public void LoadSceneByName()
	{
		SceneManager.LoadScene(SceneToLoad);
        Debug.Log("Scene loaded: " + SceneToLoad);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void Apply()
    {
        
    }

    public void Back()
    {
        InSettings = false;
        StartCoroutine(SettingsMoveTop());
        StartCoroutine(SettingsMoveBottom());
        EventSystem.current.SetSelectedGameObject(null); // Clear the current selected GameObject
        EventSystem.current.SetSelectedGameObject(firstSelectedPauseButton); // Set the first selected button

    }
    #endregion

    #region === Animations for Pause ===
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        float startOpacity = canvasGroup.alpha; // Where are we starting from? (will be 0 here)
        float targetOpacity = 1f;               // Desired value

        // Keep looping as long as we haven't reached our target duration
        while (elapsedTime < fadeDuration)
        {
            // Add the time passed since the last frame to see if its passed the cooldown
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate our progress percentage (between 0.0 and 1.0)
            float percentage = elapsedTime / fadeDuration;

            // Set the opacity based on that progress
            canvasGroup.alpha = Mathf.Lerp(startOpacity, targetOpacity, percentage);

            // Wait for the very next frame before continuing the loop
            yield return null;
        }

        // This is to make sure it doesn't do any fancy decimils and is a perfect int at the end of the fade
        canvasGroup.alpha = targetOpacity;
        if (canvasGroup.alpha == 1f)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
    private IEnumerator FadeOut()
    {
        canvasGroup.interactable = false;
        float elapsedTime = 0f;
        float startOpacity = canvasGroup.alpha; // Where are we starting from? (will be 0 here)
        float targetOpacity = 0f;               // Desired value

        // Keep looping as long as we haven't reached our target duration
        while (elapsedTime < fadeDuration)
        {
            // Add the time passed since the last frame to see if its passed the cooldown
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate our progress percentage (between 0.0 and 1.0)
            float percentage = elapsedTime / fadeDuration;

            // Set the opacity based on that progress
            canvasGroup.alpha = Mathf.Lerp(startOpacity, targetOpacity, percentage);

            // Wait for the very next frame before continuing the loop
            yield return null;
        };

        // This is to make sure it doesn't do any fancy decimils and is a perfect int at the end of the fade
        canvasGroup.alpha = targetOpacity;
        if (canvasGroup.alpha == 0f)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private IEnumerator MoveTop()
    {
        if (!InSettings) //deactivate stuff
        {
            SettingsUI.SetActive(false);
        }
        else
        {
            PauseUI.SetActive(false);
        }
        float elapsedTime = 0f;
        float startPos = InSettings ? TOTPH: TCTPH; // Where are we starting from in the ui
        float targetPos = InSettings ? TTSH : TOTPH; // Desired value
        // Keep looping as long as we haven't reached our target duration
        while (elapsedTime < fadeDuration)
        {
            // Add the time passed since the last frame to see if its passed the cooldown
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate our progress percentage (between 0.0 and 1.0)
            float percentage = elapsedTime / fadeDuration;

            Vector2 currentPos = TopImage.anchoredPosition; // keep the x position
            float newY = Mathf.Lerp(startPos, targetPos, percentage);
            
            TopImage.anchoredPosition = new Vector2(currentPos.x, newY); // Set the new position

            // Wait for the very next frame before continuing the loop
            yield return null;
        }

        TopImage.anchoredPosition = new Vector2(TopImage.anchoredPosition.x, targetPos);
        
        if (!InSettings) //reactivate stuff
        {
            PauseUI.SetActive(true);
        }
        else
        {
            SettingsUI.SetActive(true);
        }
    }
    private IEnumerator MoveBottom()
    {
        float elapsedTime = 0f;
        float startPos =  InSettings ? TOBPH : TCBPH; // Where are we starting from in the ui
        float targetPos = InSettings ? TBSH : TOBPH; // Desired value
        // Keep looping as long as we haven't reached our target duration
        while (elapsedTime < fadeDuration)
        {
            // Add the time passed since the last frame to see if its passed the cooldown
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate our progress percentage (between 0.0 and 1.0)
            float percentage = elapsedTime / fadeDuration;

            Vector2 currentPos = BottomImage.anchoredPosition; // keep the x position
            float newY = Mathf.Lerp(startPos, targetPos, percentage);
            
            BottomImage.anchoredPosition = new Vector2(currentPos.x, newY); // Set the new position

            // Wait for the very next frame before continuing the loop
            yield return null;
        }

        BottomImage.anchoredPosition = new Vector2(BottomImage.anchoredPosition.x, targetPos);

        // This is to make sure it doesn't do any fancy decimals and is a perfect int at the end of the fade
        
    }
    private IEnumerator CloseTop()
    {
        if (!InSettings) //deactivate stuff
        {
            SettingsUI.SetActive(false);
        }
        else
        {
            PauseUI.SetActive(false);
        }
        float elapsedTime = 0f;
        float startPos = TOTPH; // Where are we starting from in the ui
        float targetPos = InSettings ? TOTPH : TCTPH; // Desired value
        // Keep looping as long as we haven't reached our target duration
        while (elapsedTime < fadeDuration)
        {
            // Add the time passed since the last frame to see if its passed the cooldown
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate our progress percentage (between 0.0 and 1.0)
            float percentage = elapsedTime / fadeDuration;

            Vector2 currentPos = TopImage.anchoredPosition; // keep the x position
            float newY = Mathf.Lerp(startPos, targetPos, percentage);
            
            TopImage.anchoredPosition = new Vector2(currentPos.x, newY); // Set the new position

            // Wait for the very next frame before continuing the loop
            yield return null;
        }

        TopImage.anchoredPosition = new Vector2(TopImage.anchoredPosition.x, targetPos);
        
        if (!InSettings) //reactivate stuff
        {
            PauseUI.SetActive(true);
        }
        else
        {
            SettingsUI.SetActive(true);
        }
    }
    private IEnumerator CloseBottom()
    {
        float elapsedTime = 0f;
        float startPos = TOBPH; // Where are we starting from in the ui
        float targetPos = InSettings ? TOBPH : TCBPH; // Desired value
        // Keep looping as long as we haven't reached our target duration
        while (elapsedTime < fadeDuration)
        {
            // Add the time passed since the last frame to see if its passed the cooldown
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate our progress percentage (between 0.0 and 1.0)
            float percentage = elapsedTime / fadeDuration;

            Vector2 currentPos = BottomImage.anchoredPosition; // keep the x position
            float newY = Mathf.Lerp(startPos, targetPos, percentage);
            
            BottomImage.anchoredPosition = new Vector2(currentPos.x, newY); // Set the new position

            // Wait for the very next frame before continuing the loop
            yield return null;
        }

        BottomImage.anchoredPosition = new Vector2(BottomImage.anchoredPosition.x, targetPos);

        // This is to make sure it doesn't do any fancy decimals and is a perfect int at the end of the fade
        
    }
    private IEnumerator SettingsMoveTop()
    {
        if (!InSettings) //deactivate stuff
        {
            SettingsUI.SetActive(false);
        }
        else
        {
            PauseUI.SetActive(false);
        }
        float elapsedTime = 0f;
        float startPos = TTSH; // Where are we starting from in the ui
        float targetPos = TOTPH; // Desired value
        // Keep looping as long as we haven't reached our target duration
        while (elapsedTime < fadeDuration)
        {
            // Add the time passed since the last frame to see if its passed the cooldown
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate our progress percentage (between 0.0 and 1.0)
            float percentage = elapsedTime / fadeDuration;

            Vector2 currentPos = TopImage.anchoredPosition; // keep the x position
            float newY = Mathf.Lerp(startPos, targetPos, percentage);
            
            TopImage.anchoredPosition = new Vector2(currentPos.x, newY); // Set the new position

            // Wait for the very next frame before continuing the loop
            yield return null;
        }

        TopImage.anchoredPosition = new Vector2(TopImage.anchoredPosition.x, targetPos);
        
        if (!InSettings) //reactivate stuff
        {
            PauseUI.SetActive(true);
        }
        else
        {
            SettingsUI.SetActive(true);
        }
    }
    private IEnumerator SettingsMoveBottom()
    {
        float elapsedTime = 0f;
        float startPos = TBSH; // Where are we starting from in the ui
        float targetPos = TOBPH; // Desired value
        // Keep looping as long as we haven't reached our target duration
        while (elapsedTime < fadeDuration)
        {
            // Add the time passed since the last frame to see if its passed the cooldown
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate our progress percentage (between 0.0 and 1.0)
            float percentage = elapsedTime / fadeDuration;

            Vector2 currentPos = BottomImage.anchoredPosition; // keep the x position
            float newY = Mathf.Lerp(startPos, targetPos, percentage);
            
            BottomImage.anchoredPosition = new Vector2(currentPos.x, newY); // Set the new position

            // Wait for the very next frame before continuing the loop
            yield return null;
        }

        BottomImage.anchoredPosition = new Vector2(BottomImage.anchoredPosition.x, targetPos);

        // This is to make sure it doesn't do any fancy decimals and is a perfect int at the end of the fade
        
    }
    #endregion


    #region == Settings & Audio  ==

    public void SaveSoundSettings()
    {
        PlayerPrefs.SetFloat(masterVolumePref, masterVolumeSlider.value);
        PlayerPrefs.SetFloat(musicVolumePref, musicVolumeSlider.value);
        PlayerPrefs.SetFloat(sfxVolumePref, sfxSlider.value);
        PlayerPrefs.Save();

    }

    public void UpdateSound()
    {
        SetMasterVolume(masterVolumeSlider.value);
        SetBGMVolume(musicVolumeSlider.value);
        SetSFXVolume(sfxSlider.value);
        SaveSoundSettings();
    }

    // AudioMixers will use int values for decibels insteas of just 0-100% values,
    // so you have to convert the value so the mixer will read it correctly.
    public void SetMasterVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
    }

    public void SetBGMVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
    }

    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        audioMixer.SetFloat("UIVolume", Mathf.Log10(value) * 20);

    }

    #endregion

    
}
