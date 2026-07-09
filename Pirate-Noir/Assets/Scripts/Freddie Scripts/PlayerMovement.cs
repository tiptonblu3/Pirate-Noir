using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // Unity Input System namespace

public class PlayerMovement : MonoBehaviour
{
    private PlayerStats Stats; // Reference to the PlayerStats component for accessing movement speeds and stamina

    #region === Movement Settings ===

    [Header("Movement Settings")] // Inspector header for movement settings
    public Rigidbody RB; // Rigidbody reference for physics-based movement
    public bool IsGrounded; // Whether the player is currently grounded
    public bool IsSprinting; // Whether the player is currently sprinting
    public bool CanSprint; // Whether the player can sprint based on input and stamina
    private bool SprintDrain; // Whether the player is currently draining stamina from sprinting
    public float CurrentSpeed; // Current movement speed based on sprinting state
    public bool CanMove = true; // Whether the player can move (e.g., not stunned or in a cutscene)


    public Vector2 MoveInput; // Raw movement input from player
    public float VerticalY; // Vertical velocity value
    Vector3 MoveDirection; // Calculated movement direction

        [Header("Stamina Settings")]
    public float StaminaDrainRate = 20f; // Stamina lost per second while running
    public float StaminaRegenRate = 15f; // Stamina gained per second while resting

    #endregion

    #region === Jump Settings ===
    [Header("Jump Settings")] // Inspector header for jumping settings
    public float JumpForce = 3f; // Strength of jump force
    public float BaseGravity = 1f; // Base gravity value to keep player grounded when not jumping
    public float SwingGravity = 5f; // Gravity value when swinging to keep player grounded
    public float Gravity = -25f; // Custom gravity applied manually
    #endregion

    #region === Ground Detection ===
    [Header("Ground Detection")] // Inspector header for ground detection settings
    public LayerMask GroundLayer; // Layer mask used to identify ground objects
    public float GroundCheckRadius = 0.3f; // Radius of ground check sphere
    public float GroundCheckOffset = 0.1f; // Height offset for ground check
    #endregion

    #region === Stair & Slope Snapping ===
    [Header("Stair & Slope Snapping")]
    public float MaxStepHeight = 0.4f; // Maximum distance the script will snap down to hit a step
    private bool WasGroundedLastFrame;
    private float AirborneTimer = 0f;
    public float CoyoteTimeDuration = 0.15f; // Grace period before treating player as truly airborne
    [Header("Step Up Settings")]
    public float StepCheckDistance = 0.4f; // How far ahead to look for a step
    #endregion

    #region === Interaction Settings ===
    // Groups interaction-related variables in the Inspector
    [Header("Interaction Settings")]
    public float InteractRange = 3f;
    public LayerMask InteractableLayer; // Assign this in Inspector!
    #endregion

    #region === Attack Settings ===
    [Header("Attack Settings")]
    public LayerMask AttackableLayer;
    public bool CanAttack = true; // Whether the player can attack (e.g., not stunned or in a cutscene)
    #endregion

    #region === Animation ===

    [Header("Animation")] // Inspector header for animation settings
    public Animator Anim; // Animator reference

    #endregion

    #region === Audio ===
    [Header("Audio")]
    public AudioSource PlayerFootstepAudio; // AudioSource for footstep sounds
    public AudioSource PlayerActionAudio; // AudioSource for action sounds
    public AudioClip FootstepClip; // Footstep sound clip
    public AudioClip RunningFootstepClip; // Running footstep sound clip
    public AudioClip JumpClip; // Jump sound clip
    public AudioClip AttackClip; // Attack sound clip
    #endregion

    #region === UI ===
    [Header("UI")]
    public PauseManagement PauseManag;
    public WinEndManag WinEndManag;
    

    #endregion

    private void Start()
    {
        Stats = GetComponent<PlayerStats>(); // Get the PlayerStats component

        RB = GetComponent<Rigidbody>(); // Get the Rigidbody component

        PauseManag = Object.FindAnyObjectByType<PauseManagement>(); // Get the PauseManagement component

        WinEndManag = Object.FindAnyObjectByType<WinEndManag>(); // Get the WinEndManagement component

        RB.freezeRotation = true; // Prevent the Rigidbody from rotating due to physics

        RB.useGravity = false; // Disable built-in gravity

        RB.collisionDetectionMode = CollisionDetectionMode.Continuous; // Set collision detection mode for better accuracy
    }

    public void OnMove(InputAction.CallbackContext Context)
    {
        MoveInput = Context.ReadValue<Vector2>(); // Read the movement input as a Vector2 (x for horizontal, y for vertical)
    }

    public void OnSprint(InputAction.CallbackContext Context) // Called when sprint input changes
    {
        if (Context.performed) // Sprint button pressed
            IsSprinting = true; // Enable sprinting

        else if (Context.canceled) // Sprint button released
            IsSprinting = false; // Disable sprinting
    }

    public void OnJump(InputAction.CallbackContext Context) // Called when jump input occurs
    {
        Debug.Log("Jump input received"); // Log jump input for debugging
        if (Context.started && IsGrounded && CanMove) // Only jump if grounded and can move
        {
            VerticalY = Mathf.Sqrt(JumpForce * -2f * Gravity); // Calculate jump velocity

            if (Anim != null) // Ensure animator exists
            {
                Anim.SetTrigger("Jump"); // Trigger jump animation
            }
            if (PlayerActionAudio != null && JumpClip != null) // Ensure audio source and clip exist
            {
                PlayerActionAudio.PlayOneShot(JumpClip); // Play jump sound effect
            }
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.started && PauseManag != null)
        {
            if (PauseManag.GameIsPaused) 
                PauseManag.Resume();
            else 
                PauseManag.Pause();    
        }
    }

    public void Movement()
    {
        if (!CanMove) return; // Don't process movement if player can't move
        CanSprint = IsSprinting && MoveInput.magnitude > 0 && Stats.CurrentStamina > StaminaDrainRate; // Player can sprint if sprinting, has movement input, and enough stamina to drain
        
        if (Stats.IsSwinging)
        {
            CurrentSpeed = Stats.SwingingSpeed; // Use swinging speed when swinging
        }
        else
        {
            CurrentSpeed = CanSprint ? Stats.SprintSpeed : Stats.MoveSpeed; // Use modified speeds from stats
        }
        
        MoveDirection = transform.forward * MoveInput.y + transform.right * MoveInput.x; // Calculate movement direction based on input and player orientation
        Vector3 HorizontalVelocity = MoveDirection * CurrentSpeed; // Calculate horizontal velocity based on movement direction and current speed
        RB.linearVelocity = new Vector3(HorizontalVelocity.x, VerticalY, HorizontalVelocity.z); // Set Rigidbody velocity, preserving vertical velocity for jumping and gravity
    }

    
    private void HandleStepUp()
    {
        // Only try to step up if the player is actively providing movement input
        if (MoveDirection.magnitude < 0.1f) return;

        // Normalize our movement direction on the flat XZ plane
        Vector3 moveDirXZ = new Vector3(MoveDirection.x, 0, MoveDirection.z).normalized;

        // 1. Lower Ray: Check if we are walking into a wall/step at shin/ankle height
        Vector3 lowerOrigin = transform.position + Vector3.up * 0.05f; 
        if (Physics.Raycast(lowerOrigin, moveDirXZ, out RaycastHit lowerHit, StepCheckDistance, GroundLayer))
        {
            // 2. Upper Ray: Check if there is clear space above our MaxStepHeight
            Vector3 upperOrigin = transform.position + Vector3.up * (MaxStepHeight + 0.05f);
            if (!Physics.Raycast(upperOrigin, moveDirXZ, StepCheckDistance, GroundLayer))
            {
                // 3. Forward-Down Ray: Find the exact height of the top of the step
                // We project slightly ahead of the lower hit point
                Vector3 downRayOrigin = lowerHit.point + moveDirXZ * 0.1f + Vector3.up * MaxStepHeight;
                
                if (Physics.Raycast(downRayOrigin, Vector3.down, out RaycastHit downHit, MaxStepHeight + 0.1f, GroundLayer))
                {
                    // Ensure the surface we are stepping onto is relatively flat
                    if (Vector3.Angle(downHit.normal, Vector3.up) < 45f)
                    {
                        // Smoothly lift the Rigidbody position over the step step
                        // We add a tiny bit of forward nudge so they clear the lip of the step
                        transform.position = new Vector3(transform.position.x, downHit.point.y, transform.position.z) + (moveDirXZ * 0.05f);
                        
                        // Zero out vertical velocity so gravity doesn't violently fight the step up
                        VerticalY = 0f; 
                        IsGrounded = true;
                    }
                }
            }
        }
    }

    private void HandleStamina()
    {
        if (!CanMove) return; // Don't process movement if player can't move
        SprintDrain = IsSprinting && MoveInput.magnitude > 0 && Stats.CurrentStamina > 0f; // Evaluate if the player is actively burning stamina right now

        if (SprintDrain)
        {
            // Drain stamina directly from stats
            Stats.CurrentStamina -= StaminaDrainRate * Time.fixedDeltaTime;
            
            if (Stats.CurrentStamina < 0f) Stats.CurrentStamina = 0f;
        }
        else
        {
            // Regenerate stamina up to the MaxStamina value stored in stats
            if (Stats.CurrentStamina < Stats.MaxStamina)
            {
                Stats.CurrentStamina += StaminaRegenRate * Time.fixedDeltaTime;
                
                if (Stats.CurrentStamina > Stats.MaxStamina) Stats.CurrentStamina = Stats.MaxStamina;
            }
        }
    }

    #region === Ground Detection ===

    // Checks whether the player is currently grounded.
    
    private void CheckGround()
    {
        /*
        // Create sphere position slightly above feet
        Vector3 SpherePosition = transform.position + Vector3.up * GroundCheckOffset; // Offset sphere upward slightly

        // Perform sphere collision check
        IsGrounded = Physics.CheckSphere(SpherePosition, GroundCheckRadius, GroundLayer); // Detect ground collision
        */

        // Keep track of what we were before updating
        WasGroundedLastFrame = IsGrounded;

        Vector3 SpherePosition = transform.position + Vector3.up * GroundCheckOffset;
        IsGrounded = Physics.CheckSphere(SpherePosition, GroundCheckRadius, GroundLayer);

        // --- Ground Snapping Logic ---
        // If we just lost grounding, but we are moving down/horizontally (not jumping)
        if (!IsGrounded && WasGroundedLastFrame && VerticalY <= 0)
        {
            // Cast a ray downwards from the player's feet position to find a step
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, MaxStepHeight + 0.1f, GroundLayer))
            {
                // Ensure the surface normal isn't a steep wall
                if (Vector3.Angle(hit.normal, Vector3.up) < 45f) 
                {
                    IsGrounded = true;
                    // Snap position down to the step surface
                    transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                    VerticalY = -BaseGravity; // Match grounded gravity force
                }
            }
        }

        // --- Coyote Time / Grace Period Tracker ---
        if (IsGrounded)
        {
            AirborneTimer = 0f;
        }
        else
        {
            AirborneTimer += Time.fixedDeltaTime;
        }
    }

    #endregion

    #region === Gravity ===

    
    // Applies custom gravity to the player.
    
    private void ApplyGravity()
    {
        if (Stats.IsSwinging)
        {
            VerticalY = -SwingGravity; // Keep slight downward force to remain grounded
        }
        else
        if (IsGrounded && VerticalY < 0) // Player grounded while falling
        {
            VerticalY = -BaseGravity; // Keep slight downward force to remain grounded
        }
        else // Player airborne
        {
            VerticalY += Gravity * Time.fixedDeltaTime; // Apply gravity over time
        }
    }

    #endregion

    public void OnInteract(InputAction.CallbackContext Context)
    {
        if (Context.started && CanMove) // Only check for interactables when the interact button is initially pressed
        {
            Debug.Log("Interact button pressed, checking for interactables...");
            
            Ray Ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward); // Create a ray from the camera's position forward
            Debug.DrawRay(Ray.origin, Ray.direction * InteractRange, Color.red, 2f); // Draw the ray in the scene view for debugging purposes

            // Added QueryTriggerInteraction.Collide to include Triggers in the raycast
            if (Physics.Raycast(Ray, out RaycastHit hit, InteractRange, InteractableLayer, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.TryGetComponent(out IInteractable Interactable))
                {
                    Interactable.Interact(); // Call the Interact method on the interactable object
                    Debug.Log($"Interacted with {hit.collider.name}"); // Log the name of the interacted object for debugging
                }
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext Context)
    {
        if (Context.performed && CanMove && CanAttack) // Only check for Enemies when the attack button is initially pressed
        {
            // Start Attack Cooldown
            StartCoroutine(AttackCooldownRoutine());

            Debug.Log("Attack button pressed, checking for Enemies...");
            
            Ray AttackRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward); // Create a ray from the camera's position forward
            Debug.DrawRay(AttackRay.origin, AttackRay.direction * Stats.AttackRange, Color.red, 2f); // Draw the ray in the scene view for debugging purposes

            // Added QueryTriggerInteraction.Collide to include Triggers in the raycast
            if (Physics.Raycast(AttackRay, out RaycastHit hit, Stats.AttackRange, AttackableLayer, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.TryGetComponent(out IInteractable Interactable))
                {
                    Interactable.Interact(); // Call the Interact method on the interactable object
                    Debug.Log($"Attacked {hit.collider.name}"); // Log the name of the Attacked object for debugging
                }
            }
            if (Anim != null) // Ensure animator exists
            {
                Anim.SetTrigger("Attack"); // Trigger attack animation
            }

            if (PlayerActionAudio != null && AttackClip != null) // Ensure audio source and clip exist
            {
                PlayerActionAudio.PlayOneShot(AttackClip); // Play attack sound effect
            }
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        CanAttack = false; // Close the gate

        yield return new WaitForSeconds(Stats.AttackCooldown);

        CanAttack = true; // Open the gate
    }

    #region === Animation Logic ===

    
    // Updates animator parameters based on player state.
    
    private void UpdateAnimations()
    {
        if (Anim == null) return; // Stop if no animator assigned

        float CurrentAnimSpeed = MoveInput.magnitude; // Base movement animation speed

        if (IsSprinting && CanSprint && CurrentAnimSpeed > 0) // Check if sprinting while moving
        {
            CurrentAnimSpeed = 2f; // Increase animation speed for sprinting
        }
        Anim.SetFloat("Speed", CurrentAnimSpeed, 0.1f, Time.deltaTime); // Smoothly update movement speed parameter

        Anim.SetBool("IsGrounded", IsGrounded); // Update grounded state parameter

        Anim.SetFloat("VerticalVelocity", VerticalY); // Update vertical velocity parameter
    }

    #endregion

    #region === Movement Audio ===
    // Plays movement audio based on player state.
    private void HandleFootstepAudio()
    {
        if (PlayerFootstepAudio == null) return; 

        // MoveInput != Vector2.zero
        if (RB.linearVelocity.magnitude > 0.1f && IsGrounded)  // Check if the player is actually moving and on the ground
        {
            
            AudioClip targetClip = CanSprint ? RunningFootstepClip : FootstepClip;  // Choose the correct clip based on whether they are sprinting

            
            if (PlayerFootstepAudio.clip != targetClip || !PlayerFootstepAudio.isPlaying)  // Only change/play if the clip isn't already playing
            {
                PlayerFootstepAudio.clip = targetClip;
                PlayerFootstepAudio.loop = true;
                PlayerFootstepAudio.Play();
            }
        }
        else // If they aren't moving or are airborne, stop all footstep sounds
        {
            if (PlayerFootstepAudio.isPlaying)
            {
                PlayerFootstepAudio.Stop();
            }
        }
    }
    #endregion

    private void FixedUpdate()
    {
        Movement(); // Call the movement method in FixedUpdate for consistent physics updates
        CheckGround(); // Check if the player is grounded
        HandleStepUp(); // Handle stepping up onto stairs or ledges
        ApplyGravity(); // Apply custom gravity to the player
        UpdateAnimations(); // Update animator parameters based on current state
        HandleStamina(); // Manage stamina drain and regeneration
        HandleFootstepAudio(); // Manage footstep audio based on movement and grounded state
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red; // Change color based on grounded state

        Gizmos.DrawWireSphere( // Draw wireframe sphere
            transform.position + Vector3.up * GroundCheckOffset, // Sphere position
            GroundCheckRadius // Sphere radius
        );

        // --- 2. Step Up Detection Rays ---
        // Only draw the step rays if the player is actively moving, otherwise default to forward
        Vector3 debugDir = MoveDirection.magnitude > 0.1f 
            ? new Vector3(MoveDirection.x, 0, MoveDirection.z).normalized 
            : transform.forward;

        // Lower Ray (Ankle/Shin Height)
        Vector3 lowerOrigin = transform.position + Vector3.up * 0.05f;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(lowerOrigin, debugDir * StepCheckDistance);
        Gizmos.DrawSphere(lowerOrigin, 0.03f); // Tiny sphere at the origin

        // Upper Ray (Clearance Height)
        Vector3 upperOrigin = transform.position + Vector3.up * (MaxStepHeight + 0.05f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(upperOrigin, debugDir * StepCheckDistance);
        Gizmos.DrawSphere(upperOrigin, 0.03f); // Tiny sphere at the origin
    }

}
