using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SliderScript : MonoBehaviour
{
    public PlayerInput playerInput;
    public Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        // Find the PlayerInput component in your scene
    }
    public void InitializeInput(PlayerInput activePlayerInput)
    {
        playerInput = activePlayerInput;
    }
    public void OnSubmit(BaseEventData eventData)
    {
        EventSystem.current.sendNavigationEvents = false; // Enable navigation events
        playerInput.SwitchCurrentActionMap("SliderEdit");
        slider.image.color = Color.darkGray;


    }

    // Update is called once per frame
    void Update()
    {
        if (playerInput == null || playerInput.currentActionMap?.name != "SliderEdit") return;

        Vector2 move = playerInput.actions["SliderEdit/Left/Right"].ReadValue<Vector2>();
        if (move.x != 0)
        {
            // Move the slider value manually based on stick direction
            slider.value += move.x * Time.unscaledDeltaTime * 0.5f; 
        }

        // If they press Exit (A or B)
        if (playerInput.actions["SliderEdit/Submit"].triggered)
        {
            // Re-enable normal UI selection navigation
            EventSystem.current.sendNavigationEvents = true;

            // Go back to the standard UI map
            playerInput.SwitchCurrentActionMap("UI");

            // Reset visual color
            slider.image.color = Color.white;
        }


    }
}
