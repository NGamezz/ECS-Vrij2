using Cysharp.Threading.Tasks;
using UnityEngine;

public class SnitchingAbility : Ability
{
    public override bool Execute ( object context )
    {
        return (true);
        //Debug.Log("Snitch Ability");
    }

    public override void Initialize ( IAbilityOwner owner, CharacterData context )
    {
        Debug.Log("Intialize Snitch.");
    }
}