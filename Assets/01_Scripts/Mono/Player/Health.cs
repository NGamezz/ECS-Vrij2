using UnityEngine;

public class Health : MonoBehaviour, IDamageable, ICharacterDataHolder
{
    public bool Dead => false;

    [SerializeField] private CharacterData data;

    public void AfflictDamage ( float amount )
    {
        Debug.Log(amount);

        data.Health -= amount;

        if ( data.Health <= 0 )
        {
            Debug.Log("You Died.");
            gameObject.SetActive(false);
        }
    }

    public void SetCharacterData ( CharacterData characterData )
    {
        data = characterData;
    }
}