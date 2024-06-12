using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

//Not really neccessary.
public class EnableTeleport : MonoBehaviour
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 GetSpawnPosition () => _spawnPos;

    [SerializeField] private Transform spawnPosition;

    [SerializeField] private UnityEvent OnTeleport;

    private Vector3 _spawnPos;

    public void Teleport ()
    {
        OnTeleport?.Invoke();
    }

    private void Start ()
    {
        _spawnPos = spawnPosition.position;
    }
}