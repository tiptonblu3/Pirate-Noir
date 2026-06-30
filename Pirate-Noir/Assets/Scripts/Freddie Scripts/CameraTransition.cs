using UnityEngine;
using System.Collections;

public class CameraTransition : MonoBehaviour
{
    public GameObject MainCamera; // Reference to the Main Camera
    public GameObject CutsceneCamera; // Reference to the Cutscene Camera
    public float CutsceneDuration = 2.0f; // Duration of the transition in seconds

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCutscene();
            // disable the trigger to prevent multiple activations
            GetComponent<Collider>().enabled = false;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartCutscene()
    {
        StartCoroutine(TransitionToCutscene());
    }

    public IEnumerator TransitionToCutscene()
    {
        float ElapsedTime = 0f;

        // Disable the main camera and enable the cutscene camera
        MainCamera.SetActive(false);
        CutsceneCamera.SetActive(true);

        while (ElapsedTime < CutsceneDuration)
        {
            ElapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Enable the main camera and disable the cutscene camera after the transition
        MainCamera.SetActive(true);
        CutsceneCamera.SetActive(false);

    }
}
