using UnityEngine;

public class FireRateUpgrade : BaseUpgrade
{
    [SerializeField] private float increaseAmount = 0.5f;

    public override void Add ( object ctx )
    {
        if ( ctx is not CharacterData data || data.currentGun == null )
            return;

        data.currentGun.attackSpeed += increaseAmount;
    }

    public override void Remove ( object ctx )
    {
        if ( ctx is not CharacterData data || data.currentGun == null)
            return;

        data.currentGun.attackSpeed -= increaseAmount;
    }
}