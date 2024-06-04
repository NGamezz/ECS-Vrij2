//using UnityEngine;
//using System.Collections.Generic;
//using System.Linq;

//public class ColliderMerger : MonoBehaviour
//{
//    // The size of each cube in the tilemap
//    public Vector3 cubeSize = Vector3.one;

//    // The array of all cubes in the tilemap
//    public GameObject[] cubes;

//    void Start ()
//    {
//        MergeColliders();
//    }

//    void MergeColliders ()
//    {
//        // Create a dictionary to store the positions of cubes
//        Dictionary<Vector3, GameObject> cubeMap = new Dictionary<Vector3, GameObject>();

//        foreach ( var cube in cubes )
//        {
//            Vector3 position = cube.transform.position;
//            cubeMap[position] = cube;
//        }

//        // List to hold the processed cubes
//        List<Vector3> processedPositions = new List<Vector3>();

//        foreach ( var cube in cubes )
//        {
//            Vector3 startPosition = cube.transform.position;

//            if ( processedPositions.Contains(startPosition) )
//                continue;

//            // Find the extent of the contiguous group in the x, y, and z directions
//            Vector3 extent = FindExtent(cubeMap, startPosition, processedPositions);

//            // Create a new collider encompassing the entire group
//            CreateMergedCollider(startPosition, extent);

//            // Mark all cubes in this group as processed
//            MarkProcessed(startPosition, extent, processedPositions);
//        }

//        // Destroy the original colliders
//        foreach ( var cube in cubes )
//        {
//            Destroy(cube.GetComponent<BoxCollider>());
//        }
//    }

//    Vector3 FindExtent ( Dictionary<Vector3, GameObject> cubeMap, Vector3 startPosition, List<Vector3> processedPositions )
//    {
//        Vector3 extent = Vector3.one;

//        Vector3 testPos = startPosition;
//        while ( cubeMap.ContainsKey(testPos + Vector3.right * extent.x) && !processedPositions.Contains(testPos + Vector3.right * extent.x) )
//        {
//            extent.x += 1;
//        }

//        testPos = startPosition;
//        while ( cubeMap.ContainsKey(testPos + Vector3.up * extent.y) && !processedPositions.Contains(testPos + Vector3.up * extent.y) )
//        {
//            extent.y += 1;
//        }

//        testPos = startPosition;
//        while ( cubeMap.ContainsKey(testPos + Vector3.forward * extent.z) && !processedPositions.Contains(testPos + Vector3.forward * extent.z) )
//        {
//            extent.z += 1;
//        }

//        return extent;
//    }

//    void CreateMergedCollider ( Vector3 startPosition, Vector3 extent )
//    {
//        BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();
//        var center = (extent - Vector3.one) / 2.0f;

//        Bounds bounds = newCollider.bounds;
//        startPosition -= (cubeSize / 2.0f);
//        newCollider.center = startPosition;

//        //int count = (int)(math.round(extent / 1) * 1);

//        //bounds.Encapsulate();

//        center.Scale(cubeSize);
//        newCollider.center = startPosition + center;
//        extent.Scale(cubeSize);
//        newCollider.size = extent;
//    }

//    void MarkProcessed ( Vector3 startPosition, Vector3 extent, List<Vector3> processedPositions )
//    {
//        for ( int x = 0; x < extent.x; x++ )
//        {
//            for ( int y = 0; y < extent.y; y++ )
//            {
//                for ( int z = 0; z < extent.z; z++ )
//                {
//                    var size = new Vector3(x, y, z);
//                    size.Scale(cubeSize);
//                    Vector3 pos = startPosition + size;
//                    processedPositions.Add(pos);
//                }
//            }
//        }
//    }
//}

//using UnityEngine;

//public class MergeBoxColliders : MonoBehaviour
//{
//    public GameObject[] cubes; // Array of cube GameObjects
//    public float mergeDistance = 0.1f; // Distance within which BoxColliders will be merged

//    private void Start ()
//    {
//        foreach ( GameObject cube in cubes )
//        {
//            MergeBoxCollider(cube);
//        }
//    }

//    private void MergeBoxCollider ( GameObject cube )
//    {
//        BoxCollider boxCollider = cube.GetComponent<BoxCollider>();
//        if ( !boxCollider )
//        {
//            // If the cube doesn't have a BoxCollider, add one
//            boxCollider = cube.AddComponent<BoxCollider>();
//        }

//        // Create a list to store all colliders that need to be merged
//        List<BoxCollider> collidersToMerge = new List<BoxCollider>();

//        // Start by adding this collider to the list
//        collidersToMerge.Add(boxCollider);

//        // Loop until no more mergers are possible
//        while ( true )
//        {
//            bool mergerFound = false;

//            foreach ( BoxCollider boxCollider in collidersToMerge.ToList() )
//            {
//                // Get all BoxColliders within mergeDistance from this collider
//                Collider[] nearbyColliders = Physics.OverlapBoxNonAlloc(
//                    new Vector3(boxCollider.bounds.center.x - boxCollider.bounds.extents.x,
//                                boxCollider.bounds.center.y - boxCollider.bounds.extents.y,
//                                boxCollider.bounds.center.z - boxCollider.bounds.extents.z),
//                    new Vector3(2 * boxCollider.bounds.extents.x, 2 * boxCollider.bounds.extents.y, 2 * boxCollider.bounds.extents.z),
//                    out Collider[] hits);

//                // Filter the results to only include BoxColliders
//                BoxCollider[] nearbyBoxColliders = (from hit in hits where hit is BoxCollider select hit as BoxCollider).ToArray();

//                foreach ( BoxCollider nearbyBoxCollider in nearbyBoxColliders )
//                {
//                    // Check if this collider hasn't been merged yet and is within mergeDistance
//                    if ( !collidersToMerge.Contains(nearbyBoxCollider) && Vector3.Distance(boxCollider.bounds.center, nearbyBoxCollider.bounds.center) <= mergeDistance )
//                    {
//                        collidersToMerge.Add(nearbyBoxCollider);
//                        mergerFound = true;
//                    }
//                }
//            }

//            // If no mergers were found in this iteration, stop
//            if ( !mergerFound )
//                break;
//        }

//        // Create a new BoxCollider with the combined bounds of all merged colliders
//        BoxCollider mergedCollider = cube.AddComponent<BoxCollider>();
//        Bounds combinedBounds = new Bounds();
//        foreach ( BoxCollider boxCollider in collidersToMerge )
//        {
//            combinedBounds.Encapsulate(boxCollider.bounds);
//        }
//        mergedCollider.size = combinedBounds.size;
//        mergedCollider.center = combinedBounds.center;

//        // Remove all other colliders
//        foreach ( BoxCollider boxCollider in collidersToMerge )
//        {
//            Destroy(boxCollider);
//        }
//    }
//}

//using UnityEngine;
//using System.Collections.Generic;

//public class MergeBoxColliders : MonoBehaviour
//{
//    public GameObject[] cubes; // Array of cube GameObjects
//    public float mergeDistance = 0.1f; // Distance within which BoxColliders will be merged

//    private void Start ()
//    {
//        foreach ( GameObject cube in cubes )
//        {
//            MergeBoxCollider(cube);
//        }
//    }

//    private void MergeBoxCollider ( GameObject cube )
//    {
//        BoxCollider boxCollider = cube.GetComponent<BoxCollider>();
//        if ( !boxCollider )
//        {
//            // If the cube doesn't have a BoxCollider, add one
//            boxCollider = cube.AddComponent<BoxCollider>();
//        }

//        List<BoxCollider> collidersToMerge = new List<BoxCollider>();

//        // Start by adding this collider to the list
//        collidersToMerge.Add(boxCollider);

//        while ( true )
//        {
//            bool mergerFound = false;

//            foreach ( BoxCollider boxCollider in collidersToMerge.ToList() )
//            {
//                // Get all BoxColliders within mergeDistance from this collider
//                Collider[] nearbyColliders = Physics.OverlapBoxNonAlloc(
//                    new Vector3(boxCollider.bounds.center.x - boxCollider.bounds.extents.x,
//                                boxCollider.bounds.center.y - boxCollider.bounds.extents.y,
//                                boxCollider.bounds.center.z - boxCollider.bounds.extents.z),
//                    new Vector3(2 * boxCollider.bounds.extents.x, 2 * boxCollider.bounds.extents.y, 2 * boxCollider.bounds.extents.z),
//                    out Collider[] hits);

//                // Filter the results to only include BoxColliders
//                BoxCollider[] nearbyBoxColliders = (from hit in hits where hit is BoxCollider select hit as BoxCollider).ToArray();

//                foreach ( BoxCollider nearbyBoxCollider in nearbyBoxColliders )
//                {
//                    // Check if this collider hasn't been merged yet and is within mergeDistance
//                    if ( !collidersToMerge.Contains(nearbyBoxCollider) && Vector3.Distance(boxCollider.bounds.center, nearbyBoxCollider.bounds.center) <= mergeDistance )
//                    {
//                        collidersToMerge.Add(nearbyBoxCollider);
//                        mergerFound = true;
//                    }
//                }

//                // Find the extent of the contiguous group in the x, y, and z directions
//                Vector3 extent = FindExtent(cube.transform.position, collidersToMerge);

//                // Create a new BoxCollider with the combined bounds of all merged colliders
//                BoxCollider mergedCollider = cube.AddComponent<BoxCollider>();
//                Bounds combinedBounds = new Bounds();
//                foreach ( BoxCollider boxCollider in collidersToMerge )
//                {
//                    combinedBounds.Encapsulate(boxCollider.bounds);
//                }
//                mergedCollider.size = combinedBounds.size;
//                mergedCollider.center = combinedBounds.center;

//                // Mark all cubes in this group as processed
//                MarkProcessed(cube.transform.position, extent, collidersToMerge);

//                // Remove all other colliders
//                foreach ( BoxCollider boxCollider in collidersToMerge )
//                {
//                    Destroy(boxCollider);
//                }
//            }

//            // If no mergers were found in this iteration, stop
//            if ( !mergerFound )
//                break;
//        }
//    }

//    private Vector3 FindExtent ( Vector3 position, List<BoxCollider> collidersToMerge )
//    {
//        int minX = int.MaxValue, maxX = int.MinValue;
//        int minY = int.MaxValue, maxY = int.MinValue;
//        int minZ = int.MaxValue, maxZ = int.MinValue;

//        foreach ( BoxCollider boxCollider in collidersToMerge )
//        {
//            Vector3 center = boxCollider.transform.position + boxCollider.bounds.center;
//            minX = Mathf.Min(minX, (int)center.x);
//            maxX = Mathf.Max(maxX, (int)center.x);
//            minY = Mathf.Min(minY, (int)center.y);
//            maxY = Mathf.Max(maxY, (int)center.y);
//            minZ = Mathf.Min(minZ, (int)center.z);
//            maxZ = Mathf.Max(maxZ, (int)center.z);
//        }

//        return new Vector3((float)maxX - (float)minX, (float)maxY - (float)minY, (float)maxZ - (float)minZ);
//    }

//    private void MarkProcessed ( Vector3 position, Vector3 extent, List<BoxCollider> collidersToMerge )
//    {
//        // This method is not implemented in the provided code. It should mark all cubes in this group as processed.
//        // The implementation depends on your specific use case and requirements.
//    }
//}