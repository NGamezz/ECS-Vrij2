using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu]
public class GunStats : ScriptableObject
{
    public float attackSpeed;

    [OnValueChanged(nameof(UpdateCombinedValue))]
    public byte damage;
    [OnValueChanged(nameof(UpdateCombinedValue))]
    public byte projectileLifeTime;
    [OnValueChanged(nameof(UpdateCombinedValue))]
    public byte projectileSpeed;

    public Vector3 spreadOffset;

    public int amountOfBulletsPer;

    public int damageProjectileLifeTimeSpeed;

    public GameObject prefab;
    public GameObject projectTilePrefab;

    private void UpdateCombinedValue()
    {
        damageProjectileLifeTimeSpeed = damage;
        damageProjectileLifeTimeSpeed += (projectileSpeed << 8);
        damageProjectileLifeTimeSpeed += (projectileLifeTime << 16);
    }
}