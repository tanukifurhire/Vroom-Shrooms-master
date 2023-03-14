using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System;

[RequireComponent(typeof(PlayerInput))]
public class Car_Controller : MonoBehaviour
{
    public static Car_Controller Instance { get; private set; }
    public event EventHandler OnShoot;
    [Header("References")]
    [SerializeField] private Transform flWheel;
    private float flWheelSteerAngle;
    [SerializeField] private Transform frWheel;
    private float frWheelSteerAngle;

    [SerializeField] private Transform mlWheel;
    private float mlWheelSteerAngle;
    [SerializeField] private Transform mrWheel;
    private float mrWheelSteerAngle;
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
    private float mlDampedTargetRotationCurrentVelocity;
    private float mrDampedTargetRotationCurrentVelocity;
    private bool drift = false;
    [SerializeField] [Range(0, 250f)] private float tireGripFactor = .05f;
    [SerializeField] private float tireMass = 1f;

    private void Awake()
    {
        Instance = this;
        Input = GetComponent<PlayerInput>();
        componentBase = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        CarSuspension = GetComponent<Car_Suspension>();
    }
    // Start is called before the first frame update
    void Start()
    {
        Input.PlayerActions.Shoot.performed += Shoot_performed;
    }

    private void Shoot_performed(InputAction.CallbackContext obj)
    {
        OnShoot?.Invoke(this, EventArgs.Empty);
    }

    // Update is called once per frame
    void Update()
    {
        ReadMovementInput();
        Drift();
    }

    private void FixedUpdate()
    {
        rb.centerOfMass = centerOfMass.localPosition;
        CalculateAckermannAngle();
        AddSteerAngleToWheel();
        HandleSteering();
        MoveCar();
    }
    #region Main Methods
    void Grip()
    {
        var locVel = transform.InverseTransformDirection(rb.velocity);
        //rb.velocity -= transform.right * locVel.x;
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
            rb.AddForce((transform.forward.normalized * GetDriveForce()) / 2);
        }
        if (WheelGroundedCheck(rrWheel))
        {
            rb.AddForce((transform.forward.normalized * GetDriveForce()) / 2);
        }
        if (rb.velocity.magnitude >= 2.5f)
        {
            //rb.angularVelocity += -transform.up * GetSteeringAngularAcceleration() * Time.fixedDeltaTime;
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
        mlWheelSteerAngle = ackermannAngleLeft;
        mrWheelSteerAngle = ackermannAngleRight;

        float flWheelSmoothedSteerAngle = Mathf.SmoothDampAngle(flWheel.localEulerAngles.y, flWheelSteerAngle, ref flDampedTargetRotationCurrentVelocity, smoothInputSpeed * Time.deltaTime);
        float frWheelSmoothedSteerAngle = Mathf.SmoothDampAngle(frWheel.localEulerAngles.y, frWheelSteerAngle, ref frDampedTargetRotationCurrentVelocity, smoothInputSpeed * Time.deltaTime);
        float mlWheelSmoothedSteerAngle = Mathf.SmoothDampAngle(mlWheel.localEulerAngles.y, mlWheelSteerAngle, ref mlDampedTargetRotationCurrentVelocity, smoothInputSpeed * Time.deltaTime);
        float mrWheelSmoothedSteerAngle = Mathf.SmoothDampAngle(mrWheel.localEulerAngles.y, mrWheelSteerAngle, ref mrDampedTargetRotationCurrentVelocity, smoothInputSpeed * Time.deltaTime);

        flWheel.localRotation = Quaternion.Euler(flWheel.localRotation.x, flWheel.localRotation.y + flWheelSmoothedSteerAngle, transform.localRotation.z);
        frWheel.localRotation = Quaternion.Euler(frWheel.localRotation.x, frWheel.localRotation.y + frWheelSmoothedSteerAngle, transform.localRotation.z);
        mlWheel.localRotation = Quaternion.Euler(mlWheel.localRotation.x, mlWheel.localRotation.y + mlWheelSmoothedSteerAngle, transform.localRotation.z);
        mrWheel.localRotation = Quaternion.Euler(mrWheel.localRotation.x, mrWheel.localRotation.y + mrWheelSmoothedSteerAngle, transform.localRotation.z);
    }

    private void HandleSteering()
    {
        if (WheelGroundedCheck(frWheel) && WheelGroundedCheck(flWheel))
        {
            Vector3 flSteeringDir = flWheel.transform.right;
            Vector3 frSteeringDir = frWheel.transform.right;
            Vector3 mlSteeringDir = mlWheel.transform.right;
            Vector3 mrSteeringDir = mrWheel.transform.right;
            Vector3 rlSteeringDir = rlWheel.transform.right;
            Vector3 rrSteeringDir = rrWheel.transform.right;

            Vector3 frSpringWorldVel = rb.GetPointVelocity(frWheel.transform.position);
            Vector3 flSpringWorldVel = rb.GetPointVelocity(flWheel.transform.position);
            Vector3 mrSpringWorldVel = rb.GetPointVelocity(mrWheel.transform.position);
            Vector3 mlSpringWorldVel = rb.GetPointVelocity(mlWheel.transform.position);
            Vector3 rrSpringWorldVel = rb.GetPointVelocity(rrWheel.transform.position);
            Vector3 rlSpringWorldVel = rb.GetPointVelocity(rlWheel.transform.position);

            float flSteeringVel = Vector3.Dot(flSteeringDir, flSpringWorldVel);
            float frSteeringVel = Vector3.Dot(frSteeringDir, frSpringWorldVel);
            float mlSteeringVel = Vector3.Dot(mlSteeringDir, mlSpringWorldVel);
            float mrSteeringVel = Vector3.Dot(mrSteeringDir, mrSpringWorldVel);
            float rlSteeringVel = Vector3.Dot(rlSteeringDir, rlSpringWorldVel);
            float rrSteeringVel = Vector3.Dot(rrSteeringDir, rrSpringWorldVel);

            float flDesiredVelChange = -flSteeringVel * tireGripFactor;
            float frDesiredVelChange = -frSteeringVel * tireGripFactor;
            float mlDesiredVelChange = -mlSteeringVel * tireGripFactor;
            float mrDesiredVelChange = -mrSteeringVel * tireGripFactor;
            float rlDesiredVelChange = -rlSteeringVel * tireGripFactor;
            float rrDesiredVelChange = -rrSteeringVel * tireGripFactor;

            float flDesiredAccel = flDesiredVelChange / Time.fixedDeltaTime;
            float frDesiredAccel = frDesiredVelChange / Time.fixedDeltaTime;
            float mlDesiredAccel = mlDesiredVelChange / Time.fixedDeltaTime;
            float mrDesiredAccel = mrDesiredVelChange / Time.fixedDeltaTime;
            float rlDesiredAccel = rlDesiredVelChange / Time.fixedDeltaTime;
            float rrDesiredAccel = rrDesiredVelChange / Time.fixedDeltaTime;

            rb.AddForceAtPosition(flSteeringDir * tireMass * flDesiredAccel, flWheel.transform.position);
            rb.AddForceAtPosition(frSteeringDir * tireMass * frDesiredAccel, frWheel.transform.position);
            rb.AddForceAtPosition(mlSteeringDir * tireMass * mlDesiredAccel, flWheel.transform.position);
            rb.AddForceAtPosition(mrSteeringDir * tireMass * mrDesiredAccel, frWheel.transform.position);
            rb.AddForceAtPosition(rlSteeringDir * tireMass * rlDesiredAccel, rlWheel.transform.position);
            rb.AddForceAtPosition(rrSteeringDir * tireMass * rrDesiredAccel, rrWheel.transform.position);
        }
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
