using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SetPlayerPositionMono : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    void Start ()
    {
        var world = World.DefaultGameObjectInjectionWorld;

        SetPlayerPositionSystem playerPositionSystem = world.GetExistingSystemManaged<SetPlayerPositionSystem>();
        playerPositionSystem.onRequestPlayerPosition = GetFloat3PlayerPos;

        PlayerShootingSystem shootingSystem = world.GetExistingSystemManaged<PlayerShootingSystem>();
        shootingSystem.requestPlayerPosition = GetFloat3PlayerPos;
        shootingSystem.requestPlayerRotation = GetPlayerRotation;

        SpawnEnemies spawnEnemiesSystem = world.GetExistingSystemManaged<SpawnEnemies>();
        spawnEnemiesSystem.requestPlayerPosition = GetFloat3PlayerPos;
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