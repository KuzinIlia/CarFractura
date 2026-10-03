using UnityEngine;

public class BaseBullet : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float speed = 30f;
    [SerializeField, Min(0.1f)] private float lifetime = 3f;
    [SerializeField, Min(0.01f)] private float hitRadius = 0.25f;
    [SerializeField, Min(1)] private int damage = 25;

    [SerializeField] private LayerMask hitMask;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        float distance = speed * Time.deltaTime;

        if (distance <= 0f)
            return;

        Vector3 direction = transform.forward;

        if (Physics.SphereCast(transform.position, hitRadius, direction, out RaycastHit hit, distance, hitMask, QueryTriggerInteraction.Ignore))
        {
            transform.position += direction * hit.distance;
            HealthUnit health = hit.collider.GetComponentInParent<HealthUnit>();

            if (health != null)
                health.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        transform.position += transform.forward * distance;
    }
}
