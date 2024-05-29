using NaughtyAttributes;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CreateColliders : MonoBehaviour
{
    [SerializeField] GameObject[] objects;

    private List<GameObject> generatedColliderObjects;

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

    private void MergeColliders ( params CalculationData[] data )
    {
        List<CalculationData> currentBatchToCalculate = new();

        for ( int i = 0; i < data.Length; ++i )
        {
            var currentNode = data[i];

            currentBatchToCalculate.Add(currentNode);

            int count = 0;

            if ( data[++i].hasCollider )
            {
                SetColliders(currentBatchToCalculate.ToArray());
                continue;
            }


            for ( int z = 0; z < data.Length; z++ )
            {



            }


            for ( int z = -2; z < 2; ++z )
            {
                for ( int x = -2; x < 2; ++x )
                {

                }
            }
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