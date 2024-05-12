using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<DifficultyGrade> difficultyGrades = new();

    [SerializeField] private MoveTarget enemyTarget;

    [SerializeField] private List<Enemy> activeEnemies = new();

    [Tooltip("How many can be seen on the screen at once, without having to create more of them.")]
    [SerializeField] private byte startingAmountOfPooledObjects = 30;

    [SerializeField] private float2 spawnRange = new(5.0f, 15.0f);

    [SerializeField] private float maxDistanceToPlayer;

    private readonly ObjectPool<Enemy> objectPool = new();

    private DifficultyGrade currentDifficultyGrade;

    [SerializeField] private Transform playerTransform;

    private bool spawnEnemies = false;

    public bool SpawnEnemies
    {
        get => spawnEnemies;
        set
        {
            spawnEnemies = value;
            if ( !value )
                StopRoutine();
        }
    }

    private Vector3 ownPosition;

    public void StopRoutine ()
    {
        StopAllCoroutines();
    }

    private void Start ()
    {
        currentDifficultyGrade = difficultyGrades[0];

        enemyTarget.target = playerTransform;
        ownPosition = transform.position;

        GenerateObjects();

        SpawnEnemies = true;
        StartCoroutine(SpawnEnemiesIE());
        StartCoroutine(CleanUpLostEnemies());
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
            var gameObject = Instantiate(currentDifficultyGrade.enemyPrefabs[UnityEngine.Random.Range(0, currentDifficultyGrade.enemyPrefabs.Count)], transform);
            gameObject.SetActive(false);
            var enemy = gameObject.GetOrAddComponent<Enemy>();

            enemy.OnStart(currentDifficultyGrade.enemyStats, enemyTarget, gameObject.transform.position);
            enemy.OnDeath += OnEnemyDeath;
            enemy.OnDisabled += RemoveEnemy;

            objectPool.PoolObject(enemy);
        }
    }

    private IEnumerator SpawnEnemiesIE ()
    {
        while ( spawnEnemies )
        {
            var currentEnemyStats = currentDifficultyGrade.enemyStats;
            Vector3 playerPos = playerTransform.position;

            for ( int i = 0; i < currentEnemyStats.enemiesPerBatch; i++ )
            {
                var position = playerPos + (UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(spawnRange.x, spawnRange.y));
                position.y = ownPosition.y;

                var enemy = objectPool.GetPooledObject();
                var gameObject = enemy.GameObject;

                if ( enemy == null )
                {
                    gameObject = Instantiate(currentDifficultyGrade.enemyPrefabs[UnityEngine.Random.Range(0, currentDifficultyGrade.enemyPrefabs.Count)], transform);
                    enemy = gameObject.AddComponent<Enemy>();
                    enemy.OnDisabled += RemoveEnemy;
                    enemy.OnDeath += OnEnemyDeath;
                    enemy.OnStart(currentDifficultyGrade.enemyStats, enemyTarget, position);
                }
                else
                {
                    enemy.OnReuse(currentDifficultyGrade.enemyStats, position);
                    gameObject.SetActive(true);
                }

                activeEnemies.Add(enemy);
            }

            yield return Utility.Yielders.Get(currentEnemyStats.spawnSpeed);
        }
    }

    private void OnDisable ()
    {
        foreach ( var enemy in activeEnemies )
        {
            enemy.OnDeath -= OnEnemyDeath;
        }
        objectPool.ClearPool();
        activeEnemies.Clear();

        StopAllCoroutines();
    }

    private void Update ()
    {
        if ( activeEnemies.Count < 1 )
            return;

        var target = enemyTarget.target;
        var targetPos = target.position;

        foreach ( var enemy in activeEnemies )
        {
            if ( enemy == null )
                continue;

            enemy.OnUpdate();
            enemy.CheckAttackRange(target, targetPos);
        }
    }

    private IEnumerator CleanUpLostEnemies ()
    {
        while ( spawnEnemies )
        {
            Vector3 playerPos = playerTransform.position;

            for ( int i = 0; i < activeEnemies.Count; i++ )
            {
                var enemy = activeEnemies[i];
                if ( enemy == null )
                    continue;

                var ownPos = enemy.Transform.position;
                if ( Vector3.Distance(ownPos, playerPos) > maxDistanceToPlayer )
                {
                    RemoveEnemy(enemy);
                }
            }

            yield return Utility.Yielders.FixedUpdate;
        }
    }

    private void FixedUpdate ()
    {
        if ( activeEnemies.Count < 1 )
            return;

        foreach ( var enemy in activeEnemies )
        {
            if ( enemy == null )
                continue;

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