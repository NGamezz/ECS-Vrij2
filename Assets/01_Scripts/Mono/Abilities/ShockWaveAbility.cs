using System.Threading.Tasks;
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

        if ( ownerData.Player && ownerData.Souls < ActivationCost )
        {
            return false;
        }

        var ownerPos = ownerData.CharacterTransform.position;

        var hitCount = Physics.OverlapSphereNonAlloc(ownerPos, shockWaveRadius, this.hits);
        if ( hitCount == 0 )
            return false;

        ownerData.Souls -= (int)ActivationCost;
        for ( int i = 0; i < hitCount; i++ )
        {
            if ( hits[i].transform == ownerData.CharacterTransform )
                continue;

            IDamageable hit;
            if ( hits[i].transform.root == hits[i].transform )
                hit = (IDamageable)hits[i].GetComponent(typeof(IDamageable));
            else
                hit = (IDamageable)hits[i].GetComponentInParent(typeof(IDamageable));

            if ( hit == null )
                continue;

            hit.AfflictDamage(999999);
        }
        return true;
    }

    public override void Initialize ( IAbilityOwner owner, CharacterData context )
    {
        initialized = true;
        ownerData = context;

        ActivationCooldown = 5;

        ActivationCost = 10;
        shockWaveRadius = 10;

        Trigger = () => { return InputHandler.IsKeyPressed(VirtualKeys.KeyO); };
    }
}