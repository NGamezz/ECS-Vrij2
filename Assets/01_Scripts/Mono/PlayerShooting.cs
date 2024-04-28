using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class PlayerShooting
{
    [SerializeField] private List<GunStats> guns = new();

    [SerializeField] private Transform gunPosition;

    [SerializeField] private Transform meshTransform;

    [SerializeField] private int defaultAmountOfPooledObjects = 25;

    [SerializeField] private int playerLayer;

    private GunStats currentGun;

    private bool canShoot = true;

    private ObjectPool<GameObject> objectPool = new();

    private Transform bulletHolder;

    public void OnStart ( Transform bulletHolder )
    {
        this.bulletHolder = bulletHolder;

        if ( guns.Count < 1 )
            return;

        currentGun = guns[0];
        var gunObject = GameObject.Instantiate(currentGun.prefab, gunPosition);
        gunObject.transform.position = gunPosition.position;
        SetLayerRecursive(gunObject, bulletHolder.gameObject.layer);

        GenerateBullets();
    }

    private void SetLayerRecursive ( GameObject objectToSect, int layer )
    {
        objectToSect.layer = layer;
        foreach ( Transform child in objectToSect.transform )
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    private void GenerateBullets ()
    {
        for ( int i = 0; i < defaultAmountOfPooledObjects; i++ )
        {
            var gameObject = GameObject.Instantiate(currentGun.projectTilePrefab, bulletHolder);
            gameObject.SetActive(false);

            var bulletComponent = gameObject.GetComponent<Gun>();
            bulletComponent.UponHit = OnObjectHit;
            bulletComponent.playerLayer = bulletHolder.gameObject.layer;

            objectPool.PoolObject(gameObject);
        }
    }

    private void SelectGun ( GunStats gun )
    {
        currentGun = gun;
    }

    private void OnObjectHit ( bool succes, GameObject objectToPool )
    {
        objectPool.PoolObject(objectToPool);
    }

    public void OnShoot ()
    {
        if ( !canShoot )
            return;

        Shoot();
    }

    private async void Shoot ()
    {
        canShoot = false;

        for ( int i = 0; i < currentGun.amountOfBulletsPer; i++ )
        {
            bool instantiated = false;
            var bullet = objectPool.GetPooledObject();

            if ( bullet == null )
            {
                bullet = GameObject.Instantiate(currentGun.projectTilePrefab, bulletHolder);
                instantiated = true;
            }

            bullet.SetActive(true);

            var meshForward = meshTransform.forward;
            var newPos = meshTransform.position + meshForward;
            bullet.transform.position = newPos;
            bullet.transform.forward = (meshForward + new Vector3(Random.Range(currentGun.spreadOffset.x, currentGun.spreadOffset.y), 0.0f, Random.Range(currentGun.spreadOffset.x, currentGun.spreadOffset.y))).normalized;

            var gunComponent = bullet.GetComponent<Gun>();

            if ( instantiated )
            {
                gunComponent.UponHit = OnObjectHit;
                gunComponent.playerLayer = bulletHolder.gameObject.layer;
            }

            gunComponent.Damage = currentGun.damageProjectileLifeTimeSpeed & 255;
            gunComponent.Speed = (currentGun.damageProjectileLifeTimeSpeed >> 8) & 255;
            gunComponent.ProjectileLifeTime = (currentGun.damageProjectileLifeTimeSpeed >> 16) & 255;
        }

        await Awaitable.WaitForSecondsAsync(currentGun.attackSpeed);

        canShoot = true;
    }
}