using UnityEngine;

public class LoopingTheRoad : MonoBehaviour
{
    [SerializeField] private Transform[] segments;
    [SerializeField, Min(0.1f)] private float segmentLength = 30f;
    [SerializeField, Min(0f)] private float speed = 5f;
    [SerializeField] private bool isRunning = true;

    public float DistanceTravelled { get; private set; }
    public float Speed => speed;
    public bool IsRunning => isRunning;

    private Vector3[] initialPositions;

    private void Awake()
    {
        if (segments == null || segments.Length < 2)
            return;

        foreach (Transform segment in segments)
        {
            if (segment == null)
                return;
        }

        initialPositions = new Vector3[segments.Length];
        Vector3 firstPosition = segments[0].position;
        Renderer segmentRenderer = segments[0].GetComponentInChildren<Renderer>();

        if (segmentRenderer != null)
            segmentLength = Mathf.Max(0.1f, segmentRenderer.bounds.size.z);

        for (int i = 0; i < segments.Length; i++)
        {
            Vector3 position = firstPosition + Vector3.forward * segmentLength * i;
            segments[i].position = position;
            initialPositions[i] = position;
        }
    }

    private void Update()
    {
        if (!isRunning)
            return;

        float distance = speed * Time.deltaTime;
        DistanceTravelled += distance;
        float loopLength = segmentLength * segments.Length;

        foreach (Transform segment in segments)
        {
            Vector3 position = segment.position;
            position.z -= distance;

            while (position.z + segmentLength < 10f)
            {
                position.z += loopLength;
            }
            segment.position = position;
        }
    }

    public void SetRunning(bool value)
    {
        isRunning = value;
    }

    public void SetSpeed(float value)
    {
        speed = Mathf.Max(0f, value);
    }

    public void ResetRoad()
    {
        isRunning = false;
        DistanceTravelled = 0f;

        if (initialPositions == null)
            return;

        for (int i = 0; i < segments.Length; i++)
            segments[i].position = initialPositions[i];
    }
}
