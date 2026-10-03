using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemyAI enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyAI>();
    }

    public void Hit()
    {
        if (enemy != null)
            enemy.ApplyAnimationHit();
    }
}
