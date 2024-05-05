using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<DifficultyGrade> difficultyGrades = new();

    [SerializeField] private MoveTarget enemyTarget;

    [SerializeField] private List<Enemy> activeEnemies = new();

    [Tooltip("How many can be seen on the screen at once, without having to create more of them.")]
    [SerializeField] private byte startingAmountOfPooledObjects = 30;

    [SerializeField] private float2 spawnRange = new(5.0f, 15.0f);

    [SerializeField] private float maxDistanceToPlayer;

    private ObjectPool<Enemy> objectPool = new();

    private DifficultyGrade currentDifficultyGrade;

    [SerializeField] private Transform playerTransform;

    private Vector3 ownPosition;

    private void Start ()
    {
        currentDifficultyGrade = difficultyGrades[0];

        enemyTarget.target = playerTransform;

        ownPosition = transform.position;

        GenerateObjects();
        SpawnEnemies();
    }

    private void RemoveEnemy ( Enemy sender )
    {
        activeEnemies.Remove(sender);
        sender.gameObject.SetActive(false);
        objectPool.PoolObject(sender);
    }

    private void OnEnemyDeath ( Enemy sender )
    {
        Vector3 position = sender.Transform.position;

        Task.Run(() =>
        {
            if ( !WorldManager.InvokeCellEvent(CellEventType.OnEntityDeath, position, position) )
            {
                EventManagerGeneric<int>.InvokeEvent(EventType.UponHarvestSoul, 1);
            }
        }).ConfigureAwait(false);

        RemoveEnemy(sender);
    }

    private void GenerateObjects ()
    {
        for ( int i = 0; i < startingAmountOfPooledObjects; i++ )
        {
            var gameObject = Instantiate(currentDifficultyGrade.enemyPrefabs[0], transform);
            gameObject.SetActive(false);
            var enemy = gameObject.AddComponent<Enemy>();
            enemy.OnStart(currentDifficultyGrade.enemyStats, enemyTarget, gameObject.transform.position);
            enemy.OnDeath += OnEnemyDeath;
            objectPool.PoolObject(enemy);
        }
    }

    //Still to be improved.
    private async void SpawnEnemies ()
    {
        Vector3 playerPos = playerTransform.position;

        for ( int i = 0; i < currentDifficultyGrade.enemyStats.enemiesPerBatch; i++ )
        {
            var position = playerPos + (UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(spawnRange.x, spawnRange.y));
            position.y = ownPosition.y;

            bool instantiated = false;

            var enemy = objectPool.GetPooledObject();

            var gameObject = enemy.GameObject;

            if ( gameObject == null )
            {
                gameObject = Instantiate(currentDifficultyGrade.enemyPrefabs[0], transform);
                instantiated = true;
            }

            gameObject.SetActive(true);

            if ( enemy == null )
            {
                enemy = gameObject.AddComponent<Enemy>();
            }
            enemy.Dead = false;

            if ( instantiated )
            {
                enemy.OnDeath += OnEnemyDeath;
                enemy.OnStart(currentDifficultyGrade.enemyStats, enemyTarget, position);
            }
            else
            {
                enemy.OnReuse(currentDifficultyGrade.enemyStats, position);
            }

            activeEnemies.Add(enemy);
        }

        await Awaitable.WaitForSecondsAsync(currentDifficultyGrade.enemyStats.spawnSpeed);

        SpawnEnemies();
    }

    //Need to add proper disposal.
    private void OnDisable ()
    {
        foreach ( var enemy in activeEnemies )
        {
            enemy.OnDeath -= OnEnemyDeath;
        }
    }

    private void Update ()
    {
        if ( activeEnemies.Count < 1 )
            return;

        var target = enemyTarget.target;
        var targetPos = target.position;

        foreach ( var enemy in activeEnemies )
        {
            enemy.OnUpdate();
            enemy.CheckAttackRange(target, targetPos);
        }
    }

    private void CleanUpLostEnemies ()
    {
        List<Enemy> enemies = activeEnemies;
        Vector3 playerPos = playerTransform.position;

        for ( int i = 0; i < enemies.Count; i++ )
        {
            if ( Vector3.Distance(enemies[i].Transform.position, playerPos) > maxDistanceToPlayer )
            {
                var enemy = enemies[i];
                RemoveEnemy(enemy);
            }
        }
    }

    private void FixedUpdate ()
    {
        if ( activeEnemies.Count < 1 )
            return;

        foreach ( var enemy in activeEnemies )
        {
            enemy.OnFixedUpdate();
        }

        CleanUpLostEnemies();
    }
}

[Serializable]
public class DifficultyGrade
{
    public List<GameObject> enemyPrefabs;

    public EnemyStats enemyStats;
}