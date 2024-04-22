using System;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float Damage;
    public float Speed;

    public int playerLayer;

    [SerializeField] private float projectileLifeTime;
    public float ProjectileLifeTime
    {
        get => projectileLifeTime;
        set { projectileLifeTime = value; currentLifeTime = value; }
    }

    public Action<bool, GameObject> UponHit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer || other.gameObject.layer == gameObject.layer)
        {
            return;
        }

        if (!other.TryGetComponent<IDamageable>(out var damagable))
        {
            gameObject.SetActive(false);
            UponHit?.Invoke(false, gameObject);
            return;
        }

        damagable.AfflictDamage(Damage);
        gameObject.SetActive(false);
        UponHit?.Invoke(true, gameObject);
    }

    private float currentLifeTime = 0.0f;

    private void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        currentLifeTime -= Time.deltaTime;
        transform.Translate(Speed * Time.deltaTime * transform.forward, Space.World);

        if (currentLifeTime <= 0)
        {
            UponHit?.Invoke(false, gameObject);
            gameObject.SetActive(false);
        }
    }
}
