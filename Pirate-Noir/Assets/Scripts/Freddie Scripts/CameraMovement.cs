using UnityEngine;
using UnityEngine.InputSystem; // Unity Input System namespace

public class CameraMovement : MonoBehaviour
{
    public float SensX = 1f; // Mouse sensitivity for X rotation
    public float SensY = 1f; // Mouse sensitivity for Y rotation
    public float ControllerSensX = 10f; // Controller sensitivity for X rotation
    public float ControllerSensY = 10f; // Controller sensitivity for Y rotation
    public bool IsUsingGamepad; // Flag to track if the player is using a gamepad/controller
    public Vector2 LookInput; // Stores the current look input from mouse or controller

    public float deltaX; // Horizontal rotation delta for the current frame
    public float deltaY; // Vertical rotation delta for the current frame

    public Transform Orientation; // Reference to the player's orientation for camera rotation

    public float MouseX; // X mouse input
    public float MouseY; // Y mouse input

    private float xRotation; // Cumulative vertical rotation value


    #region === Player Movement ===]
    public PlayerMovement Movement; // Reference to the PlayerMovement component ~F
    #endregion

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        Cursor.visible = false; // Hide the cursor
        
        Movement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>(); // Get the PlayerMovement component from the player Gameobject ~F

    }

    public void OnLook(InputAction.CallbackContext Context)
    {
        if (!Movement.CanMove) // Check if the player can move
        {
            return; // If the player cannot move, exit the method
        }

        LookInput = Context.ReadValue<Vector2>();
        IsUsingGamepad = Context.control.device is Gamepad;

    }

    private void Update()
    {
        if (!Movement.CanMove) // Check if the player can move
        {
            deltaX = 0f; // Reset horizontal rotation delta
            deltaY = 0f; // Reset vertical rotation delta
            LookInput = Vector2.zero; // Reset look input
            return; // If the player cannot move, exit the method
        }
        // Detect if the input is coming from a Gamepad/Controller or a Mouse
        if (IsUsingGamepad)
        {
            

            // Multiply by Controller Sensitivity AND Time.deltaTime
            deltaX = LookInput.x * ControllerSensX * Time.deltaTime;
            deltaY = LookInput.y * ControllerSensY * Time.deltaTime;
        }
        else
        {
            // Calculate the frame's deltas
            deltaX = LookInput.x * SensX;
            deltaY = LookInput.y * SensY;
        }

        // Accumulate the vertical rotation (subtract or add depending on your preferred inversion)
        xRotation -= deltaY; 

        // Clamp the CUMULATIVE rotation, not the delta
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        // Apply horizontal rotation to the player body (still using delta is fine here)
        Orientation.Rotate(Vector3.up * deltaX);

        // Directly set the local rotation of the camera using the clamped total
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
