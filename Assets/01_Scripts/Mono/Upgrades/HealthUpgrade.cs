using UnityEngine;

public class HealthUpgrade : BaseUpgrade
{
    [SerializeField] private float increaseAmount = 10;

    public override void Add ( object ctx )
    {
        if ( ctx is not CharacterData data )
            return;

        data.Health += increaseAmount;
        data.MaxHealth += increaseAmount;
    }

    public override void Remove ( object ctx )
    {
        if ( ctx is not CharacterData data )
            return;
        
        data.MaxHealth -= increaseAmount;
    }
}