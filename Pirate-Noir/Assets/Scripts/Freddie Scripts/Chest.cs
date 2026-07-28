using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class Chest : MonoBehaviour, IInteractable
{
    private PlayerStats Stats;
    private GameObject Player;

    [Header("Auto Assigned Chest Settings")]
    public int Points;

    [Header("Locked Chest Settings")]
    public bool Locked = false; // Indicates if the chest is locked
    public int GoldAmountLockedMin = 500; // Minimum amount of gold in the chest if it's locked
    public int GoldAmountLockedMax = 2000; // Maximum amount of gold in the chest if it's locked
    [Header("Unlocked Chest Settings")]
    public int GoldAmountMin = 100; // Minimum amount of gold in the chest
    public int GoldAmountMax = 500; // Maximum amount of gold in the chest

    [Header("Item Spawn Settings")]
    public bool SpawnItemOnOpen = false; // Indicates if an item should be spawned when the chest is opened
    private int ItemSpawnChance = 50; // Chance (in percentage) to spawn an item when the chest is opened
    public List<GameObject> ItemPrefabs;// Prefab list

    [Header("Audio Settings")]
    public AudioClip ChestLockedSound;
    public AudioClip ChestOpenSound;
    public AudioMixerGroup SFXMixerGroup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player"); // Find the player GameObject by tag
        Stats = Player.GetComponent<PlayerStats>(); // Get the PlayerStats component from the player Game
        ChestLockedSound = Resources.Load<AudioClip>("AudioClips/ChestLocked"); // Load the break sound clip from the Resources folder
        ChestOpenSound = Resources.Load<AudioClip>("AudioClips/ChestOpen"); // Load the break sound clip from the Resources folder
        SFXMixerGroup = Resources.Load<AudioMixer>("MasterVolume").FindMatchingGroups("sfxVolume")[0]; // Load the audio mixer and find the SFX group



        if (Locked)
        {
            Points = Random.Range(GoldAmountLockedMin, GoldAmountLockedMax + 1); // Assign a random point value between the locked gold amounts
        }
        else
        {
            Points = Random.Range(GoldAmountMin, GoldAmountMax + 1); // Assign a random point value between the unlocked gold amounts
        }

        if (Random.Range(0, 100) < ItemSpawnChance)
        {
            SpawnItemOnOpen = true; // Set to true if the random chance is met
        }

    }

    public void Interact()
    {
        if (Locked)
        {
            Debug.Log("Chest is locked. Player needs a key to open it.");
            if (Stats.Keys > 0)
            {
                Stats.Keys--; // Use a key to unlock the chest
                Locked = false; // Unlock the chest
                Debug.Log("Chest unlocked! Player used a key.");
                Stats.Gold += Points; // Add the points to the player's gold
                gameObject.SetActive(false); // disable this game object after interaction
                if (SpawnItemOnOpen)
                {
                    SpawnItem(); // Call the SpawnItem method to spawn an item
                }
                PlayOpenAudio();
            }
            else
            {
                Debug.Log("Player does not have any keys to unlock the chest.");
                PlayLockedAudio();

            }
        }
        else
        {
            Debug.Log("Chest interacted with!");
            Stats.Gold += Points; // Add the points to the player's gold
            gameObject.SetActive(false); // disable this game object after interaction
            if (SpawnItemOnOpen)
            {
                SpawnItem(); // Call the SpawnItem method to spawn an item
            }
            PlayOpenAudio();
        }

        
    }

    public void SpawnItem()
    {
        // Spawn a random item from the ItemPrefabs list at the chest's position
        if (ItemPrefabs.Count > 0)
        {
            int randomIndex = Random.Range(0, ItemPrefabs.Count);
            GameObject itemPrefab = ItemPrefabs[randomIndex];
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }
    }

    public void PlayOpenAudio() //this is meant to create a temporary audio object to play the sound effect then destroy that audio source object
    {
        GameObject TempAudioSource = new GameObject("TempAudio" + ChestOpenSound); // Create a temporary GameObject for the audio source
        TempAudioSource.transform.position = transform.position; // Set the position of the temporary audio source to the object's position

        AudioSource audioSource = TempAudioSource.AddComponent<AudioSource>(); // Add an AudioSource component to the temporary GameObject
        audioSource.clip = ChestOpenSound; // Assign the break sound clip to the audio source

        audioSource.outputAudioMixerGroup = SFXMixerGroup; // Assign the audio mixer group to the audio source
        audioSource.Play(); // Play the break sound effect

        Destroy(TempAudioSource, ChestOpenSound.length); // Destroy the temporary audio source after the sound has finished playing
    }

    public void PlayLockedAudio() //this is meant to create a temporary audio object to play the sound effect then destroy that audio source object
    {
        GameObject TempAudioSource = new GameObject("TempAudio" + ChestLockedSound); // Create a temporary GameObject for the audio source
        TempAudioSource.transform.position = transform.position; // Set the position of the temporary audio source to the object's position

        AudioSource audioSource = TempAudioSource.AddComponent<AudioSource>(); // Add an AudioSource component to the temporary GameObject
        audioSource.clip = ChestLockedSound; // Assign the break sound clip to the audio source

        audioSource.outputAudioMixerGroup = SFXMixerGroup; // Assign the audio mixer group to the audio source
        audioSource.Play(); // Play the break sound effect

        Destroy(TempAudioSource, ChestLockedSound.length); // Destroy the temporary audio source after the sound has finished playing
    }
}
