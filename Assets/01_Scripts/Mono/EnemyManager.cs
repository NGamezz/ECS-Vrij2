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

    private ObjectPool<GameObject> objectPool = new();

    private DifficultyGrade currentDifficultyGrade;

    [SerializeField] private Transform playerTransform;

    private void Start ()
    {
        currentDifficultyGrade = difficultyGrades[0];

        enemyTarget.target = playerTransform;

        GenerateObjects();
        SpawnEnemies();
    }

    private void RemoveEnemy ( Enemy sender )
    {
        activeEnemies.Remove(sender);
        sender.OnDeath -= OnEnemyDeath;
        sender.gameObject.SetActive(false);
        objectPool.PoolObject(sender.gameObject);
    }

    private void OnEnemyDeath ( Enemy sender )
    {
        Vector3 position = sender.Position;

        Task.Run(() =>
        {
            WorldManager.InvokeCellEvent(CellEventType.OnEntityDeath, position, position);
        });

        RemoveEnemy(sender);
    }

    private void GenerateObjects ()
    {
        for ( int i = 0; i < startingAmountOfPooledObjects; i++ )
        {
            var gameObject = Instantiate(currentDifficultyGrade.enemyPrefabs[0], transform);
            gameObject.SetActive(false);
            gameObject.AddComponent<Enemy>();
            objectPool.PoolObject(gameObject);
        }
    }

    private async void SpawnEnemies ()
    {
        for ( int i = 0; i < currentDifficultyGrade.enemyStats.enemiesPerBatch; i++ )
        {
            var position = playerTransform.position + (UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(spawnRange.x, spawnRange.y));
            position.y = transform.position.y;

            var gameObject = objectPool.GetPooledObject() ?? Instantiate(currentDifficultyGrade.enemyPrefabs[0], transform);
            gameObject.SetActive(true);

            if ( !gameObject.TryGetComponent(out NavMeshAgent agent) )
            {
                agent = gameObject.AddComponent<NavMeshAgent>();
            }
            agent.Warp(position);

            if ( !gameObject.TryGetComponent<Enemy>(out var enemy) )
            {
                enemy = gameObject.AddComponent<Enemy>();
            }

            enemy.OnStart(currentDifficultyGrade.enemyStats, enemyTarget);
            enemy.OnDeath += OnEnemyDeath;
            enemy.UpdateStats(currentDifficultyGrade.enemyStats);
            enemy.Dead = false;

            activeEnemies.Add(enemy);
        }

        await Awaitable.WaitForSecondsAsync(currentDifficultyGrade.enemyStats.spawnSpeed);

        SpawnEnemies();
    }

    private void Update ()
    {
        foreach ( var enemy in activeEnemies )
        {
            enemy.OnUpdate();
        }
    }

    private void FixedUpdate ()
    {
        foreach ( var enemy in activeEnemies )
        {
            enemy.OnFixedUpdate();
        }
    }
}

[Serializable]
public class DifficultyGrade
{
    public List<GameObject> enemyPrefabs;

    public EnemyStats enemyStats;
}