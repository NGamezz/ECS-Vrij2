using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

//Should be Improved. Maybe check for a close vicinity player/decoy instead of using the movetarget.
public class LieAbility : Ability
{
    //In ms.
    private readonly int abilityDuration = 2000;
    
    private CharacterData ownerData;
    private float2 spawnRange = new(5.0f, 5.0f);
    private int activeCount = 0;

    private async void ActivateDecoy (Vector3 refPos)
    {
        var gameObject = Object.Instantiate(ownerData.decoyPrefab);
        refPos.y = ownerData.CharacterTransform.position.y;
        gameObject.transform.position = refPos;

        gameObject.name = ownerData.CharacterTransform.name + "Clone" + activeCount;

        activeCount++;
        ownerData.MoveTarget.target = gameObject.transform;

        await Task.Delay(abilityDuration);

        if ( ownerData.MoveTarget.target == null )
            ownerData.MoveTarget.target = ownerData.CharacterTransform;

        activeCount--;
        if ( gameObject != null )
        {
            Object.Destroy(gameObject);
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
        ActivateDecoy(ownerData.PlayerMousePosition);
        return false;
    }

    private bool EnemyBehaviour ()
    {
        var position = ownerData.CharacterTransform.position + (UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(spawnRange.x, spawnRange.y));
        ActivateDecoy(position);
        return true;
    }

    public override void Initialize ( IAbilityOwner owner, CharacterData context )
    {
        this.Owner = owner;
        ownerData = context;

        ActivationCost = 10;

        Trigger = () => { return Input.GetKeyDown(KeyCode.Q); };
    }
}