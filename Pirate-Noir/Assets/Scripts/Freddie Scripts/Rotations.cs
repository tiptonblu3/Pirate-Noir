using UnityEngine;

public class Rotations : MonoBehaviour
{
    public float RotationSpeed = 50f; // Speed of rotation in degrees per second
    public bool RotateX = false; // Option to rotate around the X-axis
    public bool RotateY = false; // Option to rotate around the Y-axis
    public bool RotateZ = false; // Option to rotate around the Z-axis
    public bool Floating = false; // Option to enable or disable floating animation
    private Vector3 StartPos; // Store the initial position of the object for animation purposes
    private GameObject Player; // Reference to the Player object
    private GameObject Camera; // Reference to the main camera
    public bool LookAtPlayer = false; // Option to enable or disable looking at the Player
    public bool LookAtCamera = false; // Option to enable or disable looking at the camera
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartPos = transform.position;
        Player = GameObject.FindGameObjectWithTag("Player");
        Camera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Update is called once per frame
    void Update()
    {
        Rotate();
        Float();
        LookAt();
    }

    void Rotate()
    {
        if (RotateX)
        {
            transform.Rotate(Vector3.right, RotationSpeed * Time.deltaTime);
        }
        if (RotateY)
        {
            transform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
        }
        if (RotateZ)
        {
            transform.Rotate(Vector3.forward, RotationSpeed * Time.deltaTime);
        }
    }
    void Float()
    {
        if (Floating)
        {
            float bounce = Mathf.Sin(Time.time * 2) * 0.1f;
            transform.position = new Vector3(StartPos.x, StartPos.y + bounce, StartPos.z);
        }
    
    }
    void LookAt()
    {
        if (LookAtPlayer && Player != null)
        {
            transform.LookAt(Player.transform);
        }
        else if (LookAtCamera && Camera != null)
        {
            transform.LookAt(Camera.transform);
        }
    }
}
