using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeDequipPickUp : MonoBehaviour, IPickup
{
    [SerializeField]
    private UpgradeBaseClass upgrade;

    public event Action OnPickup;

    private UpgradeManager manager;

    private void Start()
    {
        manager = FindAnyObjectByType<UpgradeManager>();
    }

    public void PickUp(GameObject obj)
    {
        manager.RemoveUpgrade(upgrade);
        Destroy(gameObject);
    }
}
