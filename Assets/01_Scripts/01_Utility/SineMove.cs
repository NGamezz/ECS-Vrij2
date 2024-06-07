using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public enum Axis
{
    X = 0,
    Y = 1,
    Z = 2,
}

public class SineMove : MonoBehaviour
{
    [SerializeField] private float sineStrength = 0.5f;
    [SerializeField] private float frequency = 4.0f;

    private Vector3 cachedPosition = Vector3.zero;

    private Transform cachedTransform;
    private GameObject cachedGameObject;

    private void Start ()
    {
        cachedPosition = transform.position;
        cachedGameObject = gameObject;
        cachedTransform = transform;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Animate ()
    {
        var position = cachedTransform.position;
        position.y = cachedPosition.y + math.sin(Time.time * frequency) * sineStrength;
        cachedTransform.position = position;
    }

    private void FixedUpdate ()
    {
        if ( cachedGameObject.activeInHierarchy )
            Animate();
    }
}