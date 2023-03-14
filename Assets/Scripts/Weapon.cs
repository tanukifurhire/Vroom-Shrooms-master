using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Transform shootingPoint;
    [SerializeField] private LayerMask whatIsReference;

    [field: Header("Targets")]
    public List<Transform> screenTargets = new List<Transform>();
    public Transform target;
    public Image aim;
    public Vector2 uiOffset;

    [field: Header("Refs")]
    [SerializeField] private Transform bulletPrefab;
    [SerializeField] private LayerMask aimLayerMask;
    private Vector3 playerAimDirection;
    [SerializeField] private float projectileSpeed;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Car_Controller.Instance.OnShoot += CarController_OnShoot;
    }

    private void CarController_OnShoot(object sender, EventArgs e)
    {
        Fire();
    }

    private void Fire()
    {
        Debug.Log("Fire!");
        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimLayerMask))
        {
            playerAimDirection = raycastHit.point;
        }
        var aimDirection = ((playerAimDirection - shootingPoint.position)).normalized;
        var aimRotation = Quaternion.LookRotation(aimDirection, Vector3.up);
        //Instantiate a copy of our projectile and store it in a new rigidbody variable called clonedBullet
        Transform clonedBullet = Instantiate(bulletPrefab, shootingPoint.position, aimRotation);
        Physics.IgnoreCollision(clonedBullet.GetComponent<Collider>(), Car_Controller.Instance.GetComponent<Collider>());
        clonedBullet.GetComponent<Rigidbody>().velocity = clonedBullet.transform.forward * projectileSpeed;
    }

    private void Update()
    {
        Quaternion lookAtRotation = Quaternion.AngleAxis(Camera.main.transform.localEulerAngles.y, Vector3.up);
        transform.rotation = lookAtRotation;
    }

    private void LateUpdate()
    {
        if (screenTargets.Count > 0)
        {
            target = screenTargets[targetIndex()];
        }
        else
        {
            target = null;
        }
        UserInterface();
    }

    public int targetIndex()
    {
        float[] distances = new float[screenTargets.Count];

        for (int i = 0; i < screenTargets.Count; i++)
        {
            distances[i] = Vector2.Distance(Camera.main.WorldToScreenPoint(screenTargets[i].position), new Vector2(Screen.width / 2, Screen.height / 2));
        }

        float minDistance = Mathf.Min(distances);
        int index = 0;

        for (int i = 0; i < distances.Length; i++)
        {
            if (minDistance == distances[i])
                index = i;
        }

        return index;
    }

    private void UserInterface()
    {
        Color c = screenTargets.Count < 1 ? Color.clear : Color.red;
        aim.color = c;
        if (target == null)
        {
            return;
        }
        aim.transform.position = Camera.main.WorldToScreenPoint(target.position + (Vector3)uiOffset);
    }
}
