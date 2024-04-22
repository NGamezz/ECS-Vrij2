using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private int amountOfEnemiesPerBatch = 2;

    [SerializeField] private List<DifficultyGrade> difficultyGrades = new();

    [SerializeField] private List<Enemy> enemies = new();

    [SerializeField] private float2 spawnRange = new(5.0f, 15.0f);

    private ObjectPool<GameObject> objectPool;

    private Coroutine currentRoutine;

    private DifficultyGrade currentDifficultyGrade;

    [SerializeField] private Transform playerTransform;

    private void Start ()
    {
        currentDifficultyGrade = difficultyGrades[0];

        objectPool = new();

        currentRoutine = StartCoroutine(SpawnEnemies());
    }

    //Implement Object Pooling Later.
    private void RemoveEnemy ( object sender, EventArgs eventArgs )
    {
        Enemy enemy = (Enemy)sender;

        Debug.Log("Death");

        enemy.OnDeath -= RemoveEnemy;
        enemies.Remove(enemy);
        Destroy(enemy.gameObject);
    }

    private IEnumerator SpawnEnemies()
    {
        Debug.Log("Spawning Enemies.");

        for (int i = 0; i < amountOfEnemiesPerBatch; i++)
        {
            var position = playerTransform.position + (UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(spawnRange.x, spawnRange.y));

            position.y = transform.position.y;

            var gameObject = Instantiate(currentDifficultyGrade.enemyPrefabs[0], transform);

            if (!gameObject.TryGetComponent(out NavMeshAgent agent))
            {
                agent = gameObject.AddComponent<NavMeshAgent>();
            }
            agent.Warp(position);

            var enemy = gameObject.AddComponent<Enemy>();

            enemy.OnStart(playerTransform, currentDifficultyGrade.enemyStats);
            enemy.OnDeath += RemoveEnemy;

            Debug.Log("Spawned Enemy.");

            enemies.Add(enemy);
        }
        yield return new WaitForSeconds(currentDifficultyGrade.enemyStats.spawnSpeed);

        currentRoutine = StartCoroutine(SpawnEnemies());
    }

    private void Update ()
    {
        foreach ( var enemy in enemies )
        {
            enemy.OnUpdate();
        }
    }

    private void FixedUpdate ()
    {
        foreach ( var enemy in enemies )
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