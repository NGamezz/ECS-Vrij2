using Cysharp.Threading.Tasks;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Range(1f, 50f)] public float intensity;

    Transform Target;
    Vector3 initialPos;

    [SerializeField] private float timeBetweenShake = 0.1f;

    void Start ()
    {
        Target = GetComponent<Transform>();
        initialPos = Target.localPosition;

        Shake(10);
    }

    float pendingShakeDuration = 0f;

    public void Shake ( float duration )
    {
        if ( duration < 1 )
            return;

        pendingShakeDuration += duration;

        if ( !isShaking )
        {
            PerformShake().Forget();
        }
    }

    bool isShaking = false;

    private async UniTaskVoid PerformShake ()
    {
        isShaking = true;

        float t = 0;
        while ( t < pendingShakeDuration )
        {
            var random = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(-intensity, intensity);
            var pos = new Vector3(random.x, UnityEngine.Random.Range(-intensity, intensity), random.y);

            Target.localPosition += pos * Time.deltaTime;
            t += Time.deltaTime;
            
            await UniTask.Delay(TimeSpan.FromSeconds(timeBetweenShake));
        }

        pendingShakeDuration = 0f;
        Target.localPosition = initialPos;

        isShaking = false;
    }

    public void ShakeWrapper ()
    {
        Shake(1);
    }
}
