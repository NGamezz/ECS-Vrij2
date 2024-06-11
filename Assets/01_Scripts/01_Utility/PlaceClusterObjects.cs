using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class PlaceClusterObjects : MonoBehaviour
{
    [SerializeField] private GameObject[] clusterPrefabs;
    [SerializeField] private int2 clusterItemRange = new(3, 6);

    [SerializeField] private int2 clusterDensityRange = new(1, 3);

    [SerializeField] private float avoidanceRadius = 1.0f;
    [SerializeField] private int groundLayer = 3;

    [SerializeField] private int2 placeBounds = new(-50, 50);

    [SerializeField] private int amountToPlace = 15;

    Vector3 ownPos;

    [Button]
    private void PlaceClusters ()
    {
        ownPos = transform.position;

        int clusterCount = UnityEngine.Random.Range(clusterItemRange.x, clusterItemRange.y);
        int currentClusterCount = 0;

        Vector3 randomPos = Vector3.zero;

        for ( int i = 0; i < amountToPlace; ++i )
        {
            var gameObject = Instantiate(clusterPrefabs[UnityEngine.Random.Range(0, clusterPrefabs.Length)], transform);

            if ( currentClusterCount == 0 )
            {
                randomPos = GetRandomPos();
                gameObject.transform.position = randomPos;
            }
            else
            {
                randomPos = gameObject.transform.position = GetRandomPosition(randomPos);
            }

            currentClusterCount++;

            if ( currentClusterCount >= clusterCount )
            {
                clusterCount = UnityEngine.Random.Range(clusterItemRange.x, clusterItemRange.y);
                currentClusterCount = 0;
            }
        }
    }

    private Vector3 GetRandomPosition ( Vector3 refPos , int index = 0, int maxTries = 10)
    {
        var pos = refPos + UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(clusterDensityRange.x, clusterDensityRange.y);
        pos.y = ownPos.y;

        if(!ValidPlace(pos) && index < maxTries)
        {
            return GetRandomPosition(refPos, index + 1, maxTries);
        }

        return pos;
    }

    private Vector3 GetRandomPos ( int index = 0, int maxTries = 10 )
    {
        var pos = new Vector3(UnityEngine.Random.Range(placeBounds.x, placeBounds.y), ownPos.y, UnityEngine.Random.Range(placeBounds.x, placeBounds.y));

        if ( !ValidPlace(pos) && index < maxTries )
        {
            return GetRandomPos(index + 1, maxTries);
        }

        return pos;
    }

    private bool ValidPlace ( Vector3 position )
    {
        return Physics.CheckSphere(position, avoidanceRadius, 1 << groundLayer) && !Physics.CheckSphere(position, avoidanceRadius, ~(1 << groundLayer));
    }
}