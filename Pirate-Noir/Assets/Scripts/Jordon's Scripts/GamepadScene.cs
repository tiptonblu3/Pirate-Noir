using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class GamepadScene : MonoBehaviour
{

    [Header("UI Elements")]

    public PlayerInput playerInput;
    public bool isUsingGamepad = false;
    public WinEndManag WinEnd;
    public PauseManagement PauseMang;

    void Awake()
    {
        // Grab the PlayerInput component on this object or in the scene
        playerInput = GetComponent<PlayerInput>();
    }
    void Start()
    {
        WinEnd = Object.FindAnyObjectByType<WinEndManag>();
        PauseMang = Object.FindAnyObjectByType<PauseManagement>();
    }

    void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.onControlsChanged += OnControlsChanged;
        }
    }

    void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.onControlsChanged -= OnControlsChanged;
        }
    }

    private void OnControlsChanged(PlayerInput input)
    {
        // Check if the current control scheme is a Gamepad
        if (input.currentControlScheme == "Gamepad" || input.currentActionMap.bindings.ToString().Contains("Gamepad"))
        {
            if (!isUsingGamepad)
            {
                isUsingGamepad = true;
                HighlightFirstButton();
            }
        }
        else // Switched back to Keyboard/Mouse
        {
            isUsingGamepad = false;
            // Optional: Clear selection so mouse hover looks cleaner
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void HighlightFirstButton()
    {
        if (EventSystem.current != null && isUsingGamepad)
        {
            if (PauseMang.GameIsPaused == true && WinEnd.EndGameState == false && PauseMang.InSettings == false)
            { //this is for the pause menu ui
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(PauseMang.firstSelectedPauseButton);
            }

            if (PauseMang.GameIsPaused == true && WinEnd.EndGameState == false && PauseMang.InSettings == true)
            { //this is for the settings menu ui
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(PauseMang.firstSelectedSettingsButton);
            }
            
            if (PauseMang.GameIsPaused == false && WinEnd.EndGameState == true && WinEnd.WinGroup.alpha == 1f)
            { //this is for the Win menu ui
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(WinEnd.firstSelectedWinButton);
            }
            
            if (PauseMang.GameIsPaused == false && WinEnd.EndGameState == true && WinEnd.LoseGroup.alpha == 1f)
            {  //this is for the lose menu ui
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(WinEnd.firstSelectedLoseButton);
            }

                //WinEnd;
                //PauseMang;

            
        }
    }


}
