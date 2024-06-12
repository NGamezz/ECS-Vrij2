using Cysharp.Threading.Tasks;
using UnityEngine;

public class SendSoulToPlayerOrEnv : MonoBehaviour
{
    [SerializeField] private SoulCollectionPointManager _pointManager;
    private Transform playerTransform;

    void Start ()
    {
        playerTransform = FindAnyObjectByType<PlayerMesh>().GetTransform();
        _pointManager = FindAnyObjectByType<SoulCollectionPointManager>();
    }

    public void CheckWrapper ( Vector3 pos )
    {
        Check(pos);
    }

    public void Check ( Vector3 position )
    {
        var result = _pointManager.CheckCollections(position);

        if ( !result )
        {
            EventManagerGeneric<VectorAndTransformAndCallBack>.InvokeEvent(EventType.ActivateSoulEffect, new(position, playerTransform, () =>
            {
                EventManagerGeneric<int>.InvokeEvent(EventType.UponHarvestSoul, 1);
            }));
        }
    }
}