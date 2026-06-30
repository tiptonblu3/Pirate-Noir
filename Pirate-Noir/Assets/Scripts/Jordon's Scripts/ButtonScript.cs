using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    public AudioSource audioSource; // Reference to the AudioSource component
    public AudioClip hoverSound; // Reference to the hover sound clip
    public AudioClip selectSound; // Reference to the select sound clip

    public void Start()
    {
        // Ensure the AudioSource component is attached to the same GameObject
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    public void PlayHover() 
    {
        audioSource.PlayOneShot(hoverSound);
    }

    // Update is called once per frame
    public void PlaySelect() 
    {
        audioSource.PlayOneShot(selectSound);
    }
}
