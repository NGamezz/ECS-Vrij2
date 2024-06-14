using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour, IDamageable, ICharacterDataHolder
{
    public bool Dead => false;

    [SerializeField] private CharacterData data;

    public Image healthBar;

    public void Heal ( float amount )
    {
        if ( amount < 1 )
            return;

        data.Health += amount;
        healthBar.fillAmount = data.Health / data.MaxHealth;
    }

    public void AfflictDamage ( float amount, bool silent )
    {
        if ( data.shieldActive )
        {
            ResetShield(data).Forget();
            EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, "Shield Used"));
            return;
        }

        if ( !data.canTakeDamage )
            return;

        data.Health -= amount;

        if ( data.Health <= 0 )
        {
            EventManager.InvokeEvent(EventType.GameOver);
        }

        data.OnHit?.Invoke();

        healthBar.fillAmount = data.Health / data.MaxHealth;
    }

    private async UniTaskVoid ResetShield ( CharacterData data )
    {
        data.shieldActive = false;
        await UniTask.Delay(System.TimeSpan.FromSeconds(data.shieldRechargeSpeed));

        if ( !data.hasShield )
            return;

        data.shieldActive = true;
    }

    public void SetCharacterData ( CharacterData characterData )
    {
        data = characterData;
    }
}