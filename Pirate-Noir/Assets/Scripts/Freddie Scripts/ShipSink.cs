using UnityEngine;

public class ShipSink : MonoBehaviour
{
    [Header("Trigger")]
    public bool ShouldSink = false;

    [Header("Target Configuration")]
    [Tooltip("The Empty GameObject representing the final sunken position and rotation.")]
    public Transform SinkTarget;

    [Header("Movement Settings")]
    public float SinkSpeed = 2f;
    public float RotationSpeed = 10f;
    private bool Sinking = false;

    [Header("Rocking")]
    public BoatRock ShipRock;  // Reference to the Boat Rock script

    // Update is called once per frame
    void Update()
    {
        if (ShouldSink && !Sinking)
        {
            StartSink();
        }
        if (Sinking && SinkTarget != null)
        {
            ExecuteSink();
        }
    }

    void StartSink()
    {
        Sinking = true;
        if (ShipRock != null)
        {
            ShipRock.enabled = false;  // Disable the rocking effect
        }
    }

    void ExecuteSink()
    {
        transform.position = Vector3.MoveTowards(transform.position, SinkTarget.position, SinkSpeed * Time.deltaTime);  // Smoothly move position towards the target object

        transform.rotation = Quaternion.RotateTowards(transform.rotation, SinkTarget.rotation, RotationSpeed * Time.deltaTime);  // Smoothly rotate towards the target object's rotation

        // Disable the object once it perfectly reaches the target
        if (transform.position == SinkTarget.position && transform.rotation == SinkTarget.rotation)
        {
            // The ship has fully sunk to its destination.
            enabled = false; 
        }
    }
}
