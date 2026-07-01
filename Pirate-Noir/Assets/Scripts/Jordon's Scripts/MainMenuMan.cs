using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

public class MainMenuMan : MonoBehaviour
{
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxSlider;

    private float masterVolume;
    private float musicVolume;
    private float sfxVolume;

    public AudioMixer audioMixer;
    private static readonly string masterVolumePref = "MasterVolumePref";
    private static readonly string musicVolumePref = "MusicVolumePref";
    private static readonly string sfxVolumePref = "SoundEffectsVolumePref";
    private bool isInitializing = false;

    public bool ToggleMusicOn = false;
    public AudioSource MusicAS;

    public bool ToggleSFXOn = false;
    public AudioSource SFXAS;
    public RectTransform LadderUI;
    public float targetTime = 1f; // how long the transition will take
    public float targetYPosition = -1000f; // the target Y position for the UI to move to
    public GameObject ButtonsUI;

    public void Awake()
    {
        audioMixer = Resources.Load<UnityEngine.Audio.AudioMixer>("MasterVolume");
    }

    void Start()
    {
        StartCoroutine(Setups());
    }
    
    private IEnumerator Setups()
    {
        isInitializing = true;

            masterVolume = PlayerPrefs.GetFloat(masterVolumePref, 0.5f);
            musicVolume = PlayerPrefs.GetFloat(musicVolumePref, 0.5f);
            sfxVolume = PlayerPrefs.GetFloat(sfxVolumePref, 0.5f);

            yield return null;

        GameObject MasVolume = GameObject.Find("Master Volume"); // Get the Master Volume Slider component 
            if (MasVolume != null) masterVolumeSlider = MasVolume.GetComponent<Slider>(); // check if its there
        GameObject MusicSlider = GameObject.Find("Music Volume"); // Get the Music Volume Slider component 
            if (MusicSlider != null) musicVolumeSlider = MusicSlider.GetComponent<Slider>(); // check if its there
        GameObject sfx = GameObject.Find("SFX Volume"); // Get the SFX Slider component 
            if (sfx != null) sfxSlider = sfx.GetComponent<Slider>(); // check if its there

            

            // then saves the value to the sliders -
                if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;
                    if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
                        if (sfxSlider != null) sfxSlider.value = sfxVolume;

            if (audioMixer != null) // and applies it to the AudioMixer
            {   
                
                SetMasterVolume(masterVolume);
                SetBGMVolume(musicVolume);
                SetSFXVolume(sfxVolume);
            }
            Debug.Log("Sound settings loaded:\nMaster Volume: " + masterVolume + "\nMusic Volume: " + musicVolume + "\nSFX Volume: " + sfxVolume);

            isInitializing = false;

    }
    public IEnumerator AnimateMainMenu(string sceneName)
    {
        // make ui buttons dissappear
        //make all the ui background elements lerp downward the screen at a faster speed but stopping after a certain amount of time
        //then load scene by name

        ButtonsUI.SetActive(false); // Disable the buttons UI

        float elapsedTime = 0f;

        // Keep looping as long as we haven't reached our target duration
        while (elapsedTime < targetTime)
        {
            // Add the time passed since the last frame to see if its passed the cooldown
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate our progress percentage (between 0.0 and 1.0)
            float percentage = elapsedTime / targetTime;

            Vector2 currentPos = LadderUI.anchoredPosition;
            float newY = Mathf.Lerp(currentPos.y, targetYPosition, percentage);
            
            
            LadderUI.anchoredPosition = new Vector2(currentPos.x, newY); // Set the new position



            // Wait for the very next frame before continuing the loop
            yield return null;
        }

        // This is to make sure it doesn't do any fancy decimils and is a perfect int at the end of the fade
        elapsedTime = targetTime;

        SceneManager.LoadScene(sceneName);
        Debug.Log("Scene loaded: " + sceneName);
        
    }


    public void LoadSceneByName(string sceneName)
	{
        SceneManager.LoadScene(sceneName);
        Debug.Log("Scene loaded: " + sceneName);
    }

    public void LoadSceneByNameMainMenu(string sceneName)
	{
        StartCoroutine(AnimateMainMenu(sceneName));
    }

    public void Quit()
    {
        Debug.Log("Quit button pressed\nGame exiting...");
        Application.Quit();


        // stops playback in editor to test out mechanics when called (can be comented out)
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    #region === Audio ===

    public void SaveSoundSettings()
    {
        PlayerPrefs.SetFloat(masterVolumePref, masterVolumeSlider.value);
        PlayerPrefs.SetFloat(musicVolumePref, musicVolumeSlider.value);
        PlayerPrefs.SetFloat(sfxVolumePref, sfxSlider.value);
        PlayerPrefs.Save();

    }

    public void Reset()
    {
        SetMasterVolume(0.5f);
            masterVolumeSlider.value = 0.5f;
        SetBGMVolume(0.5f);
            musicVolumeSlider.value = 0.5f;
        SetSFXVolume(0.5f);
            sfxSlider.value = 0.5f;
        SaveSoundSettings();

    }

    public void UpdateSound()
    {
        if (isInitializing) return;

        SetMasterVolume(masterVolumeSlider.value);
        SetBGMVolume(musicVolumeSlider.value);
        SetSFXVolume(sfxSlider.value);
        SaveSoundSettings();
    }

    // AudioMixers will use int values for decibels insteas of just 0-100% values,
    // so you have to convert the value so the mixer will read it correctly.
    public void SetMasterVolume(float value)
    {
        if (audioMixer == null) audioMixer = Resources.Load<AudioMixer>("MasterVolume");

        if (audioMixer != null)
        {
            value = Mathf.Clamp(value, 0.0001f, 1f);
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        }
        else
        {
            Debug.LogError("[Audio Error] AudioMixer is missing from the Inspector slot and could not be found in the Resources folder!");
        }
    }

    public void SetBGMVolume(float value)
    {
        if (audioMixer == null) audioMixer = Resources.Load<AudioMixer>("MasterVolume");

        if (audioMixer != null)
        {
            value = Mathf.Clamp(value, 0.0001f, 1f);
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        }
    }

    public void SetSFXVolume(float value)
    {
        if (audioMixer == null) audioMixer = Resources.Load<AudioMixer>("MasterVolume");

        if (audioMixer != null)
        {
            value = Mathf.Clamp(value, 0.0001f, 1f);
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
            audioMixer.SetFloat("UIVolume", Mathf.Log10(value) * 20);
        }

    }

    public void ToggleMusic()
    {
        if (ToggleMusicOn == false)
            {
                MusicAS.Play();
                ToggleMusicOn = true;
            }
            else
            {
                MusicAS.Stop();
                ToggleMusicOn = false;
            }
    }

    public void ToggleSFX()
    {
            if (ToggleSFXOn == false)
            {
                SFXAS.Play();
                ToggleSFXOn = true;
            }
            else
            {
                SFXAS.Stop();
                ToggleSFXOn = false;
            }
    }


    #endregion

}
