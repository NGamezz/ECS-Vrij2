using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
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

    private readonly ObjectPool<Enemy> objectPool = new();

    private GameState gameState = GameState.Running;

    [Header("Difficulty Scaling.")]
    [SerializeField] private List<DifficultyGrade> difficultyGrades = new();
    [SerializeField] private int gradeIndex = 0;

    private DifficultyGrade currentDifficultyGrade;

    private CancellationTokenSource tokenSrc = new();

    SoulCollectionPointManager soulCollectionManager;

    private bool spawnEnemies = false;
    private Transform playerTransform;

    IEnemyCreator EnemyCreator = new EnemyCreator();

    private EventSubscription subscription;

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

        DontDestroyOnLoad(gameObject);

        Instance = this;
    }

    private void SceneChange ()
    {
        OnSceneChange();
    }

    private UniTask OnSceneChange ()
    {
        var projectiles = GetComponentsInChildren(typeof(Gun), true);

        for ( int i = 0; i < projectiles.Length; ++i )
        {
            Destroy(projectiles[i].gameObject);
        }

        var pooledEnemies = objectPool.GetAllPooledObjects();

        for ( int i = 0; i < pooledEnemies.Length; ++i )
        {
            if ( pooledEnemies[i] != null && pooledEnemies[i].gameObject != null )
                Destroy(pooledEnemies[i].gameObject);
        }

        for ( int i = 0; i < activeEnemies.Count; ++i )
        {
            if ( activeEnemies[i] != null && activeEnemies[i].gameObject != null )
                Destroy(activeEnemies[i].gameObject);
        }

        EnemyCreator ??= new EnemyCreator();

        spawnEnemies = true;

        soulCollectionManager = FindAnyObjectByType<SoulCollectionPointManager>();

        UpdateDifficultyIndex();

        GenerateObjects();

        return UniTask.CompletedTask;
    }

    private void Start ()
    {
        foreach ( var difficulty in difficultyGrades )
        {
            difficulty.InitializeDictionary();
        }

        soulCollectionManager = FindAnyObjectByType<SoulCollectionPointManager>();

        currentDifficultyGrade = difficultyGrades[gradeIndex];

        var playerMesh = FindAnyObjectByType<PlayerMesh>();
        playerTransform = playerMesh.GetTransform();

        enemyTarget.target = playerTransform;
        ownPosition = transform.position;

        GenerateObjects();

        SpawnEnemies = true;
        SpawnEnemiesIE(tokenSrc.Token).Forget();
        //CleanUpLostEnemies(tokenSrc.Token).Forget();
    }

    private void RemoveEnemy ( Enemy sender )
    {
        sender.OnDeath = null;

        activeEnemies.Remove(sender);
        sender.gameObject.SetActive(false);
        objectPool.PoolObject(sender);
    }

    private void OnEnemyDeath ( Enemy sender, bool silent )
    {
        sender.gameObject.SetActive(false);
        RemoveEnemy(sender);
        UnityEngine.Debug.Log(silent);

        if ( silent )
            return;

        UnityEngine.Debug.Log(silent);
        onEnemyDeath?.Invoke();

        Vector3 position = sender.MeshTransform.position;

        var result = soulCollectionManager.CheckCollections(position);

        if ( !result )
        {
            EventManagerGeneric<VectorAndTransformAndCallBack>.InvokeEvent(EventType.ActivateSoulEffect, new(position, playerTransform, () =>
            {
                EventManagerGeneric<int>.InvokeEvent(EventType.UponHarvestSoul, 1);
            }));
        }
    }

    private void GenerateObjects ()
    {
        for ( int i = 0; i < startingAmountOfPooledObjects; i++ )
        {
            var enemy = EnemyCreator.CreateEnemy(currentDifficultyGrade, transform, OnDeathWrapper, enemyTarget, ownPosition, transform);
            objectPool.PoolObject(enemy);
        }
    }

    private void OnDeathWrapper ( Enemy sender, bool silent )
    {
        OnEnemyDeath(sender, silent);
    }

    private async UniTaskVoid SpawnEnemiesIE ( CancellationToken token )
    {
        while ( spawnEnemies )
        {
            token.ThrowIfCancellationRequested();

            var currentEnemyStats = currentDifficultyGrade.enemyStats;
            Vector3 playerPos = playerTransform.position;

            for ( int i = 0; i < currentEnemyStats.enemiesPerBatch; i++ )
            {
                token.ThrowIfCancellationRequested();

                var position = playerPos + (UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(spawnRange.x, spawnRange.y));
                position.y = ownPosition.y;

                var floor = Physics.CheckSphere(position, 1.0f, 1 << 3);

                if ( !floor )
                    continue;

                var succes = objectPool.GetPooledObject(out var enemy);

                if ( !succes )
                {
                    enemy = EnemyCreator.CreateEnemy(currentDifficultyGrade, transform, OnDeathWrapper, enemyTarget, position, transform);
                }
                else
                {
                    enemy.OnReuse(currentDifficultyGrade.enemyStats, position);
                }

                enemy.OnDeath = OnDeathWrapper;
                var gameObject = enemy.GameObject;
                gameObject.SetActive(true);

                activeEnemies.Add(enemy);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(currentDifficultyGrade.enemyStats.spawnSpeed), cancellationToken: tokenSrc.Token);
        }
    }

    private void OnEnable ()
    {
        EventManagerGeneric<GameState>.AddListener(EventType.OnGameStateChange, SetGameState);
        EventManager.AddListener(EventType.OnSceneChange, SceneChange, this);
        EventManager.AddListener(EventType.GameOver, OnGameOver, this);
    }

    private void OnGameOver ()
    {
        Destroy(gameObject);
    }

    private void SetGameState ( GameState state )
    {
        if ( gameState == GameState.Running && state == GameState.Pauzed )
        {
            tokenSrc.Cancel();
            spawnEnemies = false;
            gameState = state;

            foreach ( var enemy in activeEnemies )
            {
                enemy.agent.ResetPath();
            }
        }
        else if ( gameState == GameState.Pauzed && state == GameState.Running )
        {
            //Reset the token Source.
            tokenSrc = new();

            spawnEnemies = true;
            gameState = state;
            SpawnEnemiesIE(tokenSrc.Token).Forget();
            //CleanUpLostEnemies(tokenSrc.Token).Forget();
        }
    }

    private void OnDisable ()
    {
        EventManagerGeneric<GameState>.RemoveListener(EventType.OnGameStateChange, SetGameState);

        foreach ( var enemy in activeEnemies )
        {
            enemy.OnDeath = null;
        }

        objectPool.ClearPool();
        activeEnemies.Clear();
        SpawnEnemies = false;

        StopAllCoroutines();
    }

    private bool updatingDifficulty = false;
    private void UpdateDifficultyIndex ()
    {
        if ( updatingDifficulty )
            return;
        updatingDifficulty = true;

        if ( ++gradeIndex >= difficultyGrades.Count )
            return;

        currentDifficultyGrade = difficultyGrades[gradeIndex];

        updatingDifficulty = false;
    }

    private async UniTaskVoid CleanUpLostEnemies ( CancellationToken token )
    {
        while ( spawnEnemies )
        {
            Vector3 playerPos = playerTransform.position;

            token.ThrowIfCancellationRequested();

            for ( int i = 0; i < activeEnemies.Count; i++ )
            {
                var enemy = activeEnemies[i];
                if ( enemy == null )
                    continue;

                token.ThrowIfCancellationRequested();

                if ( Vector3.Distance(enemy.MeshTransform.position, playerPos) > maxDistanceToPlayer )
                {
                    RemoveEnemy(enemy);
                }

                if ( i != 0 && i % 10 == 0 )
                    await UniTask.NextFrame(cancellationToken: token);
            }

            await UniTask.WaitForFixedUpdate(cancellationToken: token);
        }
    }

    private void FixedUpdate ()
    {
        if ( gameState == GameState.Pauzed )
            return;

        if ( activeEnemies.Count < 1 )
            return;

        for ( int i = activeEnemies.Count - 1; i >= 0; --i )
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

public class EnemyCreator : IEnemyCreator
{
    public Enemy CreateEnemy ( DifficultyGrade difficulty, Transform transform, Action<Enemy, bool> onDeath, MoveTarget enemyTarget, Vector3 position, Transform parentTransform, bool inAnimate = false )
    {
        var gameObject = UnityEngine.Object.Instantiate(difficulty.enemyPrefabs[UnityEngine.Random.Range(0, difficulty.enemyPrefabs.Count)].meshPrefab, parentTransform);

        var enemy = gameObject.GetOrAddComponent<Enemy>();
        enemy.OnDeath = onDeath;

        enemy.OnStart(difficulty.enemyStats, enemyTarget, position, () => CreateEnemyDataObject(enemy, difficulty, enemyTarget), transform, inAnimate);
        gameObject.SetActive(false);

        return enemy;
    }

    public CharacterData CreateEnemyDataObject ( Enemy enemy, DifficultyGrade currentDifficultyGrade, MoveTarget enemyTarget )
    {
        CharacterData data = (CharacterData)ScriptableObject.CreateInstance(nameof(CharacterData));

        var prefab = currentDifficultyGrade.RequestPrefab(enemy.EnemyType);
        var defaultStats = prefab.defaultStats;

        data.Speed = defaultStats.MoveSpeed;
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

        return data;
    }
}

public interface IEnemyCreator
{
    public Enemy CreateEnemy ( DifficultyGrade difficulty, Transform transform, Action<Enemy, bool> onDeath, MoveTarget enemyTarget, Vector3 position, Transform parentTransform, bool inAnimate = false );
    public CharacterData CreateEnemyDataObject ( Enemy enemy, DifficultyGrade currentDifficultyGrade, MoveTarget enemyTarget );
}