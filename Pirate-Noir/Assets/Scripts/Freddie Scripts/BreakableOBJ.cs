using UnityEngine;
using UnityEngine.Audio;

public class BreakableOBJ : MonoBehaviour, IInteractable
{
    public float Health = 20f; // Health of the object
    public PlayerStats Stats;
    public GameObject Player;
    public AudioMixerGroup SFXMixerGroup;
    public AudioClip BreakSound; // Reference to the break sound clip
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player"); // Find the player GameObject by tag
        Stats = Player.GetComponent<PlayerStats>(); // Get the PlayerStats component from the player Game
        SFXMixerGroup = Resources.Load<AudioMixer>("MasterVolume").FindMatchingGroups("sfxVolume")[0]; // Load the audio mixer and find the SFX group
        BreakSound = Resources.Load<AudioClip>("AudioClips/BreakSound"); // Load the break sound clip from the Resources folder
    }

    public void Interact()
    {
        Debug.Log("Player Hit!"); // Log interaction for debugging
        Health -= Stats.AttackPower; // Reduce Object health by player's attack power
        Debug.Log($"Object Health: {Health}"); // Log object health for debugging
        
        DestroyObject(); //call this method to play breaking audio

        if (Health <= 0) // Check if the object is defeated
        {
            Debug.Log("Object Defeated!"); // Log enemy defeat for debugging
            Destroy(this.gameObject); // Destroy the GameObject
        }
    }

    public void DestroyObject() //this is meant to create a temporary audio object to play the sound effect then destroy that audio source object
    {
        GameObject TempAudioSource = new GameObject("TempAudio" + BreakSound); // Create a temporary GameObject for the audio source
        TempAudioSource.transform.position = transform.position; // Set the position of the temporary audio source to the object's position

        AudioSource audioSource = TempAudioSource.AddComponent<AudioSource>(); // Add an AudioSource component to the temporary GameObject
        audioSource.clip = BreakSound; // Assign the break sound clip to the audio source

        audioSource.outputAudioMixerGroup = SFXMixerGroup; // Assign the audio mixer group to the audio source
        audioSource.Play(); // Play the break sound effect

        Destroy(TempAudioSource, BreakSound.length); // Destroy the temporary audio source after the sound has finished playing
    }
}
