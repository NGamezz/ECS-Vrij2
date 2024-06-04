using System.Collections;
using UnityEngine;

public class SoulEffectManager : MonoBehaviour
{
    [SerializeField] private GameObject soulEffectPrefab;

    [SerializeField] private float effectSpeed = 2.0f;

    [SerializeField] private Vector3 positionOffset = new(0.0f, 1.0f, 0.0f);

    [SerializeField] private float minDistanceToTarget = 2.0f;

    private void OnEnable ()
    {
        EventManagerGeneric<DoubleVector3>.AddListener(EventType.ActivateSoulEffect, SpawnEffect);
        EventManagerGeneric<VectorAndTransform>.AddListener(EventType.ActivateSoulEffect, SpawnEffect);
    }

    private void OnDisable ()
    {
        EventManagerGeneric<DoubleVector3>.RemoveListener(EventType.ActivateSoulEffect, SpawnEffect);
        EventManagerGeneric<VectorAndTransform>.RemoveListener(EventType.ActivateSoulEffect, SpawnEffect);
    }

    private void SpawnEffect ( DoubleVector3 positions )
    {
        MainThreadQueue.Instance.Enqueue(() =>
        {
            var gameObject = Instantiate(soulEffectPrefab);
            gameObject.transform.position = positions.a + positionOffset;
            StartCoroutine(DispatchObject(positions.b + positionOffset, gameObject.transform, effectSpeed));
        });
    }

    private void SpawnEffect ( VectorAndTransform originAndTarget )
    {
        MainThreadQueue.Instance.Enqueue(() =>
        {
            var gameObject = Instantiate(soulEffectPrefab);
            gameObject.transform.position = originAndTarget.origin + positionOffset;
            StartCoroutine(DispatchObject(originAndTarget.target, gameObject.transform, effectSpeed));
        });
    }

    private IEnumerator DispatchObject ( Transform target, Transform transform, float speed )
    {
        int count = 0;

        while ( Vector3.Distance(target.position, transform.position) > minDistanceToTarget && count < 10000 )
        {
            var dir = (target.position + positionOffset) - transform.position;
            transform.Translate(speed * Time.deltaTime * dir);

            yield return null;
        }

        Destroy(transform.gameObject);
    }

    private IEnumerator DispatchObject ( Vector3 target, Transform transform, float speed )
    {
        int count = 0;
        while ( Vector3.Distance(target, transform.position) > minDistanceToTarget && count < 10000 )
        {
            var dir = target - transform.position;
            transform.Translate(speed * Time.deltaTime * dir);

            yield return null;
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