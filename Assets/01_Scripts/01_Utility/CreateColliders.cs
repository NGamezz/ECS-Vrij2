using NaughtyAttributes;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;

public class CreateColliders : MonoBehaviour
{
    [SerializeField] GameObject[] objects;

    private struct CalculationData
    {
        public bool hasCollider;
        public GameObject gameObject;

        public int2 direction;

        public CalculationData ( bool hasCollider, GameObject gameObject, int2 direction )
        {
            this.hasCollider = hasCollider;
            this.gameObject = gameObject;
            this.direction = direction;
        }
    }

    [Button]
    private void GenerateBoxColliders ()
    {
        Debug.Log("Start Generating Colliders");

        CombineInstance[] combineInstance = new CombineInstance[objects.Length];
        CalculationData[] calculationDatas = new CalculationData[objects.Length];

        for ( int i = 0; i < objects.Length; ++i )
        {
            var meshFilter = (MeshFilter)objects[i].GetComponent(typeof(MeshFilter));

            calculationDatas[i] = new(false, objects[i], new());

            combineInstance[i].mesh = meshFilter.sharedMesh;
            combineInstance[i].transform = objects[i].transform.localToWorldMatrix;
            objects[i].SetActive(false);
        }

        Mesh mesh = new();
        mesh.CombineMeshes(combineInstance);
        transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        transform.gameObject.SetActive(true);

        Physics.BakeMesh(mesh.GetInstanceID(), false);

        var collider = transform.GetComponent<MeshCollider>();
        collider.sharedMesh = mesh;

        var navSurface = (NavMeshSurface)GetComponent(typeof(NavMeshSurface));
        navSurface.BuildNavMesh();

        Debug.Log("Finish Generating Colliders");
    }

    private void SetColliders ( CalculationData[] datas )
    {
        var gameObject = new GameObject();

        var collider = gameObject.AddComponent<BoxCollider>();

        for ( int i = 0; i < datas.Length; ++i )
        {
            collider.bounds.Encapsulate(datas[i].gameObject.transform.position);
        }
    }
}

public enum AdjacentObjects
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4,
}