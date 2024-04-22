using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private List<GunStats> guns = new();

    [SerializeField] private Transform gunPosition;

    [SerializeField] private Transform meshTransform;

    [SerializeField] private int defaultAmountOfPooledObjects = 25;

    [SerializeField] private int playerLayer;

    private GunStats currentGun;

    private Coroutine currentShootRoutine;

    private bool canShoot = true;

    private ObjectPool<GameObject> objectPool = new();

    void Start()
    {
        if (guns.Count < 1)
            return;

        currentGun = guns[0];
        var gunObject = Instantiate(currentGun.prefab, gunPosition);
        gunObject.transform.position = gunPosition.position;
        SetLayerRecursive(gunObject, gameObject.layer);

        GenerateBullets();
    }

    private void SetLayerRecursive(GameObject objectToSect, int layer)
    {
        objectToSect.layer = layer;
        foreach (Transform child in objectToSect.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    private void GenerateBullets()
    {
        for (int i = 0; i < defaultAmountOfPooledObjects; i++)
        {
            var gameObject = Instantiate(currentGun.projectTilePrefab, transform);
            gameObject.SetActive(false);

            var bulletComponent = gameObject.GetComponent<Gun>();
            bulletComponent.UponHit = OnObjectHit;
            bulletComponent.playerLayer = this.gameObject.layer;

            objectPool.PoolObject(gameObject);
        }
    }

    private void SelectGun(GunStats gun)
    {
        StopCoroutine(currentShootRoutine);

        currentGun = gun;
    }

    private void OnObjectHit(bool succes, GameObject objectToPool)
    {
        objectPool.PoolObject(objectToPool);
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (!canShoot)
            return;

        currentShootRoutine = StartCoroutine(Shoot());
    }

    private IEnumerator Shoot()
    {
        canShoot = false;

        for (int i = 0; i < currentGun.amountOfBulletsPer; i++)
        {
            bool instantiated = false;

            var bullet = objectPool.GetPooledObject();
            if (bullet == null)
            {
                bullet = Instantiate(currentGun.projectTilePrefab, transform);
                instantiated = true;
            }

            bullet.SetActive(true);

            var newPos = (meshTransform.position + currentGun.spreadOffset) + (meshTransform.forward);
            bullet.transform.SetPositionAndRotation(newPos, meshTransform.rotation);

            var gunComponent = bullet.GetComponent<Gun>();

            if (instantiated)
            {
                gunComponent.UponHit = OnObjectHit;
                gunComponent.playerLayer = gameObject.layer;
            }

            gunComponent.Damage = currentGun.damageProjectileLifeTimeSpeed & 255;
            gunComponent.Speed = (currentGun.damageProjectileLifeTimeSpeed >> 8) & 255;
            gunComponent.ProjectileLifeTime = (currentGun.damageProjectileLifeTimeSpeed >> 16) & 255;
        }

        yield return new WaitForSeconds(currentGun.attackSpeed);

        canShoot = true;
    }
}