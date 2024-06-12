using Unity.Mathematics;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs;

    [SerializeField] private int2 spawnRange = new(-10, 10);

    private float t = 0;
    private float increment = 5;

    void Update ()
    {
        t += Time.deltaTime;
        if ( t > increment )
        {
            var gameObject = Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)]);

            var pos = transform.position + new Vector3(UnityEngine.Random.Range(spawnRange.x, spawnRange.y), 0.0f, UnityEngine.Random.Range(spawnRange.x, spawnRange.y));

            if ( !Physics.CheckSphere(pos, 5.0f, (1 << 3)) )
                return;

            gameObject.transform.position = pos;
            t = 0;
        }
    }
}