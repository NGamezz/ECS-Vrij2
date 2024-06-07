using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class SoulEffectManager : MonoBehaviour
{
    [SerializeField] private GameObject soulEffectPrefab;

    [SerializeField] private float effectSpeed = 2.0f;

    [SerializeField] private Vector3 positionOffset = new(0.0f, 1.0f, 0.0f);

    [SerializeField] private float minDistanceToTarget = 2.0f;

    private Vector3 ownPosition;

    private void Start ()
    {
        ownPosition = transform.position;
    }

    private void SpawnEffectWrapper ( DoubleVector3 positions )
    {
        SpawnEffect(positions).Forget();
    }

    private void SpawnEffectWrapper ( VectorAndTransform vecTransform )
    {
        SpawnEffect(vecTransform).Forget();
    }

    private void OnEnable ()
    {
        EventManagerGeneric<DoubleVector3>.AddListener(EventType.ActivateSoulEffect, SpawnEffectWrapper);
        EventManagerGeneric<VectorAndTransform>.AddListener(EventType.ActivateSoulEffect, SpawnEffectWrapper);
        EventManagerGeneric<VectorAndTransformAndCallBack>.AddListener(EventType.ActivateSoulEffect, SpawnEffectWrapper);
    }

    private void OnDisable ()
    {
        EventManagerGeneric<DoubleVector3>.RemoveListener(EventType.ActivateSoulEffect, SpawnEffectWrapper);
        EventManagerGeneric<VectorAndTransform>.RemoveListener(EventType.ActivateSoulEffect, SpawnEffectWrapper);
        EventManagerGeneric<VectorAndTransformAndCallBack>.RemoveListener(EventType.ActivateSoulEffect, SpawnEffectWrapper);
    }

    private void SpawnEffectWrapper ( VectorAndTransformAndCallBack data )
    {
        SpawnEffect(data).Forget();
    }

    private async UniTaskVoid SpawnEffect ( VectorAndTransformAndCallBack data )
    {
        await UniTask.SwitchToMainThread();

        var gameObject = Instantiate(soulEffectPrefab);

        data.origin.y = positionOffset.y;
        gameObject.transform.position = data.origin;

        DispatchObject(data.target, gameObject.transform, effectSpeed, data.callBack).Forget();
    }

    private async UniTaskVoid SpawnEffect ( DoubleVector3 positions )
    {
        await UniTask.SwitchToMainThread();

        var gameObject = Instantiate(soulEffectPrefab);

        positions.a.y = positionOffset.y;
        gameObject.transform.position = positions.a;

        DispatchObject(positions.b + positionOffset, gameObject.transform, effectSpeed).Forget();
    }

    private async UniTaskVoid SpawnEffect ( VectorAndTransform originAndTarget )
    {
        await UniTask.SwitchToMainThread();

        var gameObject = Instantiate(soulEffectPrefab);

        originAndTarget.origin.y = positionOffset.y;
        gameObject.transform.position = originAndTarget.origin;

        DispatchObject(originAndTarget.target, gameObject.transform, effectSpeed).Forget();
    }

    private async UniTaskVoid DispatchObject ( Transform target, Transform transform, float speed )
    {
        int count = 0;

        var targetPos = target.position;
        var otherPos = transform.position;

        while ( Vector3.Distance(targetPos, otherPos) > minDistanceToTarget && count < 10000 )
        {
            targetPos = target.position;
            targetPos.y = ownPosition.y;

            otherPos = transform.position;
            otherPos.y = ownPosition.y;

            var dir = (targetPos + positionOffset) - otherPos;
            dir.y = 0.0f;

            transform.Translate(speed * Time.deltaTime * dir);

            await UniTask.NextFrame();
        }

        Destroy(transform.gameObject);
    }

    private async UniTaskVoid DispatchObject ( Transform target, Transform transform, float speed, Action completionCallBack )
    {
        int count = 0;

        var targetPos = target.position;
        var otherPos = transform.position;

        while ( Vector3.Distance(targetPos, otherPos) > minDistanceToTarget && count < 10000 )
        {
            targetPos = target.position;
            targetPos.y = ownPosition.y;

            otherPos = transform.position;
            otherPos.y = ownPosition.y;

            var dir = (targetPos + positionOffset) - otherPos;
            dir.y = 0.0f;

            transform.Translate(speed * Time.deltaTime * dir);

            await UniTask.NextFrame();
        }

        completionCallBack?.Invoke();

        Destroy(transform.gameObject);
    }

    private async UniTaskVoid DispatchObject ( Vector3 target, Transform transform, float speed )
    {
        int count = 0;

        var otherPos = transform.position;

        while ( Vector3.Distance(target, otherPos) > minDistanceToTarget && count < 10000 )
        {
            otherPos = transform.position;
            otherPos.y = ownPosition.y;

            var dir = target - otherPos;
            dir.y = 0.0f;

            transform.Translate(speed * Time.deltaTime * dir);

            await UniTask.NextFrame();
        }

        Destroy(transform.gameObject);
    }
}

public struct DoubleVector3
{
    public Vector3 a;
    public Vector3 b;

    public DoubleVector3 ( Vector3 a, Vector3 b )
    {
        this.a = a;
        this.b = b;
    }
}

public struct VectorAndTransform
{
    public Vector3 origin;
    public Transform target;

    public VectorAndTransform ( Vector3 origin, Transform target )
    {
        this.origin = origin;
        this.target = target;
    }
}

public struct VectorAndTransformAndCallBack
{
    public Vector3 origin;
    public Transform target;
    public Action callBack;

    public VectorAndTransformAndCallBack ( Vector3 origin, Transform target, Action callBack )
    {
        this.origin = origin;
        this.target = target;
        this.callBack = callBack;
    }
}