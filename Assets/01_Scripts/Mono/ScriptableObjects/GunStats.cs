using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu]
public class GunStats : ScriptableObject
{
    public float attackSpeed;

    public GunType GunType;

    [OnValueChanged(nameof(UpdateCombinedValue))]
    public byte damage;
    [OnValueChanged(nameof(UpdateCombinedValue))]
    public byte projectileLifeTime;
    [OnValueChanged(nameof(UpdateCombinedValue))]
    public byte projectileSpeed;

    public float2 spreadOffset;

    public float ReloadSpeed = 1.0f;

    public float Recoil = 0;

    public int CurrentAmmo;
    public int MagSize;

    public byte amountOfBulletsPer;

    public int damageProjectileLifeTimeSpeed;

    public GameObject prefab;
    public GameObject projectTilePrefab;

    private void UpdateCombinedValue()
    {
        damageProjectileLifeTimeSpeed = damage;
        damageProjectileLifeTimeSpeed |= (projectileSpeed << 8);
        damageProjectileLifeTimeSpeed |= (projectileLifeTime << 16);
    }
}

public enum GunType
{
    Default = 0,
    Burst = 1,
    AssaultRifle = 2,
    Melee = 3,
}