using Cysharp.Threading.Tasks;
using System;
using Unity.Mathematics;
using UnityEngine;

//Should be Improved. Maybe check for a close vicinity player/decoy instead of using the movetarget.
public class LieAbility : Ability
{
    //In seconds.
    private readonly int abilityDuration = 2;

    private float2 spawnRange = new(5.0f, 5.0f);
    private int activeCount = 0;

    private async UniTaskVoid ActivateDecoy ( Vector3 refPos, Action<CharacterData, Enemy> contextCallBackStart, Action<CharacterData, Enemy> contextCallBackFinish, Transform objectToCopy )
    {
        refPos.y = ownerData.CharacterTransform.position.y;

        var transform = GameObject.Instantiate(objectToCopy);

        transform.gameObject.SetActive(true);

        var enemy = (Enemy)transform.gameObject.GetComponent(typeof(Enemy));

        contextCallBackStart?.Invoke(ownerData, enemy);

        activeCount++;
        Owner.OnExecuteAbility(Type);

        await UniTask.Delay(TimeSpan.FromSeconds(abilityDuration));

        contextCallBackFinish?.Invoke(ownerData, enemy);

        if ( enemy != null && ownerData.TargetedTransform == transform )
        {
            EventManagerGeneric<Transform>.InvokeEvent(EventType.TargetSelection, null);
        }

        activeCount--;
        if ( transform != null )
        {
            UnityEngine.Object.Destroy(transform.gameObject);
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

        var position = ownerData.CharacterTransform.position + (UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(spawnRange.x, spawnRange.y));
        var cachedTarget = ownerData.MoveTarget.target;

        ActivateDecoy(position, ( data, enemy ) =>
        {
            ownerData.MoveTarget.target = enemy.MeshTransform;
            return;
        }, ( data, enemy ) =>
        {
            if ( cachedTarget == null )
                cachedTarget = ownerData.CharacterTransform;

            ownerData.MoveTarget.target = cachedTarget;
            return;
        }, ownerData.CharacterTransform).Forget();
        return true;
    }

    private bool EnemyBehaviour ()
    {
        var position = ownerData.CharacterTransform.position + (UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(spawnRange.x, spawnRange.y));

        ActivateDecoy(position, null, null, ownerData.CharacterTransform).Forget();
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