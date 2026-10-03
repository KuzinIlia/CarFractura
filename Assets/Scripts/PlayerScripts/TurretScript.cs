using UnityEngine;

public class TurretScript : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField] private Transform turretPivot;
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bulletPrefab;

    [SerializeField, Min(1f)] private float rotationSpeed = 180f;
    [SerializeField, Min(0.1f)] private float shotsPerSecond = 5f;
    [SerializeField] private bool isRunning = true;

    private float nextShotTime;
    private Quaternion initialRotation;

    private void Awake()
    {
        if (aimCamera == null)
            aimCamera = Camera.main;

        initialRotation = turretPivot.localRotation;
    }

    public void SetRunning(bool value)
    {
        if (value && !isRunning)
            nextShotTime = Time.time;

        isRunning = value;
    }

    private void Update()
    {
        if (!isRunning || Time.deltaTime <= 0f)
            return;

        if (!aimCamera.pixelRect.Contains((Vector2)Input.mousePosition))
            return;

        Ray ray = aimCamera.ScreenPointToRay(Input.mousePosition);
        Plane aimPlane = new Plane(Vector3.up, muzzle.position);

        if (!aimPlane.Raycast(ray, out float distance))
            return;

        Vector3 targetPoint = ray.GetPoint(distance);
        Vector3 direction = targetPoint - turretPivot.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Transform parent = turretPivot.parent;
        Vector3 localDirection = parent != null ? parent.InverseTransformDirection(direction) : direction;
        float targetAngle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
        turretPivot.localRotation = Quaternion.RotateTowards(turretPivot.localRotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + 1f / Mathf.Max(shotsPerSecond, 0.1f);
        }
    }

    private void Shoot()
    {
        Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
    }

    public void ResetTurret()
    {
        isRunning = false;
        nextShotTime = 0f;
        turretPivot.localRotation = initialRotation;
    }
}
