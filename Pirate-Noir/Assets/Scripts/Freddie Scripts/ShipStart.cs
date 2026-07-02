using UnityEngine;

public class ShipStart : MonoBehaviour
{
    [Header("Target Configuration")]
    [Tooltip("The Empty GameObject representing the start position.")]
    public Transform StartPoint;

    [Header("Movement Settings")]
    public float SailSpeed = 2f;
    private bool Sailing;

    [Header("Rocking")]
    public BoatRock ShipRock;  // Reference to the Boat Rock script

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        Sailing = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Sailing)
        {
            ExecuteSail();
        }
    }

    public void ExecuteSail()
    {
        transform.position = Vector3.MoveTowards(transform.position, StartPoint.position, SailSpeed * Time.deltaTime);  // Smoothly move position towards the target object

        // Disable the object once it perfectly reaches the target
        if (transform.position == StartPoint.position)
        {
            Sailing = false;
            // The ship has fully sailed to its destination.
            enabled = false; 
        }
       
    }
}
