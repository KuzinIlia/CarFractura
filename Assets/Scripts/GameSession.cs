using UnityEngine;
using Zenject;

public enum GameState
{
    WaitingToStart,
    Playing,
    Won,
    Lost
}

public class GameSession : MonoBehaviour
{
    [SerializeField, Min(1f)] private float levelLength = 300f;
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameHUD hud;

    public GameState State { get; private set; }
    public float LevelLength => levelLength;

    private CarScript car;
    private TurretScript turret;
    private LoopingTheRoad road;
    private HealthUnit carHealth;
    private EnemySpawner spawner;

    [Inject]
    public void Construct(CarScript car, TurretScript turret, LoopingTheRoad road, [Inject(Id = "CarHealth")] HealthUnit carHealth, EnemySpawner spawner)
    {
        this.car = car;
        this.turret = turret;
        this.road = road;
        this.carHealth = carHealth;
        this.spawner = spawner;
    }

    private void Start()
    {
        ResetRun();
    }

    private void LateUpdate()
    {
        if (State == GameState.Playing)
        {
            if (carHealth.IsDead())
            {
                FinishRun(GameState.Lost);
            }
            else if (road.DistanceTravelled >= levelLength)
            {
                FinishRun(GameState.Won);
            }
            return;
        }

        if (!Input.GetMouseButtonDown(0))
            return;

        if (State == GameState.WaitingToStart)
        {
            StartRun();
        }
        else
        {
            ResetRun();
        }
    }

    private void StartRun()
    {
        State = GameState.Playing;
        SetRunning(true);
        RefreshPanels();
    }

    private void FinishRun(GameState result)
    {
        State = result;
        SetRunning(false);
        ClearBullets();
        RefreshPanels();
    }

    private void ResetRun()
    {
        SetRunning(false);

        spawner.ClearEnemies();
        ClearBullets();

        car.ResetMovement();
        turret.ResetTurret();
        road.ResetRoad();
        carHealth.ResetHealth();

        State = GameState.WaitingToStart;

        if (hud != null)
            hud.ResetKillCount();

        RefreshPanels();
    }

    private void SetRunning(bool value)
    {
        road.SetRunning(value);
        car.SetRunning(value);
        turret.SetRunning(value);
    }

    private void RefreshPanels()
    {
        if (waitingPanel != null)
            waitingPanel.SetActive(State == GameState.WaitingToStart);

        if (winPanel != null)
            winPanel.SetActive(State == GameState.Won);

        if (losePanel != null)
            losePanel.SetActive(State == GameState.Lost);
    }

    private void ClearBullets()
    {
        BaseBullet[] bullets = Object.FindObjectsByType<BaseBullet>(FindObjectsSortMode.None);

        foreach (BaseBullet bullet in bullets)
        {
            bullet.gameObject.SetActive(false);
            Destroy(bullet.gameObject);
        }
    }
}