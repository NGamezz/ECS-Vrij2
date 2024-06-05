using UnityEngine;

public class ShieldEveryX : BaseUpgrade
{
    [SerializeField] private float rechargeSpeed = 6.0f;

    public override void Add ( object ctx )
    {
        if ( ctx is not CharacterData data || data.hasShield )
            return;

        data.shieldRechargeSpeed = rechargeSpeed;

        data.hasShield = true;
        data.shieldActive = true;
    }

    public override void Remove ( object ctx )
    {
        if ( ctx is not CharacterData data || !data.hasShield )
            return;

        data.shieldActive = false;
        data.hasShield = false;
    }
}