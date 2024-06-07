using UnityEngine;

public class ManaUpgrade : BaseUpgrade
{
    [SerializeField] private int increaseAmount = 5;

    public override void Add ( object ctx )
    {
        if ( ctx is not CharacterData data )
            return;

        data.currentGun.MagSize += increaseAmount;
    }

    public override void Remove ( object ctx )
    {
        if ( ctx is not CharacterData data )
            return;

        data.currentGun.MagSize += increaseAmount;
    }
}