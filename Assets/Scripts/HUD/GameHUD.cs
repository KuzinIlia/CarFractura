using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private GameSession session;
    [SerializeField] private Image carHealthFill;
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text enemyKillerCount;
    [SerializeField] private Vector2 healthBarSize = new Vector2(150f, 12f);
    [SerializeField] private Vector2 healthBarOffset = new Vector2(0f, -45f);

    private HealthUnit carHealth;
    private LoopingTheRoad road;
    private Camera viewCamera;
    private Canvas canvas;
    private RectTransform healthBar;
    private RectTransform healthBarParent;
    private int previousDistance = -1;
    private int killCount = 0;

    [Inject]
    public void Construct([Inject(Id = "CarHealth")] HealthUnit carHealth, LoopingTheRoad road)
    {
        this.carHealth = carHealth;
        this.road = road;
    }

    private void Start()
    {
        viewCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();

        if (carHealthFill == null) 
            return;
        
        healthBar = carHealthFill.transform.parent as RectTransform;
        healthBarParent = healthBar != null ? healthBar.parent as RectTransform : null;

        if (healthBar == null || healthBarParent == null) 
            return;

        healthBar.anchorMin = healthBar.anchorMax = new Vector2(0.5f, 0.5f);
        healthBar.pivot = new Vector2(0.5f, 0.5f);
        healthBar.sizeDelta = healthBarSize;
        healthBar.localScale = Vector3.one;
    }

    private void LateUpdate()
    {
        if (carHealth == null || road == null || session == null) 
            return;

        if (carHealthFill != null) 
            carHealthFill.fillAmount = carHealth.HealthRatio();

        UpdateHealthPosition();
        float length = Mathf.Max(1f, session.LevelLength);
        float distance = Mathf.Clamp(road.DistanceTravelled, 0f, length);

        if (progressFill != null) 
            progressFill.fillAmount = distance / length;

        int meters = Mathf.FloorToInt(distance);

        if (distanceText != null && previousDistance != meters)
        {
            previousDistance = meters;
            distanceText.SetText("{0} / {1} m", meters, Mathf.CeilToInt(length));
        }
    }

    private void UpdateHealthPosition()
    {
        if (viewCamera == null || canvas == null || healthBarParent == null) 
            return;

        Vector3 screenPoint = viewCamera.WorldToScreenPoint(carHealth.transform.position);
        bool visible = screenPoint.z > 0f;
        healthBar.gameObject.SetActive(visible);

        if (!visible) 
            return;

        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(healthBarParent, screenPoint, uiCamera, out Vector2 localPoint))
        {
            healthBar.localPosition = new Vector3(localPoint.x + healthBarOffset.x, localPoint.y + healthBarOffset.y, 0f);
        }
    }

    public void UpdateEnemyKillerCount()
    {
        killCount++;

        if (enemyKillerCount != null)
        {
            enemyKillerCount.SetText("{0}", killCount);
        }
    }
    public void ResetKillCount()
    {
        killCount = 0;

        if (enemyKillerCount != null)
            enemyKillerCount.SetText("{0}", killCount);
    }

}
