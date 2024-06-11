using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using UnityEngine;

//Not really neccessary.
public class EnableTeleport : MonoBehaviour
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 GetSpawnPosition () => _spawnPos;

    [SerializeField] private Transform spawnPosition;

    private Vector3 _spawnPos;

    private void Start ()
    {
        _spawnPos = spawnPosition.position;
    }
}