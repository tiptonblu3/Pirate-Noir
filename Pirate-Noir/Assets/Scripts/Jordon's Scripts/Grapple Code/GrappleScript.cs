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
    public float swingAcceleration = 20f;
    public Rigidbody rb;

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

    private void Awake()
    {
        // Cache the Rigidbody reference on awake
        rb = player.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // If the joint exists, the player is currently actively swinging
        if (joint != null && rb != null)
        {
            // 1. Direction from player pointing straight to the anchor hook
            Vector3 dirToTarget = (swingPoint - player.position).normalized;
            
            // 2. Direction the player is looking
            Vector3 forwardForce = cam.forward;
            
            // 3. Mix the forces: 60% looking direction, 40% pulling up/in toward the hook point
            Vector3 pullDirection = (forwardForce * 0.6f) + (dirToTarget * 0.4f);
            
            // 4. Force a baseline upward lift so they don't lose altitude easily while swinging
            pullDirection.y = Mathf.Max(pullDirection.y + 0.2f, 0.3f);
            
            // 5. Apply continuous physics force (ForceMode.Force builds natural velocity)
            rb.AddForce(pullDirection.normalized * swingAcceleration, ForceMode.Force);
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
           playerStats.IsSwinging = true; // Swinging state is true when starting to swing
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

            if (rb != null)
            {
                // Calculate a direction that is forward based on the camera, but slightly tilted up
                Vector3 dirToTarget = (swingPoint - player.position).normalized;
                
                Vector3 pushDirection = (cam.forward * 0.6f) + (dirToTarget * 0.4f); // Forces a slight upward lift

                pushDirection.y = Mathf.Max(pushDirection.y, 0.5f, 0.5f); // Ensure there's always some upward force
                pushDirection = pushDirection.normalized; // Normalize the direction to ensure consistent force application

                if (rb.linearVelocity.y < 0)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                }


                // Apply an instant velocity change to launch the player forward
                rb.AddForce(pushDirection * forwardPullForce, ForceMode.Impulse);
            }
            maxSwingDistance = 2f;

        }
        
    }

    private void StopSwing()
    {
        playerStats.IsSwinging = false; // Swinging state is false when stopping the swing
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
