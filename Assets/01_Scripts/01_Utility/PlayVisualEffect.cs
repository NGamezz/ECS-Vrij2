using UnityEngine;
using UnityEngine.VFX;

public class PlayVisualEffect : MonoBehaviour
{
    [SerializeField] private VisualEffect visualEffect;

    public void Activate()
    {
        if ( visualEffect == null )
            return;

        visualEffect.Play();
    }
}