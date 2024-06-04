using System;
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

    public Action<bool, Gun> UponHit;

    public void OnStart ()
    {
        Transform = transform;
        GameObject = gameObject;
    }

    private void OnTriggerEnter ( Collider other )
    {
        var layer = other.gameObject.layer;
        if ( layer == playerLayer || layer == GameObject.layer)
        {
            return;
        }

        IDamageable damagable;
        if ( other.transform.root == other.transform )
            damagable = (IDamageable)other.GetComponent(typeof(IDamageable));
        else
            damagable = (IDamageable)other.GetComponentInParent(typeof(IDamageable));

        if ( damagable == null )
        {
            GameObject.SetActive(false);
            UponHit?.Invoke(false, this);
            return;
        }

        GameObject.SetActive(false);
        if ( damagable.Dead )
        {
            return;
        }

        damagable.AfflictDamage(Damage);
        UponHit?.Invoke(true, this);
    }

    private float currentLifeTime = 0.0f;

    private void Update ()
    {
        float deltaTime = Time.deltaTime;

        currentLifeTime -= deltaTime;

        Transform.Translate(Speed * deltaTime * Transform.forward, Space.World);

        if ( currentLifeTime <= 0 )
        {
            UponHit?.Invoke(false, this);
            GameObject.SetActive(false);
        }
    }
}