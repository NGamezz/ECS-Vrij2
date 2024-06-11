using Cysharp.Threading.Tasks;
using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class SpawnFloatingDamage : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float lifeTime = 1;
    [SerializeField] private int2 spawnRange = new(1, 3);

    public void Activate ( float amount, Transform target )
    {
        var random = UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(spawnRange.x, spawnRange.y);
        Debug.Log(target.position);

        var pos = target.position + random;
        pos.y += 5.0f;

        Debug.Log(pos);

        var instance = Instantiate(prefab, target);
        instance.transform.position = pos;

        var text = instance.GetComponent<TMP_Text>();

        text.text = amount.ToString();
        instance.transform.forward = target.forward;

        DestroyAfterTime(instance).Forget();
    }

    private async UniTaskVoid DestroyAfterTime ( GameObject target )
    {
        await UniTask.Delay(TimeSpan.FromSeconds(lifeTime));
        Destroy(target);
    }
}