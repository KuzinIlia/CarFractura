using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float spawnInterval = 1.5f;
    [SerializeField, Min(1)] private int maxAlive = 30;
    [SerializeField, Min(0f)] private float halfWidth = 4f;
    [SerializeField, Min(0f)] private float depth = 10f;

    private readonly List<EnemyAI> enemies = new();

    private EnemyAI.Factory factory;
    private LoopingTheRoad road;
    private HealthUnit carHealth;
    private float timer;

    [Inject]
    public void Construct(EnemyAI.Factory factory, LoopingTheRoad road, [Inject(Id = "CarHealth")] HealthUnit carHealth)
    {
        this.factory = factory;
        this.road = road;
        this.carHealth = carHealth;
    }

    private void Update()
    {
        enemies.RemoveAll(enemy => enemy == null);

        if (!road.IsRunning || carHealth.IsDead())
            return;

        timer -= Time.deltaTime;

        if (timer > 0f || enemies.Count >= maxAlive)
            return;

        timer = spawnInterval;
        Vector3 position = transform.position;
        position.x += Random.Range(-halfWidth, halfWidth);
        position.z += Random.Range(0f, depth);

        EnemyAI enemy = factory.Create();
        Rigidbody enemyBody = enemy.GetComponent<Rigidbody>();
        Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        enemy.transform.SetPositionAndRotation(position, rotation);

        enemyBody.position = position;
        enemyBody.rotation = rotation;
        enemyBody.linearVelocity = Vector3.zero;
        enemyBody.angularVelocity = Vector3.zero;

        enemies.Add(enemy);
    }

    public void ClearEnemies()
    {
        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null)
                continue;

            enemy.gameObject.SetActive(false);
            Destroy(enemy.gameObject);
        }

        enemies.Clear();
        timer = 0f;
    }
}
