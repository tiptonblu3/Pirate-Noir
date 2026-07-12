using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;


public class GamepadMainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject firstSelectedButton;

    public bool isUsingGamepad = false;

    void OnEnable()
    {
            InputSystem.onActionChange += OnControlsChanged;   
    }

    void OnDisable()
    {
            InputSystem.onActionChange -= OnControlsChanged; 
    }

    private void OnControlsChanged(object obj, InputActionChange change)
    {
        // Check if the current control scheme is a Gamepad
        if (change == InputActionChange.ActionPerformed)
        {
            InputAction action = (InputAction)obj;
            
            if (action.activeControl?.device is Gamepad)
            {
                
                if (!isUsingGamepad) // Only highlight if we weren't already using a gamepad
                {
                    isUsingGamepad = true;
                    HighlightFirstButton();
                }
            }
        
            else if (action.activeControl?.device is Mouse) // Switched back to Keyboard/Mouse
            {
                if (isUsingGamepad)
                {
                    isUsingGamepad = false;
                    // Optional: Clear selection so mouse hover looks cleaner
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }
    }

    public void HighlightFirstButton()
    {
        if (EventSystem.current != null && firstSelectedButton != null && isUsingGamepad)
        {
            // Clear current selection first to prevent Unity UI bugs
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }
}

