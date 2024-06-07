using System;
using UnityEngine;

public class GiveNearbyEntityUpgrade : MonoBehaviour
{
    [SerializeField] private BaseUpgrade upgrade;

    [SerializeField] private int scanRadius = 10;

    [SerializeField] private bool oneTimeUse = true;

    private Collider[] hits = new Collider[25];

    public void Activate ()
    {
        var hitCount = Physics.OverlapSphereNonAlloc(transform.position, scanRadius, hits);

        if ( hitCount < 1 )
        {
            DestroyIfApplicable();
            return;
        }

        var span = hits.AsSpan();
        for ( int i = 0; i < hitCount; ++i )
        {
            IUpgradable upgradable;

            if ( span[i].transform.root == span[i].transform )
                upgradable = (IUpgradable)span[i].GetComponent(typeof(IUpgradable));
            else
                upgradable = (IUpgradable)span[i].GetComponentInParent(typeof(IUpgradable));

            if ( upgradable != null )
            {
                upgradable.Upgrade(upgrade);
                DestroyIfApplicable();
            }
        }

        DestroyIfApplicable();
    }

    private void DestroyIfApplicable ()
    {
        if ( oneTimeUse )
            Destroy(gameObject);
    }
}