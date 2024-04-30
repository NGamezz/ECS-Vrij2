using System;
using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float Damage;
    public float Speed;

    public Transform Transform;
    public GameObject GameObject;

    public int playerLayer;

    [SerializeField] private float projectileLifeTime;
    public float ProjectileLifeTime
    {
        get => projectileLifeTime;
        set { projectileLifeTime = value; currentLifeTime = value; }
    }

    public Action<bool, GameObject> UponHit;

    public void OnStart ()
    {
        Transform = transform;
        GameObject = gameObject;
    }

    private void OnTriggerEnter ( Collider other )
    {
        var layer = other.gameObject.layer;
        if ( layer == playerLayer || layer == GameObject.layer )
        {
            return;
        }

        if ( !other.TryGetComponent<IDamageable>(out var damagable) )
        {
            GameObject.SetActive(false);
            UponHit?.Invoke(false, GameObject);
            return;
        }
        GameObject.SetActive(false);

        if ( damagable.IsDead )
        {
            return;
        }

        damagable.AfflictDamage(Damage);
        UponHit?.Invoke(true, GameObject);
    }

    private float currentLifeTime = 0.0f;

    private void Update ()
    {
        if ( !GameObject.activeInHierarchy )
            return;

        currentLifeTime -= Time.deltaTime;
        Transform.Translate(Speed * Time.deltaTime * Transform.forward, Space.World);

        if ( currentLifeTime <= 0 )
        {
            UponHit?.Invoke(false, GameObject);
            GameObject.SetActive(false);
        }
    }
}