using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    private readonly ObjectPool<Gun> objectPool = new();
    private Rigidbody rb;
    private Transform bulletHolder;
    private bool shootHeld = false;
    private WaitUntil waitUntilShoot;
    private bool running = false;
    private Coroutine shootRoutine;
    private bool reloading = false;

    public void OnStart ( Transform bulletHolder, MonoBehaviour owner )
    {
        this.bulletHolder = bulletHolder;

        waitUntilShoot = new WaitUntil(() => shootHeld);

        this.owner = owner;

        if ( guns.Count < 1 )
            return;

        this.rb = (Rigidbody)meshTransform.GetComponent(typeof(Rigidbody));

        currentGun = guns[0];
        var gunObject = UnityEngine.Object.Instantiate(currentGun.prefab, gunPosition);
        gunObject.transform.position = gunPosition.position;
        currentGun.CurrentAmmo = currentGun.MagSize;
        SetLayerRecursive(gunObject, bulletHolder.gameObject.layer);

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
            var bullet = CreateBulletObject(currentGun.projectTilePrefab, OnObjectHit, playerLayer, bulletHolder);

            bullet.GameObject.SetActive(false);

            objectPool.PoolObject(bullet);
        }
    }

    private void SelectGun ( GunStats gun )
    {
        owner.StopCoroutine(shootRoutine);
        currentGun = gun;
        gun.CurrentAmmo = gun.MagSize;
        shootRoutine = owner.StartCoroutine(Shoot());
    }

    public void OnReload()
    {
        if ( !reloading )
            owner.StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        reloading = true;
        yield return Utility.Yielders.Get(currentGun.ReloadSpeed);
        currentGun.CurrentAmmo = currentGun.MagSize;
        reloading = false;
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

            while ( shootHeld && !reloading)
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
        if ( currentGun.CurrentAmmo - 1 < 0 )
            return;
        
        var succes = objectPool.GetPooledObject(out var bullet);

        if ( !succes )
        {
            bullet = CreateBulletObject(currentGun.projectTilePrefab, OnObjectHit, playerLayer, bulletHolder);
        }
        else
        {
            bullet.GameObject.SetActive(true);
        }

        UpdateBulletStats(ref bullet, currentGun, meshTransform);
        currentGun.CurrentAmmo--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateBulletStats ( ref Gun bullet, GunStats currentGun, Transform ownerTransform )
    {
        bullet.Damage = currentGun.damageProjectileLifeTimeSpeed & 255;
        bullet.Speed = (currentGun.damageProjectileLifeTimeSpeed >> 8) & 255;
        bullet.ProjectileLifeTime = (currentGun.damageProjectileLifeTimeSpeed >> 16) & 255;

        var meshForward = ownerTransform.forward;
        var newPos = ownerTransform.position + meshForward;
        bullet.Transform.position = newPos;
        var direction = (meshForward + new Vector3(Random.Range(currentGun.spreadOffset.x, currentGun.spreadOffset.y), 0.0f, Random.Range(currentGun.spreadOffset.x, currentGun.spreadOffset.y))).normalized;
        bullet.Transform.forward = direction;
        rb.AddForce(-direction * currentGun.Recoil, ForceMode.Impulse);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Gun CreateBulletObject ( GameObject prefab, Action<bool, Gun> uponHit, int playerLayer, Transform bulletHolder )
    {
        var gameObject = UnityEngine.Object.Instantiate(prefab, bulletHolder);
        var bullet = gameObject.GetOrAddComponent<Gun>();
        bullet.UponHit = uponHit;
        bullet.playerLayer = playerLayer;
        bullet.OnStart();

        return bullet;
    }
}