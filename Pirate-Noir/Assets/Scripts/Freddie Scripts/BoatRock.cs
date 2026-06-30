using UnityEngine;

public class BoatRock : MonoBehaviour
{
    public GameObject Boat; // Reference to the Boat GameObject that will be rocked
    public float RockSpeed = 6f; // Speed at which the boat rocks back and forth
    public float RockAngle = 2.5f; // Maximum angle in degrees that the boat will rock
    public bool XAxisRock; // Boolean to determine if the rocking should occur on the X axis
    public bool ZAxisRock; // Boolean to determine if the rocking should occur on the Z axis
    public bool FlipRock; // Boolean to determine if the rocking direction should be flipped
    private Quaternion StartRotation; // Store the starting rotation of the Boat

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartRotation = Boat.transform.rotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Rock(); // Call the Rock method to apply the rocking effect
    }

    public void Rock() // Public method that can be called by other scripts to make the Boat object rock
    {
        if (Boat != null && FlipRock == false) // Check that the Boat reference exists and that we are not flipping the rocking direction
        {
            float xRotation = 0f; // Initialize X rotation to 0
            float zRotation = 0f; // Initialize Z rotation to 0

            if (XAxisRock) // If rocking on the X axis is enabled
            {
                xRotation = Mathf.Sin(Time.time * RockSpeed) * RockAngle; // Calculate a sine wave based rotation for the X axis
            }

            if (ZAxisRock) // If rocking on the Z axis is enabled
            {
                zRotation = Mathf.Sin(Time.time * RockSpeed) * RockAngle; // Calculate a sine wave based rotation for the Z axis
            }

            Boat.transform.rotation = StartRotation * Quaternion.Euler(xRotation, 0, zRotation); // Apply the calculated rotations to the Boat relative to its starting rotation
        }
        else if (Boat != null && FlipRock == true) // Check that the Boat exists and that the rocking direction should be flipped
        {
            float xRotation = 0f; // Initialize X rotation to 0
            float zRotation = 0f; // Initialize Z rotation to 0

            if (XAxisRock) // If rocking on the X axis is enabled
            {
                xRotation = -Mathf.Sin(Time.time * RockSpeed) * RockAngle; // Calculate a sine wave based rotation for the X axis but invert it to flip the rocking direction
            }

            if (ZAxisRock) // If rocking on the Z axis is enabled
            {
                zRotation = -Mathf.Sin(Time.time * RockSpeed) * RockAngle; // Calculate a sine wave based rotation for the Z axis but invert it to flip the rocking direction
            }

            Boat.transform.rotation = StartRotation * Quaternion.Euler(xRotation, 0, zRotation); // Apply the flipped rotations to the Boat relative to its starting rotation
        }
    }
}
