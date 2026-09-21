using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputBridge))] // Forces both files to exist together

public class PhysicsMotor : MonoBehaviour
{
    [Header("Run Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float acceleration = 12f; // Higher = snappier stops
    [SerializeField] private float turnSpeed = 15f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 6.5f;
    [SerializeField] private float groundCheckDistance = 2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Dodge Settings")]
    [SerializeField] private float dashForce = 20f;     // how fast the dash propels you
    [SerializeField] private float dashCooldown = 0.8f; 
    [SerializeField] private float dashDuration = 0.2f;
    private float nextDashTime = 0f;
    private bool isDashing = false;

    private Rigidbody rb;
    private PlayerInputBridge input;
    private Vector3 targetDirection = Vector3.zero;
    private bool isGrounded;
    private Transform mainCameraTransform; // Reference for where camera is looking
    private Animator animator;  // animation state tracker loops

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInputBridge>();
        rb.useGravity = true;
        animator = GetComponent<Animator>();

        // Automatically finds the main camera in your scene
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    // Activate Input Bridge event when active
    private void OnEnable()
    {
        if (input == null) input = GetComponent<PlayerInputBridge>();
        input.OnDodgeTriggered += ExecuteDodgeDash;
    }

    // Deactivate when disabled to prevent system memory leaks
    private void OnDisable()
    {
        if (input != null) input.OnDodgeTriggered -= ExecuteDodgeDash;
    }

    private void ExecuteDodgeDash()
    {
        // Check cooldown boundaries
        if (Time.time < nextDashTime || !isGrounded) return;

        Debug.Log("Dashing!");
        nextDashTime = Time.time + dashCooldown;
        isDashing = true;

        // Choose dash direction: use active running direction, or default to player's face forward direction
        Vector3 dashDir = targetDirection != Vector3.zero ? targetDirection : transform.forward;
        // Zero out current velocity for a snappy, consistent starting burst
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        // Apply sudden physics burst forward
        rb.AddForce(dashDir * dashForce, ForceMode.Impulse);
        // Trigger a dodge animation state instantly if an animator controller exists
        if (animator != null) animator.SetTrigger("DodgeTrigger");
        // Reset the dashing state after the short duration expires
        Invoke(nameof(ResetDashState), dashDuration);
    }

    private void ResetDashState()
    {
        isDashing = false;
    }

    void FixedUpdate()
    {
        // Calculate raw keyboard input direction
        Vector3 rawInputDirection = new Vector3(input.MoveInput.x, 0f, input.MoveInput.y);

        // CRITICAL: Translate raw inputs to align with the camera view matrix
        if (rawInputDirection != Vector3.zero && mainCameraTransform != null)
        {
            // Extract the camera's forward and right vectors
            Vector3 camForward = mainCameraTransform.forward;
            Vector3 camRight = mainCameraTransform.right;

            // Flatten them on the Y-axis so looking down at the ground doesn't make you walk slower
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            // Blend inputs with camera angles: W/S matches Cam Forward, A/D matches Cam Right
            targetDirection = (camForward * rawInputDirection.z + camRight * rawInputDirection.x).normalized;
        }
        
        // Process movement velocities ONLY if we are not actively in the middle of a dash impulse burst
        if (!isDashing)
        {
            float currentTargetSpeed = input.IsSprinting ? sprintSpeed : walkSpeed;
            Vector3 targetVelocity = targetDirection * currentTargetSpeed;

            targetVelocity.y = rb.linearVelocity.y;

            // Blends normal walking/jogging tracking velocities
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // If we aren't touching any keys, clear the vector so physics drop back to 0
            targetDirection = Vector3.zero;
        }

        // Smoothly rotate character to face the direction they are actively moving
        if (targetDirection != Vector3.zero && !isDashing)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        }

        // Ground checking and jumping loops
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        
        if (input.JumpTriggered)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
                Debug.Log("Physics Jump Launched!");
            }
            input.JumpTriggered = false;
        }

        if (animator != null)
        {
            // 1. Calculate how fast our physics body is actually sliding across the flat ground
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float currentMoveSpeed = horizontalVelocity.magnitude;

            // 2. Smoothly blend the current animation value to match real speeds to prevent jarring snapping transitions
            float currentAnimSpeedValue = animator.GetFloat("Speed");
            float smoothSpeedBlend = Mathf.Lerp(currentAnimSpeedValue, currentMoveSpeed, 10f * Time.fixedDeltaTime);

            // 3. Inject parameter values directly over into the animation runtime engine
            animator.SetFloat("Speed", smoothSpeedBlend);
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}