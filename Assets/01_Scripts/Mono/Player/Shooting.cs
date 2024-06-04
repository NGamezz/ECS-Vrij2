using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

[Serializable]
public class Shooting
{
    public GunStats currentGun;
    public float recoilMultiplier = 1;
    public CharacterData ownerData;

    [SerializeField] private Transform gunPosition;
    [SerializeField] private Transform meshTransform;
    [SerializeField] private int defaultAmountOfPooledObjects = 25;
    [SerializeField] private int ownLayer;

    private List<Gun> activeBullets = new();

    private bool shootHeld = false;
    private MonoBehaviour owner;
    private readonly ObjectPool<Gun> objectPool = new();
    private Rigidbody rb;
    private Transform bulletHolder;
    private WaitUntil waitUntilShoot;
    private bool running = false;
    private Coroutine shootRoutine;
    private bool reloading = false;

    public void OnStart ( Transform bulletHolder, MonoBehaviour owner )
    {
        this.bulletHolder = bulletHolder;

        waitUntilShoot = new WaitUntil(() => shootHeld);

        if ( currentGun == null )
            return;

        this.owner = owner;
        rb = (Rigidbody)meshTransform.GetComponent(typeof(Rigidbody));

        var gunObject = UnityEngine.Object.Instantiate(currentGun.prefab, gunPosition);
        gunObject.transform.position = gunPosition.position;
        currentGun.CurrentAmmo = currentGun.MagSize;
        SetLayerRecursive(gunObject, meshTransform.gameObject.layer);

        GenerateBullets();

        if ( !ownerData.Player )
            return;

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
            var bullet = CreateBulletObject(currentGun.projectTilePrefab, OnObjectHit, ownLayer, bulletHolder);

            bullet.GameObject.SetActive(false);

            objectPool.PoolObject(bullet);
        }
    }

    public void SelectGun ( GunStats gun )
    {
        if ( shootRoutine != null )
            owner.StopCoroutine(shootRoutine);

        currentGun = gun;
        gun.CurrentAmmo = gun.MagSize;
        shootRoutine = owner.StartCoroutine(Shoot());
    }

    private Coroutine reloadRoutine;
    public void OnReload ()
    {
        if ( reloadRoutine == null )
        {
            if ( ownerData.Player )
                EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, "Reloading."));

            reloadRoutine ??= owner.StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload ()
    {
        reloading = true;
        yield return Utility.Yielders.Get(currentGun.ReloadSpeed);
        currentGun.CurrentAmmo = currentGun.MagSize;
        reloading = false;
    }

    private void OnObjectHit ( bool succes, Gun objectToPool )
    {
        if ( activeBullets.Contains(objectToPool) )
            activeBullets.Remove(objectToPool);
        objectPool.PoolObject(objectToPool);
    }

    public void OnShoot ( InputAction.CallbackContext context )
    {
        shootHeld = context.ReadValueAsButton();
    }

    public void OnDisable ()
    {
        for ( int i = activeBullets.Count - 1; i >= 0; i-- )
        {
            activeBullets[i].GameObject.SetActive(false);
            activeBullets.RemoveAt(i);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ShootSingle ()
    {
        CheckShootType();
    }

    private IEnumerator Shoot ()
    {
        while ( running )
        {
            yield return new WaitUntil(()=> shootHeld);

            Debug.Log("Shoot");

            while ( shootHeld && !reloading )
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
            bullet = CreateBulletObject(currentGun.projectTilePrefab, OnObjectHit, ownLayer, bulletHolder);
        }
        else
        {
            bullet.GameObject.SetActive(true);
        }

        activeBullets.Add(bullet);
        UpdateBulletStats(ref bullet, currentGun, meshTransform);
        currentGun.CurrentAmmo--;

        if ( currentGun.CurrentAmmo <= 0 )
            OnReload();
    }

    private void UpdateBulletStats ( ref Gun bullet, GunStats currentGun, Transform ownerTransform )
    {
        bullet.Damage = (currentGun.damageProjectileLifeTimeSpeed & 255) * ownerData.DamageMultiplier;
        bullet.Speed = (currentGun.damageProjectileLifeTimeSpeed >> 8) & 255;
        bullet.ProjectileLifeTime = (currentGun.damageProjectileLifeTimeSpeed >> 16) & 255;

        var meshForward = ownerTransform.forward;
        meshForward.y = 0.0f;

        var newPos = gunPosition.position + meshForward;
        bullet.Transform.position = newPos;
        var direction = (meshForward + new Vector3(Random.Range(currentGun.spreadOffset.x, currentGun.spreadOffset.y), 0.0f, Random.Range(currentGun.spreadOffset.x, currentGun.spreadOffset.y))).normalized;
        bullet.Transform.forward = direction;

        if ( recoilMultiplier > 0 )
            rb.AddForce(currentGun.Recoil * recoilMultiplier * -direction, ForceMode.Impulse);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Gun CreateBulletObject ( GameObject prefab, Action<bool, Gun> uponHit, int playerLayer, Transform bulletHolder )
    {
        var gameObject = UnityEngine.Object.Instantiate(prefab, bulletHolder);
        gameObject.layer = playerLayer;
        var bullet = gameObject.GetOrAddComponent<Gun>();
        bullet.UponHit = uponHit;
        bullet.playerLayer = playerLayer;
        bullet.OnStart();

        return bullet;
    }
}