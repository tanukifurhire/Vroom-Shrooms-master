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
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        car = GetComponent<Car_Controller>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (car.WheelsGrounded() || Vector3.Angle(Vector3.down, -transform.up.normalized) < maxAngle)
        {
            direction = -transform.up.normalized;
        }
        else
        {
            direction = Vector3.down;
        }
        rb.AddForceAtPosition(direction * (acceleration * 1f) * Time.fixedDeltaTime, flWheel.position, ForceMode.VelocityChange);
        rb.AddForceAtPosition(direction * (acceleration * 1f) * Time.fixedDeltaTime, frWheel.position, ForceMode.VelocityChange);
        rb.AddForceAtPosition(direction * (acceleration * 1f) * Time.fixedDeltaTime, rlWheel.position, ForceMode.VelocityChange);
        rb.AddForceAtPosition(direction * (acceleration * 1f) * Time.fixedDeltaTime, rrWheel.position, ForceMode.VelocityChange);
    }
}
