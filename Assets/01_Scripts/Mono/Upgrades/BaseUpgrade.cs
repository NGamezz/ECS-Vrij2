using UnityEngine;

public class BaseUpgrade : MonoBehaviour, IUpgrade
{
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

    public virtual void Add ( object ctx )
    {
    }

    public virtual void Remove ( object ctx )
    {
    }
}