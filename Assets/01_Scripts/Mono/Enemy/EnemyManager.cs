using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [SerializeField] private MoveTarget enemyTarget;

    [SerializeField] private List<Enemy> activeEnemies = new();

    [Tooltip("How many can be seen on the screen at once, without having to create more of them.")]
    [SerializeField] private byte startingAmountOfPooledObjects = 30;

    [SerializeField] private float2 spawnRange = new(5.0f, 15.0f);

    [SerializeField] private float maxDistanceToPlayer;

    [SerializeField] private UnityEvent onEnemyDeath;

    [SerializeField] private BlackBoardObject blackBoardObject;

    private readonly ObjectPool<Enemy> objectPool = new();

    private Transform currentlySelectedTarget = null;

    [Header("Difficulty Scaling.")]
    [SerializeField] private List<DifficultyGrade> difficultyGrades = new();
    [SerializeField] private float currentDifficultyIndex = 0;
    [SerializeField] private float difficultyIncreasePerSecond = 0.2f;
    [SerializeField] private int gradeIndex = 0;

    private DifficultyGrade currentDifficultyGrade;
    private int requiredIndexForDifficultyAdvancement = 5;

    private bool spawnEnemies = false;
    private Transform playerTransform;

    IEnemyCreator EnemyCreator = new EnemyCreator();

    public bool SpawnEnemies
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => spawnEnemies;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            spawnEnemies = value;
            if ( !value )
                StopAllCoroutines();
        }
    }
    private Vector3 ownPosition;

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

        var playerMesh = FindAnyObjectByType<PlayerMesh>();
        playerTransform = playerMesh.GetTransform();

        enemyTarget.target = playerTransform;
        ownPosition = transform.position;

        GenerateObjects();

        SpawnEnemies = true;
        StartCoroutine(SpawnEnemiesIE());
        StartCoroutine(CleanUpLostEnemies());
    }

    private void RemoveEnemy ( Enemy sender )
    {
        if ( currentlySelectedTarget != null && currentlySelectedTarget.root == sender.transform )
            EventManagerGeneric<Transform>.InvokeEvent(EventType.TargetSelection, null);

        sender.OnDeath -= OnEnemyDeath;

        activeEnemies.Remove(sender);
        sender.gameObject.SetActive(false);
        objectPool.PoolObject(sender);
    }

    private async void OnEnemyDeath ( Enemy sender )
    {
        onEnemyDeath?.Invoke();

        Vector3 position = sender.MeshTransform.position;

        RemoveEnemy(sender);

        await Awaitable.BackgroundThreadAsync();

        var succes = WorldManager.InvokeCellEvent(CellEventType.OnEntityDeath, position, position);
        if ( !succes )
        {
            EventManagerGeneric<int>.InvokeEvent(EventType.UponHarvestSoul, 1);

            await Awaitable.MainThreadAsync();
            EventManagerGeneric<VectorAndTransform>.InvokeEvent(EventType.ActivateSoulEffect, new(position, playerTransform));
        }
    }

    private void GenerateObjects ()
    {
        for ( int i = 0; i < startingAmountOfPooledObjects; i++ )
        {
            var enemy = EnemyCreator.CreateEnemy(currentDifficultyGrade, transform, RemoveEnemy, OnEnemyDeath, enemyTarget, ownPosition);
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

                if ( !succes )
                {
                    enemy = EnemyCreator.CreateEnemy(currentDifficultyGrade, transform, RemoveEnemy, OnEnemyDeath, enemyTarget, position);
                }
                else
                {
                    enemy.OnReuse(currentDifficultyGrade.enemyStats, position);
                    enemy.OnDeath += OnEnemyDeath;
                }

                var gameObject = enemy.GameObject;
                gameObject.SetActive(true);

                activeEnemies.Add(enemy);
            }

            yield return Utility.Yielders.Get(currentEnemyStats.spawnSpeed);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enemy CreateEnemy ( Vector3 position )
    {
        return EnemyCreator.CreateEnemy(currentDifficultyGrade, transform, null, null, enemyTarget, position);
    }

    private void OnEnable ()
    {
        EventManagerGeneric<Transform>.AddListener(EventType.TargetSelection, ( target ) => currentlySelectedTarget = target);
    }

    private void OnDisable ()
    {
        EventManagerGeneric<UnityEngine.Transform>.RemoveListener(EventType.TargetSelection, ( target ) => currentlySelectedTarget = target);
        foreach ( var enemy in activeEnemies )
        {
            enemy.OnDeath -= OnEnemyDeath;
        }
        objectPool.ClearPool();
        activeEnemies.Clear();
        SpawnEnemies = false;

        StopAllCoroutines();
    }

    private void UpdateDifficultyIndex ()
    {
        currentDifficultyIndex += difficultyIncreasePerSecond * Time.fixedDeltaTime;

        if ( currentDifficultyIndex >= requiredIndexForDifficultyAdvancement )
        {
            currentDifficultyIndex = 0;
            if ( ++gradeIndex >= difficultyGrades.Count )
                return;

            currentDifficultyGrade = difficultyGrades[gradeIndex];
        }

        currentDifficultyGrade.enemyStats.statMultiplier += 0.001f * Time.fixedDeltaTime;
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

                if ( Vector3.Distance(enemy.MeshTransform.position, playerPos) > maxDistanceToPlayer )
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

        for ( int i = activeEnemies.Count - 1; i >= 0; --i )
        {
            var enemy = activeEnemies[i];
            if ( enemy == null )
                continue;

            enemy.OnFixedUpdate();
        }

        //UpdateDifficultyIndex();
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

public class EnemyCreator : IEnemyCreator
{
    public Enemy CreateEnemy ( DifficultyGrade difficulty, Transform transform, Action<Enemy> onDisable, Action<Enemy> onDeath, MoveTarget enemyTarget, Vector3 position )
    {
        var gameObject = UnityEngine.Object.Instantiate(difficulty.enemyPrefabs[UnityEngine.Random.Range(0, difficulty.enemyPrefabs.Count)].meshPrefab);

        var enemy = gameObject.GetOrAddComponent<Enemy>();
        enemy.OnDisabled += onDisable;
        enemy.OnDeath += onDeath;
        enemy.OnStart(difficulty.enemyStats, enemyTarget, position, () => CreateEnemyDataObject(enemy, difficulty, enemyTarget), transform);
        gameObject.SetActive(false);

        return enemy;
    }

    public CharacterData CreateEnemyDataObject ( Enemy enemy, DifficultyGrade currentDifficultyGrade, MoveTarget enemyTarget )
    {
        CharacterData data = (CharacterData)ScriptableObject.CreateInstance(nameof(CharacterData));

        var prefab = currentDifficultyGrade.RequestPrefab(enemy.EnemyType);
        var defaultStats = prefab.defaultStats;

        data.Speed = defaultStats.MoveSpeed;
        data.SpeedMultiplier = 1;
        data.Souls = 0;
        data.Stamina = 0;
        data.DamageMultiplier = 1;

        data.DamageMultiplier = defaultStats.Damage;
        data.MaxHealth = defaultStats.MaxHealth;

        if ( enemy.EnemyType == EnemyType.LieEnemy )
            data.decoyPrefab = currentDifficultyGrade.enemyPrefabs[UnityEngine.Random.Range(0, currentDifficultyGrade.enemyPrefabs.Count)].meshPrefab;

        data.CharacterTransform = enemy.MeshTransform;
        data.Speed = currentDifficultyGrade.enemyStats.MoveSpeed;

        data.MoveTarget = enemyTarget;
        data.Reset();

        return data;
    }
}

public interface IEnemyCreator
{
    public Enemy CreateEnemy ( DifficultyGrade difficulty, Transform transform, Action<Enemy> onDisable, Action<Enemy> onDeath, MoveTarget enemyTarget, Vector3 position );
    public CharacterData CreateEnemyDataObject ( Enemy enemy, DifficultyGrade currentDifficultyGrade, MoveTarget enemyTarget );
}