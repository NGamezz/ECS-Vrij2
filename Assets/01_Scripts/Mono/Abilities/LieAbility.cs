using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

//Should be Improved. Maybe check for a close vicinity player/decoy instead of using the movetarget.
public class LieAbility : Ability
{
    //In ms.
    private readonly int abilityDuration = 2000;

    private float2 spawnRange = new(5.0f, 5.0f);
    private int activeCount = 0;

    private async void ActivateDecoy ( Vector3 refPos, bool player, Action<CharacterData, Enemy> contextCallBackStart, Action<CharacterData, Enemy> contextCallBackFinish )
    {
        refPos.y = ownerData.CharacterTransform.position.y;
        var enemy = EnemyManager.Instance.CreateEnemy(refPos);
        enemy.GameObject.SetActive(true);
        contextCallBackStart(ownerData, enemy);

        activeCount++;
        Owner.OnExecuteAbility(Type);

        await Task.Delay(abilityDuration);

        contextCallBackFinish(ownerData, enemy);

        activeCount--;
        if ( enemy != null )
        {
            UnityEngine.Object.Destroy(enemy.GameObject);
        }
    }

    public override bool Execute ( object context )
    {
        if ( ownerData.Player && activeCount < 5 )
        {
            return CharacterBehaviour();
        }
        else if ( activeCount < 1 )
        {
            return EnemyBehaviour();
        }
        return false;
    }

    private bool CharacterBehaviour ()
    {
        if ( ownerData.Souls < ActivationCost )
            return false;

        ownerData.Souls -= (int)ActivationCost;

        var cachedTarget = ownerData.MoveTarget.target;
        ActivateDecoy(ownerData.PlayerMousePosition, true, ( data, enemy ) =>
        {
            ownerData.MoveTarget.target = enemy.MeshTransform;
            return;
        }, ( data, enemy ) =>
        {
            if ( cachedTarget == null )
                cachedTarget = ownerData.CharacterTransform;

            ownerData.MoveTarget.target = cachedTarget;
            return;
        });
        return true;
    }

    private bool EnemyBehaviour ()
    {
        var position = ownerData.CharacterTransform.position + (UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(spawnRange.x, spawnRange.y));
        ActivateDecoy(position, false, ( data, enemy ) =>
        {
            return;
        }, ( data, enemy ) =>
        {
            return;
        });
        return true;
    }

    public override void Initialize ( IAbilityOwner owner, CharacterData context )
    {
        this.Owner = owner;
        ownerData = context;

        ActivationCooldown = 3;

        ActivationCost = 10;

        Trigger = () => { return ownerData.Souls >= (ActivationCost * ownerData.abilityCostMultiplier); };
    }
}