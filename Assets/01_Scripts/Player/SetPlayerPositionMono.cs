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

        PlayerShootingSystem shootingSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlayerShootingSystem>();
        shootingSystem.requestPlayerPosition = GetFloat3PlayerPos;
        shootingSystem.requestPlayerRotation += GetPlayerRotation;
    }

    private Quaternion GetPlayerRotation()
    {
        return playerTransform.rotation;
    }

    private float3 GetFloat3PlayerPos ()
    {
        return playerTransform.position;
    }
}