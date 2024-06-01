using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    #region Variables

    [Header("Debug Settings")]
    [SerializeField] public bool CheckStates = false; // Enable debug mode

    [Header("Keybinds")]
    [SerializeField] public KeyCode JumpKey = KeyCode.Space; // Key to jump
    [SerializeField] public KeyCode SprintKey = KeyCode.LeftShift; // Key to sprint
    [SerializeField] public KeyCode CrouchKey = KeyCode.LeftControl; // Key to crouch

    [Header("Camera Settings")]
    [SerializeField] public float sensitivity = 2f; // Camera sensitivity
    [SerializeField] public float constraintAngle = 60f; // Maximum angle for camera rotation
    [NonSerialized] public float MouseX = 0f; // Mouse X-axis input
    [NonSerialized] public float MouseY = 0f; // Mouse Y-axis input
    [NonSerialized] public Transform Camera; // Reference to the camera transform
    [NonSerialized] public Transform CameraTarget; // Reference to the camera target transform


    [Header("Controls Settings")]
    [SerializeField] public float MoveSpeed = 5f; // Player movement speed
    [SerializeField] public float SprintSpeed = 10f; // Player sprint speed
    [SerializeField] public float CrouchSpeed = 2.5f; // Player crouch speed
    [SerializeField] public float AirMultiplier = 0.1f; // Air movement multiplier
    [SerializeField] public float Drag = 10f; // Drag applied to the player's rigidbody
    [SerializeField] public float JumpForce = 6f; // Jump force
    [SerializeField] public float JumpDelay = 0.05f; // Delay before the player can jump again
    [NonSerialized] public bool IsReadyToJump = false; // Check if the player is ready to jump
    [NonSerialized] public bool isLeftFootGrounded = false; // Check if the left foot is grounded
    [NonSerialized] public bool isRightFootGrounded = false; // Check if the right foot is grounded
    [NonSerialized] public bool IsGrounded = false; // Check if the player is grounded
    [NonSerialized] public float Horizontal = 0f; // Horizontal input axis
    [NonSerialized] public float Vertical = 0f; // Vertical input axis
    private float timer = 0f; // Timer to check if the player is ready to jump

    [Header("Player State Machine")]
    private PlayerState currentState; // Current player state

    [Header("Physics Settings")]
    [NonSerialized] public Rigidbody Rigidbody; // Reference to the player's rigidbody
    [NonSerialized] public CapsuleCollider Collider; // Reference to the player's collider
    [NonSerialized] public GameObject PlayerNeck; // Reference to the player's neck
    [NonSerialized] public float DefaultHeight; // Default height of the player


    [Header("Animation Settings")]
    [NonSerialized] public Animator Animator; // Reference to the player's animator
    [SerializeField] public Dictionary<string, int> Animation; // Dictionary to store animations

    #endregion

    void Start()
    {
        Camera = GameObject.Find("Main Camera").transform; // Get the camera transform
        CameraTarget = GameObject.Find("Camera Target").transform; // Get the camera target transform
        Rigidbody = GetComponent<Rigidbody>(); // Get the player's rigidbody
        Animator = GetComponentInChildren<Animator>(); // Get the player's animator
        Collider = GetComponentInChildren<CapsuleCollider>(); // Get the player's collider
        PlayerNeck = GameObject.Find("mixamorig6:Neck"); // Get the player's neck
        Animation = new Dictionary<string, int>() // Initialize the animation dictionary
        {
            { "Move"  , 0 },
            { "Sprint", 1 },
            { "Crouch", 2 },
            { "Jump"  , 3 }
        };
        DefaultHeight = Collider.height; // Get the default height of the player

        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen

        currentState = new PlayerIdleState(); // Set the initial state to PlayerIdleState
        currentState.EnterState(this); // Enter the initial state
    }

    void Update()
    {
        InputHandler(); // Handle player input
        MiscHandler(); // Handle miscellaneous tasks

        currentState.Update(); // Update the current state
    }

    void FixedUpdate() {
        currentState.FixedUpdate(); // Perform physics-related updates in the current state
    }

    void LateUpdate() {
        CameraHandler(); // Handle camera rotation
    }

    void CameraHandler(){
        MouseX = Input.GetAxis("Mouse X") * sensitivity; // Get the mouse X-axis input
        MouseY = Input.GetAxis("Mouse Y") * sensitivity; // Get the mouse Y-axis input

        // Rotate the player's body based on the mouse X-axis input
        if (currentState is PlayerMoveState || currentState is PlayerSprintState || currentState is PlayerCrouchState){ 
            if (Horizontal != 0 || Vertical != 0) // Check if there is any horizontal or vertical input
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, Camera.eulerAngles.y, 0), Time.deltaTime * 10f); // Smoothly interpolate the player's rotation
            }
        }
    }

    void InputHandler(){
        Horizontal = Input.GetAxisRaw("Horizontal"); // Get the horizontal input axis
        Vertical = Input.GetAxisRaw("Vertical"); // Get the vertical input axis

        float targetInputH = Input.GetAxis("Horizontal"); // Get the target horizontal input axis
        float targetInputV = Input.GetAxis("Vertical"); // Get the target vertical input axis
        // Smoothly interpolate the values of InputH and InputV over time
        float InputH = Mathf.Lerp(Animator.GetFloat("InputH"), targetInputH, Time.deltaTime * 10);
        float InputV = Mathf.Lerp(Animator.GetFloat("InputV"), targetInputV, Time.deltaTime * 10);
        Animator.SetFloat("InputH", InputH); // Set the horizontal input parameter in the animator
        Animator.SetFloat("InputV", InputV); // Set the vertical input parameter in the animator
    }

    void MiscHandler(){
        if (!isLeftFootGrounded && !isRightFootGrounded) IsGrounded = false; // Check if both feet are not grounded
        else IsGrounded = true; // Set IsGrounded to true

        if (IsGrounded) Rigidbody.drag = Drag; // Set the drag on the player's rigidbody
        else Rigidbody.drag = 0; // Reset the drag on the player's rigidbody

        if (currentState is PlayerIdleState || currentState is PlayerMoveState || currentState is PlayerSprintState || currentState is PlayerCrouchState){
            if (!IsReadyToJump) // Check if the player is not ready to jump
            {
                timer += Time.deltaTime; // Increment the timer
                if (timer >= JumpDelay) // Check if the timer is greater than or equal to the jump delay
                {
                    IsReadyToJump = true; // Set IsReadyToJump to true
                    timer = 0; // Reset the timer
                }
            }
        }
    }

    public void ChangeState(PlayerState newState){
        currentState.ExitState(); // Exit the current state
        currentState = newState; // Set the new state
        currentState.EnterState(this); // Enter the new state
    }
}

public abstract class PlayerState
{
    protected PlayerStateController player { get; private set; } // Reference to the PlayerStateController
    public void EnterState(PlayerStateController player)
    {
        this.player = player; // Set the reference to the PlayerStateController
        Start(); // Perform state-specific initialization
    }
    public abstract void Start(); // Called when entering the state
    public abstract void Update(); // Called every frame
    public abstract void FixedUpdate(); // Called every fixed frame update
    public abstract void Check(); // Perform state-specific checks
    public abstract void ExitState(); // Called when exiting the state
}

public class PlayerIdleState : PlayerState
{
    public override void Start()
    {
        if (player.CheckStates) Debug.Log("Player is Idle");

        player.Animator.SetInteger("State", player.Animation["Move"]); // Set the animation state to Move
    }

    public override void Update()
    {
        player.Collider.height = Mathf.Lerp(player.Collider.height, player.DefaultHeight, Time.deltaTime * 20); // Smoothly interpolate the collider height
        Check();
    }

    public override void FixedUpdate()
    {

    }

    public override void Check()
    {
        // Check if there is any horizontal or vertical input
        if (player.Horizontal != 0 || player.Vertical != 0)
        {
            player.ChangeState(new PlayerMoveState()); // Change to the PlayerMoveState
        }
        // Check if jump key is pressed, if player is grounded, and if player animation is in transition
        if (Input.GetKey(player.JumpKey) && player.IsGrounded &&
            !player.Animator.GetAnimatorTransitionInfo(0).IsName("Jump -> Move"))
        {
            player.ChangeState(new PlayerJumpState()); // Change to the PlayerJumpState
        }
        // Check if crouch key is pressed
        if (Input.GetKey(player.CrouchKey))
        {
            player.ChangeState(new PlayerCrouchState()); // Change to the PlayerCrouchState
        }
    }

    public override void ExitState()
    {

    }
}

public class PlayerMoveState : PlayerState
{
    public override void Start()
    {
        if (player.CheckStates) Debug.Log("Player is Moving");

        player.Animator.SetInteger("State", player.Animation["Move"]); // Set the animation state to Move
    }

    public override void Update()
    {
        player.Collider.height = Mathf.Lerp(player.Collider.height, player.DefaultHeight, Time.deltaTime * 20); // Smoothly interpolate the collider height
        Check();
    }

    public override void FixedUpdate()
    {
        Vector3 moveDirection = player.CameraTarget.rotation * new Vector3(player.Horizontal, 0, player.Vertical); // Calculate the movement direction
        moveDirection.y = 0;    // Prevent the player from moving up or down

        player.Rigidbody.AddForce(moveDirection.normalized * player.MoveSpeed * (player.IsGrounded ? 1f : player.AirMultiplier) * 10f); // Move the player
    }

    public override void Check()
    {
        // Check if there is no horizontal and vertical input
        if (player.Horizontal == 0 && player.Vertical == 0)
        {
            player.ChangeState(new PlayerIdleState()); // Change to the PlayerIdleState
        }
        // Check if sprint key is pressed
        if (Input.GetKey(player.SprintKey))
        {
            player.ChangeState(new PlayerSprintState()); // Change to the PlayerSprintState
        }
        // Check if jump key is pressed, if player is grounded, and if player animation is in transition
        if (Input.GetKey(player.JumpKey) && player.IsGrounded &&
            !player.Animator.GetAnimatorTransitionInfo(0).IsName("Jump -> Move"))
        {
            player.ChangeState(new PlayerJumpState()); // Change to the PlayerJumpState
        }
        // Check if crouch key is pressed
        if (Input.GetKey(player.CrouchKey))
        {
            player.ChangeState(new PlayerCrouchState()); // Change to the PlayerCrouchState
        }
    }

    public override void ExitState()
    {

    }
}

public class PlayerJumpState : PlayerState
{
    private bool hasJump = false;   // Check if the player has jumped

    public override void Start()
    {
        if (player.CheckStates) Debug.Log("Player is Jumping");

        player.Animator.SetInteger("State", player.Animation["Jump"]); // Set the animation state to Jump
    }

    public override void Update()
    {
        player.Collider.height = Mathf.Lerp(player.Collider.height, player.DefaultHeight * 0.8f, Time.deltaTime * 20); // Smoothly interpolate the collider height
    }

    public override void FixedUpdate()
    {
        Vector3 moveDirection = player.Camera.rotation * new Vector3(player.Horizontal, 0, player.Vertical); // Calculate the movement direction
        moveDirection.y = 0;    // Prevent the player from moving up or down

        player.Rigidbody.AddForce(moveDirection.normalized * player.SprintSpeed * (player.IsGrounded ? 1f : player.AirMultiplier) * 10f); // Move the player

        // Check if the player is grounded and has not jumped
        if (!hasJump && player.IsGrounded && player.Rigidbody.velocity.y <= 0.05f) {
            player.Rigidbody.AddForce(Vector3.up * player.JumpForce * 2f, ForceMode.Impulse); // Apply an impulse force to the player
            hasJump = true; // Set hasJump to true
        }
        Check();
    }

    public override void Check()
    {
        // Check if the player animation is finished and there is no horizontal or vertical input
        if (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && player.Horizontal == 0 && player.Vertical == 0)
        {
            player.ChangeState(new PlayerIdleState()); // Change to the PlayerIdleState
        }
        // Check if the player animation is finished and there is horizontal or vertical input
        if (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && (player.Horizontal != 0 || player.Vertical != 0))
        {
            player.ChangeState(new PlayerMoveState()); // Change to the PlayerMoveState
        }
    }

    public override void ExitState()
    {

    }
}

public class PlayerSprintState : PlayerState
{
    public override void Start()
    {
        if (player.CheckStates) Debug.Log("Player is Sprinting");

        player.Animator.SetInteger("State", player.Animation["Sprint"]); // Set the animation state to Sprint
    }

    public override void Update()
    {
        player.Collider.height = Mathf.Lerp(player.Collider.height, player.DefaultHeight * 0.9f, Time.deltaTime * 20); // Smoothly interpolate the collider height
        Check();
    }

    public override void FixedUpdate()
    {
        Vector3 moveDirection = player.Camera.rotation * new Vector3(player.Horizontal, 0, player.Vertical); // Calculate the movement direction
        moveDirection.y = 0;    // Prevent the player from moving up or down

        player.Rigidbody.AddForce(moveDirection.normalized * player.SprintSpeed * (player.IsGrounded ? 1f : player.AirMultiplier) * 10f); // Move the player
    }

    public override void Check()
    {
        // Check if there is no horizontal and vertical input
        if (player.Horizontal == 0 && player.Vertical == 0)
        {
            player.ChangeState(new PlayerIdleState()); // Change to the PlayerIdleState
        }
        // check if sprint key is released
        if (!Input.GetKey(player.SprintKey))
        {
            player.ChangeState(new PlayerMoveState()); // Change to the PlayerMoveState
        }
        // Check if jump key is pressed, if player is grounded, and if player animation is in transition
        if (Input.GetKey(player.JumpKey) && player.IsGrounded &&
            !player.Animator.GetAnimatorTransitionInfo(0).IsName("Jump -> Sprint"))
        {
            player.ChangeState(new PlayerJumpState()); // Change to the PlayerJumpState
        }
        // Check if crouch key is pressed
        if (Input.GetKey(player.CrouchKey))
        {
            player.ChangeState(new PlayerCrouchState()); // Change to the PlayerCrouchState
        }
    }

    public override void ExitState()
    {

    }
}

public class PlayerCrouchState : PlayerState
{
    public override void Start()
    {
        if (player.CheckStates) Debug.Log("Player is Crouching");

        player.Animator.SetInteger("State", player.Animation["Crouch"]); // Set the animation state to Crouch
    }

    public override void Update()
    {
        player.Collider.height = Mathf.Lerp(player.Collider.height, player.DefaultHeight * 0.7f, Time.deltaTime * 20); // Smoothly interpolate the collider height
        Check();
    }

    public override void FixedUpdate()
    {
        Vector3 moveDirection = player.Camera.rotation * new Vector3(player.Horizontal, 0, player.Vertical); // Calculate the movement direction
        moveDirection.y = 0;    // Prevent the player from moving up or down

        player.Rigidbody.AddForce(moveDirection.normalized * player.CrouchSpeed * (player.IsGrounded ? 1f : player.AirMultiplier) * 10f); // Move the player
    }

    public override void Check()
    {
        // Check if crouch key is released
        if (!Input.GetKey(player.CrouchKey))
        {
            player.ChangeState(new PlayerIdleState()); // Change to the PlayerIdleState
        }
        // Check if jump key is pressed, the player is grounded and the player animation is in transition
        if (Input.GetKey(player.JumpKey) && player.IsGrounded &&
            !player.Animator.GetAnimatorTransitionInfo(0).IsName("Jump -> Crouch"))
        {
            player.ChangeState(new PlayerJumpState()); // Change to the PlayerJumpState
        }
    }

    public override void ExitState()
    {

    }
}