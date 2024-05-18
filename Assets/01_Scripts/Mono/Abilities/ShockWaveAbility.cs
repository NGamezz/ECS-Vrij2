using System.Threading.Tasks;
using UnityEngine;

public class ShockWaveAbility : Ability
{
    private CharacterData ownerData;
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

        Debug.Log(ownerData);
        Debug.Log(ownerData.CharacterTransform);

        var ownerPos = ownerData.CharacterTransform.position;

        var hitCount = Physics.OverlapSphereNonAlloc(ownerPos, shockWaveRadius, this.hits);
        if ( hitCount == 0 )
            return false;

        ownerData.Souls -= (int)ActivationCost;
        for ( int i = 0; i < hitCount; i++ )
        {
            if ( hits[i].transform == ownerData.CharacterTransform )
                continue;

            var hit = (IDamageable)hits[i].GetComponent(typeof(IDamageable));
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

        ActivationCost = 10;
        shockWaveRadius = 10;

        Trigger = () => { return Input.GetKeyDown(KeyCode.Space); };
    }
}