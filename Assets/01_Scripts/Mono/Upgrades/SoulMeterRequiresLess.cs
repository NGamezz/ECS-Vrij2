using UnityEngine;

public class SoulMeterRequiresLess : BaseUpgrade
{
    [SerializeField] private float multiplierIncrease = 0.05f;

    public override void Add ( object ctx )
    {
        if ( ctx is not CharacterData data)
            return;

        data.abilityCostMultiplier -= multiplierIncrease;
    }

    public override void Remove ( object ctx )
    {
        if ( ctx is not CharacterData data)
            return;

        data.abilityCostMultiplier += multiplierIncrease;
    }
}