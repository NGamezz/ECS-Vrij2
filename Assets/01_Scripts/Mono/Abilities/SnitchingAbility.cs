using UnityEngine;

public class SnitchingAbility : Ability
{
    public override void Execute ( object context )
    {
        //Debug.Log("Snitch Ability");
    }

    public override void Initialize ( IAbilityOwner owner, CharacterData context )
    {
        Debug.Log("Intialize Snitch.");
    }

    public override bool Trigger ()
    {
        return Input.GetKeyDown(KeyCode.V);
    }
}