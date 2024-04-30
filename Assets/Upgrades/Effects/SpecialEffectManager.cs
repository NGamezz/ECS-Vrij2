using System.Collections.Generic;
using UnityEngine;

public class SpecialEffectManager : MonoBehaviour /*ISpecialEffecable*/
{
    public void TakeSpecialEffects(List<SpecialEffectBaseClass> effects, GameObject target)
    {
        foreach (SpecialEffectBaseClass effect in effects)
        {
            SpecialEffectBaseClass component = (SpecialEffectBaseClass)GetComponent(effect.GetType());
            if (component != null)
            {
                component.Refresh();
            }
            else
            {
                target.AddComponent(effect.GetType());
            }
        }
    }
}