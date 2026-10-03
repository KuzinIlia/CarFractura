using UnityEngine;

public class HealthUnit : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 100;

    public int CurrentHealth{ get; private set; }
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        ResetHealth();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead() || damage <= 0)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
    }

    public bool IsDead()
    {
        return CurrentHealth <= 0;
    }

    public float HealthRatio()
    {
        return Mathf.Clamp01((float)CurrentHealth / Mathf.Max(1, maxHealth));
    }
}
