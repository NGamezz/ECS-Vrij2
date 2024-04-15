using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SetPlayerPositionMono : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    void Start ()
    {
        SetPlayerPositionSystem playerPositionSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SetPlayerPositionSystem>();
        playerPositionSystem.onRequestPlayerPosition = GetFloat3PlayerPos;
    }

    private float3 GetFloat3PlayerPos ()
    {
        return playerTransform.position;
    }
}