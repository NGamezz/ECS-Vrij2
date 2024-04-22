using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<DifficultyGrade> difficultyGrades = new();

    [SerializeField] private List<Enemy> activeEnemies = new();

    [Tooltip("How many can be seen on the screen at once, without having to create more of them.")]
    [SerializeField] private byte startingAmountOfPooledObjects = 30;

    [SerializeField] private float2 spawnRange = new(5.0f, 15.0f);

    private ObjectPool<GameObject> objectPool = new();

    private Coroutine currentRoutine;

    private DifficultyGrade currentDifficultyGrade;

    [SerializeField] private Transform playerTransform;

    private void Start()
    {
        currentDifficultyGrade = difficultyGrades[0];

        GenerateObjects();
        currentRoutine = StartCoroutine(SpawnEnemies());
    }

    //Implement Object Pooling Later.
    private void RemoveEnemy(object sender, EventArgs eventArgs)
    {
        Enemy enemy = (Enemy)sender;

        Debug.Log("Death");

        enemy.OnDeath -= RemoveEnemy;
        activeEnemies.Remove(enemy);
        enemy.gameObject.SetActive(false);
        objectPool.PoolObject(enemy.gameObject);
    }

    private void GenerateObjects()
    {
        for (int i = 0; i < startingAmountOfPooledObjects; i++)
        {
            var gameObject = Instantiate(currentDifficultyGrade.enemyPrefabs[0], transform);
            gameObject.SetActive(false);
            gameObject.AddComponent<Enemy>();
            objectPool.PoolObject(gameObject);
        }
    }

    private IEnumerator SpawnEnemies()
    {
        for (int i = 0; i < currentDifficultyGrade.enemyStats.enemiesPerBatch; i++)
        {
            var position = playerTransform.position + (UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(spawnRange.x, spawnRange.y));
            position.y = transform.position.y;

            var gameObject = objectPool.GetPooledObject() ?? Instantiate(currentDifficultyGrade.enemyPrefabs[0], transform);
            gameObject.SetActive(true);

            if (!gameObject.TryGetComponent(out NavMeshAgent agent))
            {
                agent = gameObject.AddComponent<NavMeshAgent>();
            }
            agent.Warp(position);

            if (!gameObject.TryGetComponent<Enemy>(out var enemy))
            {
                enemy = gameObject.AddComponent<Enemy>();
            }

            enemy.OnStart(playerTransform, currentDifficultyGrade.enemyStats);
            enemy.OnDeath += RemoveEnemy;
            enemy.UpdateStats(currentDifficultyGrade.enemyStats);

            Debug.Log("Spawned Enemy.");

            activeEnemies.Add(enemy);
        }
        yield return new WaitForSeconds(currentDifficultyGrade.enemyStats.spawnSpeed);

        currentRoutine = StartCoroutine(SpawnEnemies());
    }

    private void Update()
    {
        foreach (var enemy in activeEnemies)
        {
            enemy.OnUpdate();
        }
    }

    private void FixedUpdate()
    {
        foreach (var enemy in activeEnemies)
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