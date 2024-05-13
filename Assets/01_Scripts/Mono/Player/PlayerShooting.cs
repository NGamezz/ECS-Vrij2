using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public enum ProjectileType
{
    Default = 0,
}

[Serializable]
public class PlayerShooting
{
    [SerializeField] private List<GunStats> guns = new();

    [SerializeField] private Transform gunPosition;

    [SerializeField] private Transform meshTransform;

    [SerializeField] private int defaultAmountOfPooledObjects = 25;

    [SerializeField] private int playerLayer;

    private MonoBehaviour owner;

    private GunStats currentGun;

    private bool canShoot = true;

    private ObjectPool<Gun> objectPool = new();

    private Transform bulletHolder;

    private bool shootHeld = false;

    private int bulletHolderLayer;

    private WaitUntil waitUntilShoot;

    private bool running = false;

    private Coroutine shootRoutine;

    public void OnStart ( Transform bulletHolder, MonoBehaviour owner )
    {
        this.bulletHolder = bulletHolder;

        waitUntilShoot = new WaitUntil(() => shootHeld);

        this.owner = owner;

        if ( guns.Count < 1 )
            return;

        currentGun = guns[0];
        var gunObject = UnityEngine.Object.Instantiate(currentGun.prefab, gunPosition);
        gunObject.transform.position = gunPosition.position;
        SetLayerRecursive(gunObject, bulletHolder.gameObject.layer);

        bulletHolderLayer = bulletHolder.gameObject.layer;
        GenerateBullets();

        running = true;
        shootRoutine = owner.StartCoroutine(Shoot());
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
            var gameObject = UnityEngine.Object.Instantiate(currentGun.projectTilePrefab, bulletHolder);
            gameObject.SetActive(false);

            var bulletComponent = gameObject.GetComponent<Gun>();
            bulletComponent.UponHit = OnObjectHit;
            bulletComponent.playerLayer = bulletHolderLayer;

            bulletComponent.OnStart();

            objectPool.PoolObject(bulletComponent);
        }
    }

    private void SelectGun ( GunStats gun )
    {
        owner.StopCoroutine(shootRoutine);
        currentGun = gun;
        shootRoutine = owner.StartCoroutine(Shoot());
    }

    private void OnObjectHit ( bool succes, Gun objectToPool )
    {
        objectPool.PoolObject(objectToPool);
    }

    public void OnShoot ( InputAction.CallbackContext context )
    {
        shootHeld = context.ReadValueAsButton();
    }

    private IEnumerator Shoot ()
    {
        while ( running )
        {
            yield return waitUntilShoot;

            while ( shootHeld )
            {
                CheckShootType();

                if ( currentGun.GunType == GunType.Burst )
                    yield return Utility.Yielders.Get(currentGun.attackSpeed);

                yield return Utility.Yielders.Get(currentGun.attackSpeed);
            }
        }
    }

    private void CheckShootType ()
    {
        switch ( currentGun.GunType )
        {
            case GunType.Default:
                {
                    RegularShoot();
                    break;
                }
            case GunType.Burst:
                {
                    BurstFire();
                    break;
                }
            case GunType.AssaultRifle:
                {
                    RegularShoot();
                    break;
                }
            case GunType.Melee:
                {
                    Melee();
                    break;
                }
        }
    }

    private void RegularShoot ()
    {
        for ( int i = 0; i < currentGun.amountOfBulletsPer; i++ )
        {
            ShootBody();
        }
    }

    private async void BurstFire ()
    {
        for ( int i = 0; i < currentGun.amountOfBulletsPer; i++ )
        {
            ShootBody();
            await Task.Delay(75);
        }
    }

    private void ShootBody ()
    {
        bool instantiated = false;
        var bullet = objectPool.GetPooledObject();

        if ( bullet == null )
        {
            var gameObject = UnityEngine.Object.Instantiate(currentGun.projectTilePrefab, bulletHolder);
            instantiated = true;
            gameObject.SetActive(true);
            bullet = gameObject.GetComponent<Gun>();
        }
        else
        {
            bullet.GameObject.SetActive(true);
        }

        if ( instantiated )
        {
            bullet.UponHit = OnObjectHit;
            bullet.playerLayer = bulletHolderLayer;
            bullet.OnStart();
        }

        bullet.Damage = currentGun.damageProjectileLifeTimeSpeed & 255;
        bullet.Speed = (currentGun.damageProjectileLifeTimeSpeed >> 8) & 255;
        bullet.ProjectileLifeTime = (currentGun.damageProjectileLifeTimeSpeed >> 16) & 255;

        var meshForward = meshTransform.forward;
        var newPos = meshTransform.position + meshForward;
        bullet.Transform.position = newPos;
        bullet.Transform.forward = (meshForward + new Vector3(Random.Range(currentGun.spreadOffset.x, currentGun.spreadOffset.y), 0.0f, Random.Range(currentGun.spreadOffset.x, currentGun.spreadOffset.y))).normalized;
    }

    private void Melee ()
    {
    }
}