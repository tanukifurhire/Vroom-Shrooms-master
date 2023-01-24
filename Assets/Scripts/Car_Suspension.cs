using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_Suspension : MonoBehaviour
{
    [SerializeField] private Transform flWheel;
    private float flLastDistance = 0f;
    [SerializeField] private Transform frWheel;
    private float frLastDistance = 0f;
    [SerializeField] private Transform rlWheel;
    private float rlLastDistance = 0f;
    [SerializeField] private Transform rrWheel;
    private float rrLastDistance = 0f;
    [SerializeField] private LayerMask roadLayer;
    [SerializeField] private float verticalOffset;

    [SerializeField] private float maxDistance = 0.5f;
    [SerializeField] private float minDistance = -0.5f;
    [SerializeField] private float springConstant = 1f;
    [SerializeField] private float maxForce = 10f;
    [SerializeField] private float dampingConstant = 0.1f;

    [SerializeField] private Rigidbody rb;
    private Car_Controller car;
    // Start is called before the first frame update
    void Start()
    {
        car = GetComponent<Car_Controller>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ApplyWheelSupport(flWheel, flLastDistance);
        ApplyWheelSupport(frWheel, frLastDistance);
        ApplyWheelSupport(rlWheel, rlLastDistance);
        ApplyWheelSupport(rrWheel, rrLastDistance);

        flLastDistance = GetWheelDistanceFromRest(flWheel);
        frLastDistance = GetWheelDistanceFromRest(frWheel);
        rlLastDistance = GetWheelDistanceFromRest(rlWheel);
        rrLastDistance = GetWheelDistanceFromRest(rrWheel);
    }

    void ApplyWheelSupport(Transform wheel, float lastDistance)
    {
        Vector3 supportForce = GetWheelSupportForce(wheel, lastDistance);
        rb.AddForceAtPosition(supportForce, wheel.position);
        Debug.DrawRay(wheel.position, supportForce);
    }

    Vector3 GetWheelSupportForce(Transform wheel, float lastDistance)
    {
        float currentDistance = GetWheelDistanceFromRest(wheel);
        float dampingForce = (currentDistance - lastDistance) / Time.fixedDeltaTime * dampingConstant;
        float magnitude = Mathf.Clamp(-currentDistance * springConstant - dampingForce, 0, maxForce);
        return magnitude * GetUpDir();
    }

    float GetWheelDistanceFromRest(Transform wheel)
    {
        RaycastHit hit;
        float rayLength = maxDistance - minDistance;
        Vector3 rayOrigin = (wheel.position - GetUpDir() * verticalOffset) - GetUpDir() * minDistance;
        bool hasHit = Physics.Raycast(rayOrigin, -GetUpDir(), out hit, rayLength, roadLayer);
        float distanceFromRest = hasHit ? hit.distance + minDistance : 0;
        return distanceFromRest;
    }

    Vector3 GetUpDir()
    {
        return transform.up;
    }
}
