using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public Dictionary<Guid, UpgradeBaseClass> currentUpgrades = new();

    //public PlayerStatManager playerStatManager;
    //public BulletTypeManager bulletTypeManager;
 
    private void Start()
    {
        //playerStatManager = FindObjectOfType<PlayerStatManager>();
        //bulletTypeManager = FindObjectOfType<BulletTypeManager>();
    }

    public void AddUpgrade(UpgradeBaseClass upgrade)
    {
        currentUpgrades.Add(upgrade.Id, upgrade);
        upgrade.Setup(this);
        upgrade.OnEquip();
    }

    public void RemoveUpgrade(UpgradeBaseClass upgrade)
    {
        if (currentUpgrades.ContainsKey(upgrade.Id))
        {
            UpgradeBaseClass equipedUpgrade = currentUpgrades[upgrade.Id];
            equipedUpgrade.OnDequip();
            Destroy(equipedUpgrade.gameObject);
            currentUpgrades.Remove(equipedUpgrade.Id);
        }
    }
}