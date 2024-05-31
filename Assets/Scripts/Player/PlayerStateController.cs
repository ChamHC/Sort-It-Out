using System;
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

    [Header("Camera Settings")]
    [SerializeField] public float sensitivity = 2f; // Camera sensitivity
    [SerializeField] public float constraintAngle = 80f; // Maximum angle for camera rotation
    [SerializeField] public float HeadBobAmount = 0.1f; // Head bobbing amount
    [SerializeField] public float HeadBobSpeed = 10f; // Head bobbing speed
    [NonSerialized] public float MouseX = 0f; // Mouse X-axis input
    [NonSerialized] public float MouseY = 0f; // Mouse Y-axis input
    [NonSerialized] public Transform Camera; // Reference to the camera transform
    private float rotationX = 0f; // Current rotation angle around the X-axis
    private Vector3 cameraDefaultPosition;  // Default camera position

    [Header("Controls Settings")]
    [SerializeField] public float MoveSpeed = 10f; // Player movement speed
    [SerializeField] public float SprintSpeed = 20f; // Player sprint speed
    [SerializeField] public float AirMultiplier = 0.1f; // Air movement multiplier
    [SerializeField] public float Drag = 10f; // Drag applied to the player's rigidbody
    [SerializeField] public float JumpForce = 6f; // Jump force
    [NonSerialized] public bool IsGrounded = false; // Check if the player is grounded
    [NonSerialized] public float Horizontal = 0f; // Horizontal input axis
    [NonSerialized] public float Vertical = 0f; // Vertical input axis
    private float timer;  // Timer for head bobbing

    [Header("Player State Machine")]
    private PlayerState currentState; // Current player state

    [Header("Physics Settings")]
    [NonSerialized] public Rigidbody Rigidbody; // Reference to the player's rigidbody

    #endregion

    void Start()
    {
        Camera = GetComponentInChildren<Camera>().transform; // Get the camera transform
        Rigidbody = GetComponent<Rigidbody>(); // Get the player's rigidbody

        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen

        currentState = new PlayerIdleState(); // Set the initial state to PlayerIdleState
        currentState.EnterState(this); // Enter the initial state

        cameraDefaultPosition = Camera.localPosition; // Get the default camera position
    }

    void Update()
    {
        CameraHandler(); // Handle camera rotation
        InputHandler(); // Handle player input
        MiscHandler(); // Handle miscellaneous tasks
        HeadBobHandler(); // Handle head bobbing

        currentState.Update(); // Update the current state
    }

    void FixedUpdate() {
        currentState.FixedUpdate(); // Perform physics-related updates in the current state
    }

    void CameraHandler(){
        MouseX = Input.GetAxis("Mouse X") * sensitivity; // Get the mouse X-axis input
        MouseY = Input.GetAxis("Mouse Y") * sensitivity; // Get the mouse Y-axis input

        rotationX -= MouseY; // Update the rotation around the X-axis
        rotationX = Mathf.Clamp(rotationX, -constraintAngle, constraintAngle); // Clamp the rotation angle

        Camera.localRotation = Quaternion.Euler(rotationX, 0f, 0f); // Apply the camera rotation
        transform.Rotate(Vector3.up * MouseX); // Rotate the player object around the Y-axis
    }

    void InputHandler(){
        Horizontal = Input.GetAxisRaw("Horizontal"); // Get the horizontal input axis
        Vertical = Input.GetAxisRaw("Vertical"); // Get the vertical input axis
    }

    void MiscHandler(){
        IsGrounded = GetComponentInChildren<CheckGround>().IsGrounded; // Check if the player is grounded

        if (IsGrounded) Rigidbody.drag = Drag; // Set the drag on the player's rigidbody
        else Rigidbody.drag = 0; // Reset the drag on the player's rigidbody
    }

    void HeadBobHandler(){
        if (!IsGrounded) return; // Return if the player is not grounded

        // Determine the head bobbing speed and amount based on the current player state
        float BobSpeed = currentState is PlayerMoveState ? HeadBobSpeed : HeadBobSpeed * 1.5f;
        float BobAmount = currentState is PlayerMoveState ? HeadBobAmount : HeadBobAmount * 1.5f;

        // Check if there is any horizontal or vertical input
        if(Horizontal != 0 || Vertical != 0)
        {
            timer += Time.deltaTime * BobSpeed; // Increment the timer based on the bobbing speed
            // Apply head bobbing effect by modifying the camera's local position
            Camera.localPosition = new Vector3(Camera.localPosition.x, cameraDefaultPosition.y + Mathf.Sin(timer) * BobAmount, Camera.localPosition.z);
        }
        else
        {
            timer = 0; // Reset the timer
            // Smoothly return the camera to its default position using Lerp
            Camera.localPosition = new Vector3(Camera.localPosition.x, Mathf.Lerp(Camera.localPosition.y, cameraDefaultPosition.y, Time.deltaTime * BobSpeed), Camera.localPosition.z);
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
    }

    public override void Update()
    {
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
        // Check if jump key is pressed
        if (Input.GetKey(player.JumpKey) && player.IsGrounded)
        {
            player.ChangeState(new PlayerJumpState()); // Change to the PlayerJumpState
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
    }

    public override void Update()
    {
        Check();
    }

    public override void FixedUpdate()
    {
        Vector3 moveDirection = player.Camera.rotation * new Vector3(player.Horizontal, 0, player.Vertical); // Calculate the movement direction
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
        // Check if jump key is pressed
        if (Input.GetKey(player.JumpKey) && player.IsGrounded)
        {
            player.ChangeState(new PlayerJumpState()); // Change to the PlayerJumpState
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
    }

    public override void Update()
    {
        Check();
    }

    public override void FixedUpdate()
    {
        // Check if the player is grounded and has not jumped
        if (!hasJump && player.IsGrounded) {
            player.Rigidbody.AddForce(Vector3.up * player.JumpForce, ForceMode.Impulse); // Apply an impulse force to the player
            hasJump = true; // Set hasJump to true
        }
    }

    public override void Check()
    {
        // Check if the player has jumped
        if (hasJump)
            player.ChangeState(new PlayerIdleState()); // Change to the PlayerIdleState
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
    }

    public override void Update()
    {
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
        // Check if jump key is pressed
        if (Input.GetKey(player.JumpKey) && player.IsGrounded)
        {
            player.ChangeState(new PlayerJumpState()); // Change to the PlayerJumpState
        }
    }

    public override void ExitState()
    {

    }
}