using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class Car_Controller : MonoBehaviour
{
    [SerializeField] private Transform flWheel;
    private float flWheelSteerAngle;
    [SerializeField] private Transform frWheel;
    private float frWheelSteerAngle;
    [SerializeField] private Transform rlWheel;
    [SerializeField] private Transform rrWheel;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform groundTrigger;
    [SerializeField] private LayerMask wheelCollidables;

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

    private void Awake()
    {
        Input = GetComponent<PlayerInput>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ReadMovementInput();
        CalculateAckermannAngle();
        AddSteerAngleToWheel();
    }

    private void FixedUpdate()
    {
        rb.centerOfMass = centerOfMass.localPosition;

        MoveCar();
    }
    #region Main Methods
    void MoveCar()
    {
        if (!WheelsGrounded())
        {
            return;
        }

        rb.AddForce(rb.transform.forward.normalized * GetDriveForce());
        if (rb.velocity.magnitude >= 1f)
        {
            rb.angularVelocity += -transform.up * GetSteeringAngularAcceleration() * Time.fixedDeltaTime;
        }
    }
    float GetSteeringAngularAcceleration()
    {
        return -smoothedInput.x * maxAngularAcceleration * Mathf.PI / 180;
    }
    float GetDriveForce()
    {
        return steerInput.y * driveForce;
    }
    public bool WheelsGrounded()
    {
        return Physics.OverlapBox(groundTrigger.position, groundTrigger.localScale / 2, Quaternion.identity, wheelCollidables).Length > 0;
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

        Debug.Log(steerInput.y);
    }
    #endregion
}
