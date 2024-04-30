using UnityEngine;

public class SpecialBulletEffectUpgrade : UpgradeBaseClass
{
    public SpecialEffectBaseClass effect;

    public override void OnDequip()
    {
        //BulletSpawnComunicator.OnBulletSpawn -= ApplyEffect;
    }

    public override void OnEquip()
    {
        //BulletSpawnComunicator.OnBulletSpawn += ApplyEffect;
    }

    public void ApplyEffect(GunStats stats)
    {
        //stats.AddSpecialEffect(effect);
    }
}
