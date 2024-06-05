using System.Threading.Tasks;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable, ICharacterDataHolder
{
    public bool Dead => false;

    [SerializeField] private CharacterData data;

    public void AfflictDamage ( float amount )
    {
        if ( data.shieldActive )
        {
            ResetShield(data);
            EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, "Shield Used"));
            return;
        }

        if ( !data.canTakeDamage )
            return;

        data.Health -= amount;

        if ( data.Health <= 0 )
        {
            gameObject.SetActive(false);
        }

        data.OnHit?.Invoke();
    }

    private async void ResetShield ( CharacterData data )
    {
        data.shieldActive = false;
        await Task.Delay(System.TimeSpan.FromSeconds(data.shieldRechargeSpeed));

        if ( !data.hasShield )
            return;

        data.shieldActive = true;

    }

    public void SetCharacterData ( CharacterData characterData )
    {
        data = characterData;
    }
}