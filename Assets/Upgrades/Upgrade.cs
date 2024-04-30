using UnityEngine;
using System;

public abstract class UpgradeBaseClass : MonoBehaviour
{
    public Guid Id { get; private set;}

    protected UpgradeManager upgradeManager;

    public  void Setup(UpgradeManager upgradeManager)
    {
        this.upgradeManager = upgradeManager;
    }

    public abstract void OnEquip();
    
    public abstract void OnDequip();

    [ContextMenu("GenerateId")]
    public void GenerateId()
    {
        Debug.Log("generated Id");
        Id = Guid.NewGuid();
    }
}