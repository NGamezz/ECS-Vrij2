using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using System;
using UnityEngine;

public class ShockWaveAbility : Ability
{
    private bool initialized = false;

    private float shockWaveRadius;
    private readonly Collider[] hits = new Collider[10];

    public float attackDelay = 1;

    public override bool Execute ( object context )
    {
        if ( !initialized )
        { return false; }

        var ownerPos = ownerData.CharacterTransform.position;
        var ownLayer = ownerData.CharacterTransform.gameObject.layer;

        Owner.OnExecuteAbility(Type);

        var hitCount = Physics.OverlapSphereNonAlloc(ownerPos, shockWaveRadius, this.hits);
        if ( hitCount == 0 )
        {
            Debug.Log("No Hits within Range.");
            return false;
        }

        ownerData.Souls -= (int)ActivationCost;
        for ( int i = 0; i < hitCount; i++ )
        {
            if ( hits[i].gameObject.layer == ownLayer )
                continue;

            IDamageable hit;
            if ( hits[i].transform.root == hits[i].transform )
                hit = (IDamageable)hits[i].GetComponent(typeof(IDamageable));
            else
                hit = (IDamageable)hits[i].GetComponentInParent(typeof(IDamageable));

            if ( hit == null )
                continue;

            hit.AfflictDamage(float.MaxValue);
        }

        if ( Owner == null )
            return false;

        return true;
    }

    public override void Initialize ( IAbilityOwner owner, CharacterData context )
    {
        initialized = true;
        ownerData = context;

        Type = AbilityType.ShockWaveAbility;
        Owner = owner;

        ActivationCooldown = 5;

        ActivationCost = 10;
        shockWaveRadius = 10;

        Trigger = () => { return ownerData.Souls >= (ActivationCost * ownerData.abilityCostMultiplier); };
    }
}