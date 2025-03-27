using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private static readonly float Epsilon = 0.0254f * 0.03125f, unitToMeter = 0.0254f; // quake/source units to meters
    private float hostFrame, rampConsider;

    private bool OnGround, WasGrounded, stepNextTick = false;
    private Vector3 oldStepPos, oldStepVel, playerBoundingBox, directionWish; //for stepping


    [HideInInspector]
    public float yRotation, xRotation;
    [HideInInspector]
    public bool CanMoveCamera = true;

    private Rigidbody rb;
    private BoxCollider col;

    private Vector2 moveDirection;

    [Header("Quake Unit = 0.0254m")]
    [Header("Player Sizes")]
    [Tooltip("Quake = 56u")][SerializeField] private int playerHeight = 56;
    [Tooltip("Quake = 32u")][SerializeField] private int playerWidthLength = 32;
    [Tooltip("Quake = 46u")][SerializeField] private int playerEyeHeight = 46;
    [Tooltip("Quake = 10u")][SerializeField] private int accelerate = 10;
    [Tooltip("Source = 10u")][SerializeField] private int accelerateAir = 10;
    [Tooltip("Quake = 100u")][SerializeField] private int speedStop = 100;
    [Tooltip("Quake = 320u")][SerializeField] private int speedRun = 320;
    [Tooltip("Quake = 30u")][SerializeField] private int speedAir = 30;
    [Tooltip("Quake = 800u")][SerializeField] private int gravity = 800;
    [Tooltip("Quake = 4")][SerializeField] private int friction = 4;
    [Tooltip("Quake = 18u")][SerializeField] private int stepSize = 18;
    [Tooltip("Quake = 270u")][SerializeField] private int jumpAccel = 270;
    [SerializeField] private float rampDegrees = 45;

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = .5f;
    [SerializeField] private float lookingUpDownClamp = 89.9f;

    [Header("Inputs")]
    [SerializeField] private InputActionReference inputMove;
    [SerializeField] private InputActionReference inputJump, inputLook;
    [Header("Misc")]
    public Transform Camera;

    public LayerMask GroundMask;

    void Awake() // initializing some stuff
    {
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

        inputMove.action.performed += ctx => moveDirection = ctx.ReadValue<Vector2>();
        inputMove.action.canceled += ctx => moveDirection = Vector2.zero;

        inputLook.action.performed += ctx => CameraMove(ctx.ReadValue<Vector2>());

        inputJump.action.performed += playerJumpFunc;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Camera.transform.position = transform.position + Vector3.up * playerEyeHeight * unitToMeter;
    }
    #region Camera
    void CameraMove(Vector2 mouseDelta)
    {
        if (!CanMoveCamera) return;
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
    void CheckGroundState()
    {
        WasGrounded = OnGround;
        if (Physics.BoxCast(transform.position + Vector3.up * 0.4f * unitToMeter, col.bounds.extents, Vector3.down, out RaycastHit hitInfo, transform.rotation, 0.5f * unitToMeter, GroundMask))
        {
            OnGround = hitInfo.normal.y > rampConsider;
            directionWish = OnGround ? Vector3.ProjectOnPlane(RotateInputByCamera(moveDirection, Camera).normalized, hitInfo.normal).normalized : RotateInputByCamera(moveDirection, Camera).normalized;
        }
        else
        {
            directionWish = RotateInputByCamera(moveDirection, Camera).normalized; OnGround = false;
        }
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
        if (directionWish == Vector3.zero)
            return;

        Vector3 vel = rb.linearVelocity;
        float addspeed, accelspeed, currentspeed;

        currentspeed = Vector3.Dot(vel, directionWish.normalized) / unitToMeter;
        addspeed = speed - currentspeed;
        if (addspeed <= 0)
            return;

        accelspeed = accel * hostFrame * speedRun;
        if (accelspeed > addspeed) accelspeed = addspeed;

        rb.linearVelocity += accelspeed * unitToMeter * directionWish.normalized;
    }

    void WalkMove()
    {
        Accelerate(accelerate, speedRun);

        StepMove(rb.linearVelocity);
    }

    void AirMove()
    {
        Accelerate(accelerateAir, speedAir);
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
        CheckGroundState();
        FullWalkMove();
    }

    void playerJumpFunc(InputAction.CallbackContext obj)
    {
        if (OnGround)
        {
            OnGround = false;
            WasGrounded = true;

            rb.linearVelocity += Vector3.up * jumpAccel * unitToMeter;
        }
        else return;
    }
}
