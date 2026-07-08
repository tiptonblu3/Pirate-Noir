using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class CameraTransition : MonoBehaviour
{
    public GameObject MainCamera; // Reference to the Main Camera
    public GameObject CutsceneCamera; // Reference to the Cutscene Camera
    public float CutsceneDuration = 2.0f; // Duration of the transition in seconds

    void OnEnable()
    {
        StartCutscene();
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
        DisablePlayerInput(); // Disable player input during the cutscene
        CutsceneCamera.SetActive(true);

        while (ElapsedTime < CutsceneDuration)
        {
            ElapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Enable the main camera and disable the cutscene camera after the transition
        MainCamera.SetActive(true);
        EnablePlayerInput(); // Re-enable player input after the cutscene
        CutsceneCamera.SetActive(false);

    }

    [SerializeField] private PlayerInput playerInput;

    // Call this to freeze the player during cutscenes or menus
    public void DisablePlayerInput()
    {
        if (playerInput != null)
        {
            playerInput.DeactivateInput(); // Disables all actions completely
        }
    }

    // Call this to give control back to the player
    public void EnablePlayerInput()
    {
        if (playerInput != null)
        {
            playerInput.ActivateInput(); // Re-enables the default action map
        }
    }

}
