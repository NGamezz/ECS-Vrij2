using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Range(0f, 2f)]
    public float intensity;
    
    Transform Target;
    Vector3 initialPos;


    
    void Start()
    {
        Target = GetComponent<Transform>();
        initialPos = Target.localPosition;        
    }

    float pendingShakeDuration = 0f;

    public void Shake(float duration)
    {
        if (duration > 0)
        {
            pendingShakeDuration += duration;
        }
    }

    bool isShakin = false;

     void Update()
    {
        if (pendingShakeDuration > 0 && !isShakin)
        {
            StartCoroutine(doShake());
        }
    }
    IEnumerator doShake()
    {
        isShakin = true;

        var startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < startTime + pendingShakeDuration)
        {
            var randomPoint = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), initialPos.z);
            Target.localPosition = randomPoint;
            yield return null;
        }

        pendingShakeDuration = 0f;
        Target.localPosition = initialPos;
        isShakin = false;
    }

    public void ShakeWrapper()
    {
        Shake(1);
    }
}
