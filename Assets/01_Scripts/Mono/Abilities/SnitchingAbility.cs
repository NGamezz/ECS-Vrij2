using System;
using UnityEngine;

public class SnitchingAbility : Ability
{
    public override bool Execute ( object context )
    {
        return false;
        //Debug.Log("Snitch Ability");
    }

    public override void Initialize ( IAbilityOwner owner, CharacterData context )
    {
        Debug.Log("Intialize Snitch.");
    }
}