using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour, IDamageable, ICharacterDataHolder
{
    public bool Dead => false;

    [SerializeField] private CharacterData data;

    public Image healthBar;

    public void AfflictDamage ( float amount )
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
            gameObject.SetActive(false);
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