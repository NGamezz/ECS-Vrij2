using System;
using System.Threading.Tasks;
using UnityEngine;

public class InvincibleAfterHit : BaseUpgrade
{
    [SerializeField] private float invincibilityDuration = 1.0f;

    public override void Add ( object ctx )
    {
        if ( ctx is not CharacterData data)
            return;

        data.OnHit += () => StartInvincibility(data);
    }

    private async void StartInvincibility ( CharacterData ownerData )
    {
        if ( ownerData.canTakeDamage == false )
            return;

        ownerData.canTakeDamage = false;
        await Task.Delay(TimeSpan.FromSeconds(invincibilityDuration));
        ownerData.canTakeDamage = true;
    }

    public override void Remove ( object ctx )
    {
        if ( ctx is not CharacterData data)
            return;

        data.OnHit -= ()=> StartInvincibility(data);
    }
}