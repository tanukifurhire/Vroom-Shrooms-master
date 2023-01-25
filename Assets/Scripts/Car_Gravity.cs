using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_Gravity : MonoBehaviour
{
    [SerializeField] private float acceleration = 9.8f;
    [SerializeField] private Vector3 direction = Vector3.down;
    [SerializeField] private float maxAngle = 30;
    private Rigidbody rb;
    private Car_Controller car;
    [SerializeField] private Transform flWheel;
    [SerializeField] private Transform frWheel;
    [SerializeField] private Transform rlWheel;
    [SerializeField] private Transform rrWheel;
    [SerializeField] private List<Transform> wheels = new List<Transform>();
    private HashSet<Transform> liftedWheels = new HashSet<Transform>();
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        car = GetComponent<Car_Controller>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (liftedWheels.Count < 2 || Vector3.Angle(Vector3.down, -transform.up.normalized) < maxAngle)
        {
            direction = -transform.up.normalized;
        }
        else if (liftedWheels.Count >= 2 || Vector3.Angle(Vector3.down, -transform.up.normalized) > maxAngle)
        {
            direction = Vector3.down;
        }
        else
        {
            direction = Vector3.down;
        }

        AddForceToLiftedWheels();

        rb.AddForceAtPosition(direction * acceleration * Time.fixedDeltaTime, flWheel.position, ForceMode.VelocityChange);
        rb.AddForceAtPosition(direction * acceleration * Time.fixedDeltaTime, frWheel.position, ForceMode.VelocityChange);
        rb.AddForceAtPosition(direction * acceleration * Time.fixedDeltaTime, rlWheel.position, ForceMode.VelocityChange);
        rb.AddForceAtPosition(direction * acceleration * Time.fixedDeltaTime, rrWheel.position, ForceMode.VelocityChange);
    }

    void AddForceToLiftedWheels()
    {
        foreach (Transform w in wheels)
        {
            if (!car.WheelGroundedCheck(w) && !liftedWheels.Contains(w))
            {
                liftedWheels.Add(w);
                Debug.Log(w.name + "Is Not Grounded!");
            }
            if (car.WheelGroundedCheck(w) && liftedWheels.Contains(w))
            {
                liftedWheels.Remove(w);
            }
        }

        foreach (Transform w in liftedWheels)
        {
            rb.AddForceAtPosition(direction * acceleration * Time.fixedDeltaTime, w.position + transform.up, ForceMode.VelocityChange);
        }
    }
}
