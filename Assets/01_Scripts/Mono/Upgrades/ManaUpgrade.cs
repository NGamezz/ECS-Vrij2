using UnityEngine;

public class ManaUpgrade : MonoBehaviour, IUpgrade
{
    private int increaseAmount = 5;

    private void OnTriggerEnter ( Collider other )
    {
        var root = other.transform.root;

        IUpgradable upgradable;
        if ( root == other.transform )
        {
            upgradable = (IUpgradable)other.GetComponent(typeof(IUpgradable));
        }
        else
        {
            upgradable = (IUpgradable)other.GetComponentInParent(typeof(IUpgradable));
        }

        if ( upgradable == null )
            return;

        upgradable.Upgrade(this);
        Destroy(gameObject);
    }

    public void Add ( object ctx )
    {
        if ( ctx is not CharacterData data )
            return;

        data.currentGun.MagSize += increaseAmount;
    }

    public void Remove ( object ctx )
    {
        if ( ctx is not CharacterData data )
            return;

        data.currentGun.MagSize += increaseAmount;
    }
}