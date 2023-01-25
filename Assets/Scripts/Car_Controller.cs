using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent(typeof(PlayerInput))]
public class Car_Controller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform flWheel;
    private float flWheelSteerAngle;
    [SerializeField] private Transform frWheel;
    private float frWheelSteerAngle;
    [SerializeField] private Transform rlWheel;
    [SerializeField] private Transform rrWheel;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform groundTrigger;
    [SerializeField] private LayerMask wheelCollidables;
    private CinemachineComponentBase componentBase;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    public Car_Suspension CarSuspension { get; private set; }

    [Header("Specs")]
    [SerializeField] private float wheelBase;
    [SerializeField] private float rearTrack;
    [SerializeField] private float turnRadius;
    [SerializeField] private Transform centerOfMass;
    [SerializeField] private float driveForce;
    [SerializeField] private float maxAngularAcceleration = 30;

    [Header("Inputs")]
    private Vector2 steerInput;
    private Vector2 smoothedInput;
    private Vector2 smoothInputVelocity;
    private float smoothInputSpeed = 25f;

    public PlayerInput Input { get; private set; }

    private float ackermannAngleLeft;
    private float ackermannAngleRight;
    private float flDampedTargetRotationCurrentVelocity;
    private float frDampedTargetRotationCurrentVelocity;
    private bool drift = false;

    private void Awake()
    {
        Input = GetComponent<PlayerInput>();
        componentBase = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        CarSuspension = GetComponent<Car_Suspension>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ReadMovementInput();
        Drift();
        CalculateAckermannAngle();
        AddSteerAngleToWheel();
    }

    private void FixedUpdate()
    {
        rb.centerOfMass = centerOfMass.localPosition;

        MoveCar();
    }
    #region Main Methods
    void Grip()
    {
        var locVel = transform.InverseTransformDirection(rb.velocity);
        rb.velocity -= transform.right * locVel.x;
    }
    void Drift()
    {
        if (Input.PlayerActions.Drift.WasPressedThisFrame())
        {
            drift = true;
            if (componentBase is CinemachineTransposer)
            {
                (componentBase as CinemachineTransposer).m_YawDamping = 2.25f;
            }
            rb.AddForceAtPosition(5000f * transform.right * -steerInput.x, rlWheel.position);
            rb.AddForceAtPosition(5000f * transform.right * -steerInput.x, rrWheel.position);
            driveForce += 2000f;
        }
        if (Input.PlayerActions.Drift.WasReleasedThisFrame())
        {
            drift = false;
            if (componentBase is CinemachineTransposer)
            {
                (componentBase as CinemachineTransposer).m_YawDamping = .75f;
            }
            driveForce -= 2000f;
        }
    }
    void MoveCar()
    {
        if (WheelGroundedCheck(rlWheel))
        {
            rb.AddForceAtPosition((transform.forward.normalized * GetDriveForce()) / 2, rlWheel.position);
        }
        if (WheelGroundedCheck(rrWheel))
        {
            rb.AddForceAtPosition((transform.forward.normalized * GetDriveForce()) / 2, rrWheel.position);
        }
        if (rb.velocity.magnitude >= 2.5f)
        {
            rb.angularVelocity += -transform.up * GetSteeringAngularAcceleration() * Time.fixedDeltaTime;
            if (!drift)
            {
                Grip();
            }
        }
    }

    public bool WheelGroundedCheck(Transform wheel)
    {
        CarSuspension.IsGrounded(wheel, out bool hasHit, out RaycastHit hit);
        return hasHit;
    }
    float GetSteeringAngularAcceleration()
    {
        return -steerInput.x * maxAngularAcceleration * Mathf.PI / 180;
    }
    float GetDriveForce()
    {
        return steerInput.y * driveForce;
    }
    void CalculateAckermannAngle()
    {
        if (steerInput.x > 0) //turning right
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput.x;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput.x;
        }
        else if (steerInput.x < 0) //turning left
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput.x;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput.x;
        }
        else
        {
            ackermannAngleLeft = 0;
            ackermannAngleRight = 0;
        }
    }

    void AddSteerAngleToWheel()
    {
        flWheelSteerAngle = ackermannAngleLeft;
        frWheelSteerAngle = ackermannAngleRight;

        float flWheelSmoothedSteerAngle = Mathf.SmoothDampAngle(flWheel.localEulerAngles.y, flWheelSteerAngle, ref flDampedTargetRotationCurrentVelocity, smoothInputSpeed * Time.deltaTime);
        float frWheelSmoothedSteerAngle = Mathf.SmoothDampAngle(flWheel.localEulerAngles.y, flWheelSteerAngle, ref frDampedTargetRotationCurrentVelocity, smoothInputSpeed * Time.deltaTime);

        flWheel.localRotation = Quaternion.Euler(flWheel.localRotation.x, flWheel.localRotation.y + flWheelSmoothedSteerAngle, transform.localRotation.z);
        frWheel.localRotation = Quaternion.Euler(frWheel.localRotation.x, frWheel.localRotation.y + frWheelSmoothedSteerAngle, transform.localRotation.z);
    }
    #endregion

    #region Input Methods
    private void ReadMovementInput()
    {
        steerInput = new Vector2(Input.PlayerActions.WASD.ReadValue<Vector2>().x, Input.PlayerActions.WASD.ReadValue<Vector2>().y);

        smoothedInput = Vector2.SmoothDamp(smoothedInput, steerInput, ref smoothInputVelocity, smoothInputSpeed * Time.deltaTime);
    }
    #endregion
}
