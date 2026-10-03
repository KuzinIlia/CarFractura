using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{ 
    [SerializeField] private CarScript carScript;
    [SerializeField] private TurretScript turretScript;
    [SerializeField] private LoopingTheRoad road;
    [SerializeField] private HealthUnit carHealth;
    [SerializeField] private EnemyAI enemyPrefab;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private GameHUD gameHud;

    public override void InstallBindings()
    {
        Container.Bind<CarScript>().FromInstance(carScript).AsSingle();
        Container.Bind<TurretScript>().FromInstance(turretScript).AsSingle();
        Container.Bind<LoopingTheRoad>().FromInstance(road).AsSingle();
        Container.Bind<HealthUnit>().WithId("CarHealth").FromInstance(carHealth).AsSingle();
        Container.BindFactory<EnemyAI, EnemyAI.Factory>().FromComponentInNewPrefab(enemyPrefab);
        Container.Bind<EnemySpawner>().FromInstance(enemySpawner).AsSingle();
        Container.Bind<GameHUD>().FromInstance(gameHud).AsSingle();
    }
}
