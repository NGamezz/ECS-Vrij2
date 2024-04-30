using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradePickup : MonoBehaviour, IPickup
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
        UpgradeBaseClass currentUpgrade = Instantiate(upgrade, manager.transform.position, Quaternion.identity, manager.gameObject.transform);
        manager.AddUpgrade(currentUpgrade);
        Destroy(gameObject);
    }
}
