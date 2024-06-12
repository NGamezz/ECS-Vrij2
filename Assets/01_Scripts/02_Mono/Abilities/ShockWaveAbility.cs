using UnityEngine;

public class ShockWaveAbility : Ability
{
    private bool initialized = false;

    private float shockWaveRadius;
    private readonly Collider[] hits = new Collider[10];

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
            var hit = hits[i];
            if ( hit.gameObject.layer == ownLayer )
                continue;

            IDamageable damagable;
            if ( hit.transform.root == hit.transform )
                damagable = (IDamageable)hit.GetComponent(typeof(IDamageable));
            else
                damagable = (IDamageable)hit.GetComponentInParent(typeof(IDamageable));

            if ( damagable == null )
                continue;

            damagable.AfflictDamage(50.0f);
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