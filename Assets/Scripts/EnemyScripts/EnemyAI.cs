using System.Collections.Generic;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(HealthUnit), typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private Transform visual;
    [SerializeField] private Animator animator;

    [SerializeField, Min(0.1f)] private float detectionRadius = 12f;
    [SerializeField, Min(0.1f)] private float moveSpeed = 8f;
    [SerializeField, Min(1)] private int damage = 10;
    [SerializeField, Min(1f)] private float turnSpeed = 360f;
    [SerializeField, Min(1f)] private float despawnBehind = 15f;
    [SerializeField, Min(0.1f)] private float deathDuration = 1.7f;
    [SerializeField] private GameObject healthCanvas;

    private readonly HashSet<Collider> carContacts = new();
    private static readonly int StateParameter = Animator.StringToHash("State");

    private Rigidbody carBody;
    private Rigidbody body;
    private HealthUnit ownHealth;
    private HealthUnit carHealth;
    private LoopingTheRoad road;
    private GameHUD hud;

    private bool attackMode;
    private bool isDying;
    private float deathRemaining;
    private Vector3 deathVisualPosition;

    [Inject]
    public void Construct(LoopingTheRoad road, [Inject(Id = "CarHealth")] HealthUnit carHealth, GameHUD hud)
    {
        this.road = road;
        this.carHealth = carHealth;
        this.hud = hud;
        carBody = carHealth.GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        ownHealth = GetComponent<HealthUnit>();
    }

    private void FixedUpdate()
    {
        if (isDying || ownHealth.IsDead())
        {
            BeginDeath();
            body.linearVelocity = road.IsRunning ? Vector3.back * road.Speed : Vector3.zero;
            return;
        }

        if (!road.IsRunning || carHealth.IsDead())
        {
            body.linearVelocity = Vector3.zero;
            return;
        }

        float dt = Time.fixedDeltaTime;
        Vector3 velocity = Vector3.back * road.Speed;
        carContacts.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);
        Vector3 offset = carHealth.transform.position - body.position;
        offset.y = 0f;

        if (offset.sqrMagnitude <= detectionRadius * detectionRadius)
            attackMode = true;

        if (attackMode)
        {
            Vector3 direction = offset.normalized;
            velocity += direction * moveSpeed;

            if (visual != null && visual != transform && direction.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(direction);
                visual.rotation = Quaternion.RotateTowards(visual.rotation, target, turnSpeed * dt);
            }
        }

        if (carContacts.Count > 0)
        {
            Vector3 contactVelocity = carBody != null ? carBody.GetPointVelocity(body.position) : Vector3.zero;
            contactVelocity.y = 0f;
            body.linearVelocity = contactVelocity;
        }
        else
        {
            body.linearVelocity = velocity;
        }

        if (body.position.z < carHealth.transform.position.z - despawnBehind)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        RegisterContact(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        RegisterContact(collision);
    }

    private void RegisterContact(Collision collision)
    {   
        if (isDying || ownHealth.IsDead())
            return;

        HealthUnit target = collision.collider.GetComponentInParent<HealthUnit>();

        if (target != carHealth)
            return;

        carContacts.Add(collision.collider);
        attackMode = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        carContacts.Remove(collision.collider);
    }

    private void OnDisable()
    {
        carContacts.Clear();
        attackMode = false;

        if (body != null)
            body.linearVelocity = Vector3.zero;
    }

    private void Update()
    {
        if (ownHealth == null)
            return;

        if (isDying || ownHealth.IsDead())
        {
            BeginDeath();

            if (animator != null)
                animator.speed = 1f;

            deathRemaining -= Time.deltaTime;
            float duration = Mathf.Min(1.2f, deathDuration);

            if (visual != null && duration > 0f)
            {
                float progress = Mathf.Clamp01(1f - (deathRemaining-0.2f) / duration);
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
                visual.localPosition = deathVisualPosition + Vector3.down * 0.525f * smoothProgress;
            }


            if (deathRemaining <= 0f)
                Destroy(gameObject);

            return;
        }

        if (animator == null)
            return;

        bool playing = road.IsRunning && !carHealth.IsDead();
        animator.speed = playing ? 1f : 0f;

        if (!playing)
            return;

        int state;

        if (carContacts.Count > 0)
            state = 2; // Attack
        else if (attackMode)
            state = 1; // Run
        else
            state = 0; // Idle

        animator.SetInteger(StateParameter, state);
    }

    public void ApplyAnimationHit()
    {
        if (isDying || !road.IsRunning || ownHealth.IsDead() || carHealth.IsDead())
            return;

        carContacts.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);

        if (carContacts.Count == 0)
            return;

        carHealth.TakeDamage(damage);
        BeginDeath();
    }

    private void BeginDeath()
    {
        if (isDying)
            return;

        if (visual != null)
            deathVisualPosition = visual.localPosition;

        bool killedByPlayer = ownHealth.IsDead();
        if (killedByPlayer && hud != null)
            hud.UpdateEnemyKillerCount();

        isDying = true;
        ownHealth.TakeDamage(ownHealth.CurrentHealth);
        deathRemaining = deathDuration;

        attackMode = false;
        carContacts.Clear();

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        foreach (Collider enemyCollider in GetComponentsInChildren<Collider>())
            enemyCollider.enabled = false;

        if (healthCanvas != null)
            healthCanvas.SetActive(false);

        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetInteger(StateParameter, 3);
        }
    }

    public class Factory : PlaceholderFactory<EnemyAI> { }
}
