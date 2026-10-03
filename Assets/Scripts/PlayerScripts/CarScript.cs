using UnityEngine;

public class CarScript : MonoBehaviour
{

    [SerializeField] private Transform car;
    [SerializeField] private Transform[] wheels;
    [SerializeField] private Transform carVisual;
    [SerializeField] private Transform steeringFL;
    [SerializeField] private Transform steeringFR;
    [SerializeField] private Rigidbody carBody;

    [SerializeField, Min(0.01f)] private float lateralSpeed = 2f;
    [SerializeField, Min(0.01f)] private float steeringSmoothTime = 0.2f;
    [SerializeField, Min(0.01f)] private float returnSmoothTime = 0.5f;
    [SerializeField, Min(0f)] private float amplitude = 1.5f;
    [SerializeField, Min(0f)] private float maxSteeringAngle = 25f;
    [SerializeField, Min(0f)] private float maxBodyAngle = 6f;
    [SerializeField, Min(0f)] private float worldSpeed = 5f;

    [SerializeField] private Vector2 pauseRange = new(0.2f, 0.7f);
    [SerializeField, Min(0.01f)] private float wheelRadius = 0.35f;

    [SerializeField] private Vector3 wheelAxis = Vector3.right;

    private Quaternion initialBodyRotation;
    private Quaternion initialSteeringFL;
    private Quaternion initialSteeringFR;

    private float steeringInput;

    private readonly int[] route = { -1, 0, 1, 0 };

    private Vector3 previousVisualPosition;
    private Vector3 startPosition;
    private int targetIndex;
    private float pauseRemaining;
    private bool isRunning = true;
    private float steeringVelocity;
    private Vector3 initialVisualPosition;

    private void Awake()
    {
        car = carBody.transform;

        startPosition = previousVisualPosition = car.position;

        if (carVisual != null)
        {
            initialVisualPosition = carVisual.localPosition;
            initialBodyRotation = carVisual.localRotation;
        }

        if (steeringFL != null)
            initialSteeringFL = steeringFL.localRotation;

        if (steeringFR != null)
            initialSteeringFR = steeringFR.localRotation;
    }

    public void SetRunning(bool value)
    {
        isRunning = value;
    }

    public void ResetMovement()
    {
        isRunning = false;
        carBody.position = startPosition;
        car.position = startPosition;
        previousVisualPosition = car.position;
        steeringVelocity = 0f;
        targetIndex = 0;
        pauseRemaining = 0f;
        steeringInput = 0f;

        if (carVisual != null && carVisual != car)
        {
            carVisual.localPosition = initialVisualPosition;
            carVisual.localRotation = initialBodyRotation;
        }

        if (steeringFL != null)
            steeringFL.localRotation = initialSteeringFL;

        if (steeringFR != null)
            steeringFR.localRotation = initialSteeringFR;
    }

    private void FixedUpdate()
    {
        if (!isRunning)
            return;

        float dt = Time.fixedDeltaTime;
        Vector3 position = carBody.position;

        if (pauseRemaining > 0f)
        {
            pauseRemaining = Mathf.Max(0f, pauseRemaining - dt);
        }
        else
        {
            float targetX = startPosition.x + route[targetIndex] * amplitude;
            position.x = Mathf.MoveTowards(position.x, targetX, lateralSpeed * dt);

            if (Mathf.Approximately(position.x, targetX))
            {
                targetIndex = (targetIndex + 1) % route.Length;
                float min = Mathf.Max(0f, pauseRange.x);
                float max = Mathf.Max(min, pauseRange.y);
                pauseRemaining = Random.Range(min, max);
            }
        }
        carBody.MovePosition(position); 

        RotateWheels(worldSpeed * dt);
    }

    private void LateUpdate()
    {
        Vector3 currentPosition = car.position;
        float dt = Time.deltaTime;

        if (dt > 0f)
        {
            float lateralVelocity = isRunning ? (currentPosition.x - previousVisualPosition.x) / dt : 0f;
            UpdateSteering(lateralVelocity, dt);
        }

        previousVisualPosition = currentPosition;
    }

    private void RotateWheels(float distance)
    {
        if (wheels == null)
            return;

        float angle = distance / Mathf.Max(wheelRadius, 0.01f) * Mathf.Rad2Deg;

        foreach (Transform wheel in wheels)
        {
            if (wheel != null)
                wheel.Rotate(wheelAxis.normalized, angle, Space.Self);
        }
    }

    private void UpdateSteering(float lateralVelocity, float dt)
    {
        float targetInput = Mathf.Clamp(lateralVelocity / Mathf.Max(lateralSpeed, 0.01f), -1f, 1f);
        bool returningToCenter = Mathf.Abs(targetInput) < 0.01f;

        float smoothTime = returningToCenter ? returnSmoothTime : steeringSmoothTime;

        steeringInput = Mathf.SmoothDamp(steeringInput, targetInput, ref steeringVelocity, smoothTime, Mathf.Infinity, dt);
        float wheelAngle = steeringInput * maxSteeringAngle;

        if (carVisual != null)
        {
            carVisual.localRotation = Quaternion.AngleAxis(steeringInput * maxBodyAngle, Vector3.up) * initialBodyRotation;
        }

        if (steeringFL != null)
        {
            steeringFL.localRotation = Quaternion.AngleAxis(wheelAngle, Vector3.up) * initialSteeringFL;
        }

        if (steeringFR != null)
        {
            steeringFR.localRotation = Quaternion.AngleAxis(wheelAngle, Vector3.up) * initialSteeringFR;
        }
    }
}

