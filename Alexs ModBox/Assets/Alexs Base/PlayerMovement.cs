using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private static readonly float Epsilon = 0.0254f * 0.03125f, unitToMeter = 0.0254f; // quake/source units to meters
    private float hostFrame, rampConsider, usedSpeed;

    private bool OnGround, WasGrounded, stepNextTick = false, IsCrouched = false, WasCrouched = false, CrouchInput = false, IsRunning = false, jumpButtonPressed = false, oldJumpButtonPressed = false;
    private Vector3 oldStepPos, oldStepVel, playerBoundingBox, directionWish; //for stepping
    [SerializeField] private TMP_Text SpeedText;

    [HideInInspector]
    public float yRotation, xRotation;
    [HideInInspector]
    public bool CantAccel = false, CantRotateCam = false, CantShoot = false;

    private Rigidbody rb;
    private BoxCollider col;

    private Vector2 moveDirection;

    [Header("Quake Unit = 0.0254m")]
    [Header("Player Sizes")]
    [Tooltip("Quake = 56u")][SerializeField] private int playerHeight = 56;
    [SerializeField] private int playerHeightCrouch = 28;

    [Tooltip("Quake = 32u")][SerializeField] private int playerWidthLength = 32;
    [Tooltip("Quake = 46u")][SerializeField] private int playerEyeHeight = 46;
    [SerializeField] private int playerEyeHeightCrouch = 18;

    [Tooltip("Quake = 10u")][SerializeField] private int accelerate = 10;
    [Tooltip("Source = 10u")][SerializeField] private int accelerateAir = 10;
    [Tooltip("Quake = 100u")][SerializeField] private int speedStop = 100;
    [Tooltip("Quake = 320u")][SerializeField] private int speedRun = 320;
    [SerializeField] private int speedWalk = 190;
    [SerializeField] private int speedCrouch = 64;
    [Tooltip("Quake = 30u")][SerializeField] private int speedAir = 30;
    [Tooltip("Quake = 800u")][SerializeField] private int gravity = 800;
    [Tooltip("Quake = 4")][SerializeField] private int friction = 4;
    [Tooltip("Quake = 18u")][SerializeField] private int stepSize = 18;
    [Tooltip("Quake = 270u")][SerializeField] private int jumpAccel = 270;
    [SerializeField] private int jumpAccelCrouch = 130;

    [SerializeField] private float rampDegrees = 45;

    [Header("Mouse Settings")]
    public float mouseSensitivity;
    [SerializeField] private float lookingUpDownClamp = 89.9f;

    [Header("Inputs")]
    [SerializeField] private InputActionReference inputMove;
    [SerializeField] private InputActionReference inputJump, inputLook, inputCrouch, inputRun;
    [Header("Misc")]
    public Transform Camera;

    public LayerMask GroundMask;

    private void OnEnable() // we subscribe to the inputs onEnable
    {
        inputMove.action.performed += readMoveInput;
        inputMove.action.canceled += readMoveInput;

        inputCrouch.action.performed += toggleCrouchInput;
        inputCrouch.action.canceled += toggleCrouchInput;

        inputRun.action.performed += toggleRunInput;
        inputRun.action.canceled += toggleRunInput;

        inputLook.action.performed += CameraMove;

        inputJump.action.performed += ToggleSpace;
        inputJump.action.canceled += ToggleSpace;
    }
    private void OnDestroy() // unsubscribe from them if we're destroyed
    {
        inputMove.action.performed -= readMoveInput;
        inputMove.action.canceled -= readMoveInput;

        inputCrouch.action.performed -= toggleCrouchInput;
        inputCrouch.action.canceled -= toggleCrouchInput;

        inputRun.action.performed -= toggleRunInput;
        inputRun.action.canceled -= toggleRunInput;

        inputLook.action.performed -= CameraMove;

        inputJump.action.performed -= ToggleSpace;
        inputJump.action.canceled -= ToggleSpace;
    }
    private void OnDisable() // unsubscribe from them if we're disabled
    {
        inputMove.action.performed -= readMoveInput;
        inputMove.action.canceled -= readMoveInput;

        inputCrouch.action.performed -= toggleCrouchInput;
        inputCrouch.action.canceled -= toggleCrouchInput;

        inputRun.action.performed -= toggleRunInput;
        inputRun.action.canceled -= toggleRunInput;

        inputLook.action.performed -= CameraMove;

        inputJump.action.performed -= ToggleSpace;
        inputJump.action.canceled -= ToggleSpace;
    }

    void Awake() // initializing some stuff
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 240;
        rampConsider = Mathf.Cos(rampDegrees * Mathf.Deg2Rad);
        hostFrame = Time.fixedDeltaTime;

        playerEyeHeight = playerEyeHeight - playerHeight / 2;

        col = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        col.material.staticFriction = 0;
        col.material.dynamicFriction = 0;
        playerBoundingBox = new Vector3(playerWidthLength, playerHeight, playerWidthLength) * unitToMeter; // set it, cant set in when creating the value

        col.size = playerBoundingBox;

        Physics.gravity = new Vector3(0, -gravity * unitToMeter, 0);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Camera.transform.position = transform.position + Vector3.up * playerEyeHeight * unitToMeter;
    }
    void readMoveInput(InputAction.CallbackContext obj)
    {
        moveDirection = obj.ReadValue<Vector2>();
        moveDirection = moveDirection.magnitude<0.1f ? Vector2.zero : moveDirection;
    }
    #region Camera
    void CameraMove(InputAction.CallbackContext obj)
    {
        if (CantRotateCam) return;
        Vector2 mouseDelta = obj.ReadValue<Vector2>();
        float xSens = 10 * mouseSensitivity;
        float ySens = xSens;

        yRotation += mouseDelta.x * hostFrame * xSens;
        xRotation -= mouseDelta.y * hostFrame * ySens;
        xRotation = Mathf.Clamp(xRotation, -lookingUpDownClamp, lookingUpDownClamp);

        Camera.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    Vector3 RotateInputByCamera(Vector2 moveInput, Transform cameraTransform)
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Ignore vertical movement (Y-axis)
        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        return (right * moveInput.x + forward * moveInput.y);
    }
    #endregion
    void ToggleSpace(InputAction.CallbackContext obj)
    {
        jumpButtonPressed = !jumpButtonPressed;
    }
    void CheckGroundState()
    {
        WasGrounded = OnGround;
        //VisualiseBox.DisplayBoxCast(transform.position + Vector3.up * 0.4f * unitToMeter, col.bounds.extents, Vector3.down, transform.rotation, 0.5f * unitToMeter, Color.red);
        Physics.BoxCast(transform.position + Vector3.up * 0.4f * unitToMeter, col.bounds.extents, Vector3.down, out RaycastHit hitInfo, transform.rotation, 0.5f * unitToMeter, GroundMask);

        OnGround = hitInfo.normal.y > rampConsider;
        directionWish = OnGround ? Vector3.ProjectOnPlane(RotateInputByCamera(moveDirection, Camera).normalized, hitInfo.normal).normalized : RotateInputByCamera(moveDirection, Camera).normalized;

    }
    #region Movement
    void StepMove(Vector3 newVelocity) // it has some problems on sloped surfaces, i cant be bothered to fix them. ITS Good enough
    {
        if (stepNextTick)
        {
            rb.position = oldStepPos;
            rb.linearVelocity = oldStepVel;
            stepNextTick = false;
            return;
        }

        Vector3 velDir = newVelocity.normalized;
        float velDist = (newVelocity * hostFrame).magnitude;
        Vector3 startPos = rb.position;
        float maxHeight = stepSize * unitToMeter;
        Vector3 playerBox = col.bounds.extents;

        float directionDot = Mathf.Abs(Vector3.Dot(velDir, Vector3.right));
        playerBox.z *= Mathf.Lerp(1 - Epsilon, 1f, directionDot);
        playerBox.x *= Mathf.Lerp(1 - Epsilon, 1f, 1f - directionDot);

        velDist = velDist + col.bounds.extents.x * 2 * Epsilon; // these 4 make the player box a bit smaller so if we're right next to the wall we start a bit back

        bool hitWall = Physics.BoxCast(startPos, playerBox, velDir, out RaycastHit hitWallRay, transform.rotation, velDist + Epsilon, GroundMask);
        if (!hitWall) //if we didnt hit anything ignore
            return;

        if (hitWallRay.normal.y > rampConsider)//if it isnt a flat surface ignore
            return;

        float stepDist = hitWallRay.distance;

        if (Physics.BoxCast(startPos, playerBox, Vector3.up, out RaycastHit hitCeiling, transform.rotation, maxHeight, GroundMask))
            maxHeight = hitCeiling.distance; //if we hit a ceiling use the height we got

        bool cantStepOver = Physics.BoxCast(startPos + Vector3.up * maxHeight, playerBox, velDir, transform.rotation, stepDist + Epsilon, GroundMask);
        if (cantStepOver)
            return; // if we hit the step it means the ceiling is too small to fit
        //we cast down to find the step's height
        Physics.BoxCast(startPos + Vector3.up * maxHeight + velDir * (stepDist + Epsilon), playerBox, Vector3.down, out RaycastHit topCast, transform.rotation, stepSize * unitToMeter, GroundMask);

        if (topCast.distance >= maxHeight)
            return; //if somehow the step height is 0 or less we do nothing

        Vector3 endPos = startPos + Vector3.up * (maxHeight - topCast.distance) + newVelocity * hostFrame;//we move up to the step's height + epsilon + forward to the next pos

        stepNextTick = true;// we delay the stepping one tick to make it natural

        oldStepVel = new Vector3(newVelocity.x, 0, newVelocity.z).normalized * newVelocity.magnitude; //in case we go from a slope to a flat step, we dont go flying up
        oldStepPos = endPos;
    }

    void ApplyFriction()
    {
        float speed, newspeed, control;

        speed = rb.linearVelocity.magnitude / unitToMeter;
        if (speed < 1)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        control = (speed < speedStop) ? speedStop : speed;
        newspeed = speed - hostFrame * control * friction;

        if (newspeed < 0)
            newspeed = 0;
        newspeed /= speed;

        rb.linearVelocity = rb.linearVelocity * newspeed;
    }

    void Accelerate(float accel, float speed)
    {
        if (CantAccel) return;
        if (directionWish == Vector3.zero) return;

        Vector3 vel = rb.linearVelocity;
        float addspeed, accelspeed, currentspeed;

        currentspeed = Vector3.Dot(vel, directionWish.normalized) / unitToMeter;
        addspeed = speed - currentspeed;
        if (addspeed <= 0) return;

        accelspeed = accel * hostFrame * speedRun;
        if (accelspeed > addspeed) accelspeed = addspeed;

        rb.linearVelocity += accelspeed * unitToMeter * directionWish.normalized;
    }
    void CrouchFunc()
    {
        if (CantAccel) return;
        // Toggle crouch state only when input changes
        if (CrouchInput == WasCrouched) return;
        // Check if we can actually crouch/stand
        Vector3 size = new Vector3(playerWidthLength, playerHeight, playerWidthLength) * unitToMeter / 2;
        float dist = ((playerHeight - playerHeightCrouch) / 2) * unitToMeter;

        bool canChangeStance;
        if (IsCrouched)
            canChangeStance = !Physics.BoxCast(transform.position, size, Vector3.up, Quaternion.identity, dist*2, GroundMask); // Trying to stand up - check upwards
        else
            canChangeStance = !Physics.BoxCast(transform.position, size, OnGround ? Vector3.up : Vector3.down, Quaternion.identity, dist*2, GroundMask); // Trying to crouch - check in current direction based on ground state

        // Only change stance if we can
        if (canChangeStance)
            IsCrouched = !IsCrouched;

        // Update camera and collider based on current stance
        if (!IsCrouched)
        {
            // Standing
            if (OnGround) rb.position += Vector3.up * dist;

            Camera.transform.position = transform.position + Vector3.up * playerEyeHeight * unitToMeter;
            playerBoundingBox = new Vector3(playerWidthLength, playerHeight, playerWidthLength) * unitToMeter;
            col.size = playerBoundingBox;
        }
        else
        {
            // Crouching
            if (OnGround) rb.position -= Vector3.up * dist;

            Camera.transform.position = transform.position + Vector3.up * (playerEyeHeightCrouch * unitToMeter - dist);
            playerBoundingBox = new Vector3(playerWidthLength, playerHeightCrouch, playerWidthLength) * unitToMeter;
            col.size = playerBoundingBox;
        }

        // Update previous state
        WasCrouched = IsCrouched;
    }

    void WalkMove()
    {
        usedSpeed = IsCrouched ? speedCrouch : IsRunning ? speedRun : speedWalk;

        Accelerate(accelerate, usedSpeed);

        StepMove(rb.linearVelocity);
    }

    void AirMove()
    {
        Accelerate(accelerateAir, speedAir);
    }

    void CheckJump()
    {

        if (CantAccel) return;
        if (!OnGround) return;
        if (oldJumpButtonPressed) return; // if we're still holding space return
        oldJumpButtonPressed = true; // set it so we cant bunnyhop without trying
        float jumpaccel = IsCrouched ? jumpAccelCrouch : jumpAccel; // if crouched we jump less
        rb.linearVelocity += Vector3.up * jumpaccel * unitToMeter; // jump
        OnGround = false; WasGrounded = true;
    }

    void FullWalkMove()
    {
        if (OnGround)
        {
            ApplyFriction();
            WalkMove();
        }
        else
        {
            AirMove();
        }
    }



    #endregion
    private void FixedUpdate()
    {
        if (jumpButtonPressed)
            CheckJump();
        else
            oldJumpButtonPressed = false; // if not holding space reset this

        CrouchFunc();
        CheckGroundState();
        FullWalkMove();

        float speed = Mathf.Ceil(new Vector3(rb.linearVelocity.x,0,rb.linearVelocity.z).magnitude / unitToMeter/4)*4;
        SpeedText.text = speed.ToString();
    }

    void toggleCrouchInput(InputAction.CallbackContext obj)
    {
        CrouchInput = !CrouchInput;
    }
    void toggleRunInput(InputAction.CallbackContext obj)
    {
        IsRunning = !IsRunning;
    }

    
}
