using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [SerializeField] private MoveTarget enemyTarget;

    [SerializeField] private List<Enemy> activeEnemies = new();

    [Tooltip("How many can be seen on the screen at once, without having to create more of them.")]
    [SerializeField] private byte startingAmountOfPooledObjects = 30;

    [SerializeField] private float2 spawnRange = new(5.0f, 15.0f);

    [SerializeField] private float maxDistanceToPlayer;

    private readonly ObjectPool<Enemy> objectPool = new();


    [SerializeField] private Transform playerTransform;

    [Header("Difficulty Scaling.")]
    [SerializeField] private List<DifficultyGrade> difficultyGrades = new();
    [SerializeField] private float currentDifficultyIndex = 0;
    [SerializeField] private float difficultyIncreasePerSecond = 0.2f;
    [SerializeField] private int gradeIndex = 0;

    private DifficultyGrade currentDifficultyGrade;
    private int requiredIndexForDifficultyAdvancement = 5;

    private bool spawnEnemies = false;
    public bool SpawnEnemies
    {
        get => spawnEnemies;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    private void Awake ()
    {
        if ( Instance != null )
            Destroy(Instance);

        Instance = this;
    }

    private void Start ()
    {
        foreach ( var difficulty in difficultyGrades )
        {
            difficulty.InitializeDictionary();
        }

        currentDifficultyGrade = difficultyGrades[gradeIndex];

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

    private CharacterData CreateEnemyDataObject ( Enemy enemy )
    {
        CharacterData data = (CharacterData)ScriptableObject.CreateInstance(nameof(CharacterData));

        Debug.Log(enemy.EnemyType);
        Debug.Log(enemy.name);

        var prefab = currentDifficultyGrade.RequestPrefab(enemy.EnemyType);
        var defaultStats = prefab.defaultStats;

        data.Speed = defaultStats.moveSpeed;
        data.SpeedMultiplier = 1;
        data.Souls = 0;
        data.Stamina = 0;
        data.DamageMultiplier = defaultStats.damage;
        data.MaxHealth = defaultStats.maxHealth;

        if ( enemy.EnemyType == EnemyType.LieEnemy )
            data.decoyPrefab = currentDifficultyGrade.enemyPrefabs[UnityEngine.Random.Range(0, currentDifficultyGrade.enemyPrefabs.Count)].meshPrefab;

        data.CharacterTransform = enemy.Transform;
        data.Speed = currentDifficultyGrade.enemyStats.moveSpeed;

        data.MoveTarget = enemyTarget;
        data.Reset();

        return data;
    }

    private void OnEnemyDeath ( Enemy sender )
    {
        Vector3 position = sender.Transform.position;

        Task.Run(() =>
        {
            var succes = WorldManager.InvokeCellEvent(CellEventType.OnEntityDeath, position, position);

            if ( !succes )
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
            var enemy = CreateEnemy(currentDifficultyGrade, RemoveEnemy, OnEnemyDeath, enemyTarget, ownPosition);

            enemy.GameObject.SetActive(false);

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

                var succes = objectPool.GetPooledObject(out var enemy);
                var gameObject = enemy.GameObject;

                if ( !succes )
                {
                    enemy = CreateEnemy(currentDifficultyGrade, RemoveEnemy, OnEnemyDeath, enemyTarget, position);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Enemy CreateEnemy ( DifficultyGrade currentDifficultyGrade, Action<Enemy> onDisable, Action<Enemy> onDeath, MoveTarget enemyTarget, Vector3 position )
    {
        var gameObject = Instantiate(currentDifficultyGrade.enemyPrefabs[UnityEngine.Random.Range(0, currentDifficultyGrade.enemyPrefabs.Count)].meshPrefab, transform);
        var enemy = gameObject.GetOrAddComponent<Enemy>();
        enemy.OnDisabled += onDisable;
        enemy.OnDeath += onDeath;
        enemy.OnStart(currentDifficultyGrade.enemyStats, enemyTarget, position, () => CreateEnemyDataObject(enemy));
        return enemy;
    }

    public Enemy CreateEnemy ( Vector3 position )
    {
        return CreateEnemy(currentDifficultyGrade, null, null, enemyTarget, position);
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

    private void UpdateDifficultyIndex ()
    {
        currentDifficultyIndex += difficultyIncreasePerSecond * Time.deltaTime;

        if ( currentDifficultyIndex >= requiredIndexForDifficultyAdvancement )
        {
            currentDifficultyIndex = 0;
            if ( gradeIndex >= difficultyGrades.Count )
                return;

            currentDifficultyGrade = difficultyGrades[++gradeIndex];
        }
    }

    private void Update ()
    {
        if ( activeEnemies.Count < 1 )
            return;

        var target = enemyTarget.target;
        var targetPos = target == null ? playerTransform.position : target.position;

        for ( int i = 0; i < activeEnemies.Count; i++ )
        {
            var enemy = activeEnemies[i];
            if ( enemy == null )
                continue;

            enemy.CheckAttackRange(enemyTarget, targetPos);
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

                if ( Vector3.Distance(enemy.Transform.position, playerPos) > maxDistanceToPlayer )
                {
                    RemoveEnemy(enemy);
                }

                if ( i != 0 && i % 10 == 0 )
                    yield return null;
            }

            yield return Utility.Yielders.FixedUpdate;
        }
    }

    private void FixedUpdate ()
    {
        if ( activeEnemies.Count < 1 )
            return;

        for ( int i = 0; i < activeEnemies.Count; i++ )
        {
            var enemy = activeEnemies[i];
            if ( enemy == null )
                continue;

            enemy.OnFixedUpdate();
        }
    }
}

[Serializable]
public class DifficultyGrade
{
    public List<EnemyPrefab> enemyPrefabs;

    public Dictionary<EnemyType, EnemyPrefab> EnemyPrefabs;

    public EnemyStats enemyStats;

    public EnemyPrefab RequestPrefab ( EnemyType type )
    {
        return EnemyPrefabs[type];
    }

    public void InitializeDictionary ()
    {
        EnemyPrefabs = new();

        foreach ( var prefab in enemyPrefabs )
        {
            if ( !EnemyPrefabs.ContainsKey(prefab.type) )
            {
                EnemyPrefabs.Add(prefab.type, prefab);
            }
        }
    }
}

[Serializable]
public class EnemyPrefab
{
    public GameObject meshPrefab;
    public EnemyStats defaultStats;
    public EnemyType type;
}