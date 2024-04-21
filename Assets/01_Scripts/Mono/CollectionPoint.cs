using NaughtyAttributes;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SphereCollider))]
public class CollectionPoint : MonoBehaviour
{
    [OnValueChanged(nameof(UpdateRange))]
    [SerializeField] private float range = 5.0f;

    [SerializeField] private bool gizmos = true;

    [SerializeField] private int souls = 0;

    [SerializeField] private int amountToTrigger = 10;

    [SerializeField] private UnityEvent eventToTrigger;
    [SerializeField] private List<Soulable> currentSoulablesWithinReach = new();

    private SphereCollider sphereCollider;

    private void UpdateRange ()
    {
        if ( sphereCollider == null )
        {
            if ( !gameObject.TryGetComponent<SphereCollider>(out sphereCollider) )
            {
                sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.isTrigger = true;
            }
        }

        sphereCollider.radius = range / 2.0f;
    }

    private void OnEnemyDeath ( object entity, EventArgs args )
    {
        Enemy enemy = (Enemy)entity;

        Vector3 entityPos = enemy.Position;

        var direction = entityPos - transform.position;

        var lenght = math.length(direction);

        if ( lenght < range )
            AddSoul(1);

        if ( souls >= amountToTrigger )
        {
            eventToTrigger?.Invoke();
            Debug.Log("Triggered Event.");
            this.enabled = false;
        }

        enemy.OnDeath -= OnEnemyDeath;
        currentSoulablesWithinReach.Remove(enemy);
    }

    private void Start ()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void AddSoul ( int amount )
    {
        souls += amount;
    }

    private void OnTriggerEnter ( Collider other )
    {
        if ( !other.TryGetComponent<Soulable>(out var soulAble) )
        { return; }

        currentSoulablesWithinReach.Add(soulAble);
    }

    private void OnTriggerExit ( Collider other )
    {
        if ( !other.TryGetComponent<Soulable>(out var soulAble) )
        { return; }

        if ( !currentSoulablesWithinReach.Contains(soulAble) )
            return;

        soulAble.OnDeath -= OnEnemyDeath;
        currentSoulablesWithinReach.Remove(soulAble);
    }

    private void OnDrawGizmos ()
    {
        if ( !gizmos )
            return;

        Gizmos.DrawWireSphere(transform.position, range / 2.0f);
    }
}