using UnityEngine;
using UnityEngine.InputSystem;

public class GrappleScript : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    public LayerMask whatIsGrappleable;
    public PlayerMovement playerMovement;
    public PlayerStats playerStats;
    
    
    [Space(5)]
    [Header("Swinging")]
    public float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint;
    private Vector3 currentGrapplePosition;
    public float SwingTime = 5f; // How quickly the rope moves to the grapple point

    [Space(5)]
    [Header("Swinging Values")]
    public float spring = 4.5f;
    public float damper = 7f;
    public float massScale = 4.5f;
    public float forwardPullForce = 15f; 


    public void OnGrapple(InputAction.CallbackContext Context)
    {
        if (Context.started)
        {
            StartSwing();
        }
        else if (Context.canceled)
        {
            StopSwing();
        }
    }


    // Update is called once per frame
    void Update()
    {
        DrawRope();
    }

    private void StartSwing()
    {
        if (joint != null) return;
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, whatIsGrappleable))
        {
           swingPoint = hit.point;

           if (playerMovement.IsGrounded != true)
            {
                    playerMovement.CurrentSpeed = playerStats.BaseSwingingSpeed;
            }

           joint = player.gameObject.AddComponent<SpringJoint>(); 
           joint.autoConfigureConnectedAnchor = false;
           joint.connectedAnchor = swingPoint;

           float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            // The distance grapple will try to keep from grapple point. 
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            // Adjust these values to fit your game.
            joint.spring = spring;
            joint.damper = damper;
            joint.massScale = massScale;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Calculate a direction that is forward based on the camera, but slightly tilted up
                Vector3 pushDirection = cam.forward;
                pushDirection.y = Mathf.Clamp(pushDirection.y + 0.2f, 0.1f, 0.5f); // Forces a slight upward lift

                // Apply an instant velocity change to launch the player forward
                rb.AddForce(pushDirection.normalized * forwardPullForce, ForceMode.VelocityChange);
            }
            maxSwingDistance = 10f;

        }
        
    }

    private void StopSwing()
    {
        if (playerMovement.IsGrounded == true)
        {
                playerMovement.CurrentSpeed = playerMovement.CanSprint ? playerStats.SprintSpeed : playerStats.MoveSpeed;;
        }

        maxSwingDistance = 25f;

        lr.positionCount = 0;
        if (joint != null)
            Destroy(joint);
    }

    private void DrawRope()
    {
        if (!joint) 
        {
            lr.positionCount = 0;
            return;
        }

        if (lr.positionCount != 2)
        {
            lr.positionCount = 2;
        }
        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * SwingTime);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }



}
