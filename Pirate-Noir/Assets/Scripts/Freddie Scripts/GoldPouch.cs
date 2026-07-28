using UnityEngine;
using UnityEngine.Audio;

public class GoldPouch : MonoBehaviour
{
    public int GoldAmount = 100; // Amount of gold in the pouch
    public int GoldRangeMin = 50; // Minimum amount of gold in the pouch
    public int GoldRangeMax = 250; // Maximum amount of gold in the pouch
    public AudioClip GoldSound; // Reference to the break sound clip
    public AudioMixerGroup SFXMixerGroup;


    void Start()
    {
        // Assign a random gold amount within the specified range
        GoldAmount = Random.Range(GoldRangeMin, GoldRangeMax + 1);
        SFXMixerGroup = Resources.Load<AudioMixer>("MasterVolume").FindMatchingGroups("sfxVolume")[0]; // Load the audio mixer and find the SFX group
        GoldSound = Resources.Load<AudioClip>("AudioClips/GoldSound"); // Load the break sound clip from the Resources folder
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            PlayAudio();
            if (playerStats != null)
            {
                playerStats.Gold += GoldAmount; // Increment the player's gold count
                Debug.Log("Player picked up a gold pouch! Total gold: " + playerStats.Gold);
                Destroy(gameObject); // Destroy the gold pouch object after pickup
            }
        }
    }

    public void PlayAudio() //this is meant to create a temporary audio object to play the sound effect then destroy that audio source object
    {
        GameObject TempAudioSource = new GameObject("TempAudio" + GoldSound); // Create a temporary GameObject for the audio source
        TempAudioSource.transform.position = transform.position; // Set the position of the temporary audio source to the object's position

        AudioSource audioSource = TempAudioSource.AddComponent<AudioSource>(); // Add an AudioSource component to the temporary GameObject
        audioSource.clip = GoldSound; // Assign the break sound clip to the audio source

        audioSource.outputAudioMixerGroup = SFXMixerGroup; // Assign the audio mixer group to the audio source
        audioSource.Play(); // Play the break sound effect

        Destroy(TempAudioSource, GoldSound.length); // Destroy the temporary audio source after the sound has finished playing
    }
}
