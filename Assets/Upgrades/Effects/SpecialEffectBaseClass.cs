using UnityEngine;

public abstract class SpecialEffectBaseClass : MonoBehaviour
{
    private void Awake()
    {
        ExecuteEffect();
    }

    public abstract void ExecuteEffect();
    
    public virtual void EndEffect()
    {
        StopAllCoroutines();
        Destroy(this);
    }
    
    public virtual void Refresh()
    {

    }
}