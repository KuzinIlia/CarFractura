using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthHUD : MonoBehaviour
{
    [SerializeField] private Image fill;

    private HealthUnit health;
    private Camera viewCamera;
    private int previousHealth = -1;

    private void Awake()
    {
        health = GetComponentInParent<HealthUnit>();
        viewCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (health == null)
            return;

        if (viewCamera != null)
            transform.rotation = viewCamera.transform.rotation;

        if (previousHealth == health.CurrentHealth)
            return;

        previousHealth = health.CurrentHealth;
        fill.fillAmount = health.HealthRatio();
    }
}