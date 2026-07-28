using UnityEngine;
using UnityEngine.Audio;

public class Rum : MonoBehaviour
{
    public AudioClip RumSound; // Reference to the break sound clip
    public AudioMixerGroup SFXMixerGroup;

    void Start()
    {
        SFXMixerGroup = Resources.Load<AudioMixer>("MasterVolume").FindMatchingGroups("sfxVolume")[0]; // Load the audio mixer and find the SFX group
        RumSound = Resources.Load<AudioClip>("AudioClips/RumCollect"); // Load the break sound clip from the Resources folder
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                PlayAudio();
                playerStats.Rum++; // Increment the player's rum count
                Debug.Log("Player picked up rum! Total rum: " + playerStats.Rum);
                Destroy(gameObject); // Destroy the rum object after pickup
            }
        }
    }

    public void PlayAudio() //this is meant to create a temporary audio object to play the sound effect then destroy that audio source object
    {
        GameObject TempAudioSource = new GameObject("TempAudio" + RumSound); // Create a temporary GameObject for the audio source
        TempAudioSource.transform.position = transform.position; // Set the position of the temporary audio source to the object's position

        AudioSource audioSource = TempAudioSource.AddComponent<AudioSource>(); // Add an AudioSource component to the temporary GameObject
        audioSource.clip = RumSound; // Assign the break sound clip to the audio source

        audioSource.outputAudioMixerGroup = SFXMixerGroup; // Assign the audio mixer group to the audio source
        audioSource.Play(); // Play the break sound effect

        Destroy(TempAudioSource, RumSound.length); // Destroy the temporary audio source after the sound has finished playing
    }
}
