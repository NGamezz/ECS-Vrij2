using Cysharp.Threading.Tasks;
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

        data.OnHit += () => StartInvincibility(data).Forget();
    }

    private async UniTaskVoid StartInvincibility ( CharacterData ownerData )
    {
        if ( ownerData.canTakeDamage == false )
            return;

        ownerData.canTakeDamage = false;
        await UniTask.Delay(TimeSpan.FromSeconds(invincibilityDuration));
        ownerData.canTakeDamage = true;
    }

    public override void Remove ( object ctx )
    {
        if ( ctx is not CharacterData data)
            return;

        data.OnHit -= ()=> StartInvincibility(data).Forget();
    }
}