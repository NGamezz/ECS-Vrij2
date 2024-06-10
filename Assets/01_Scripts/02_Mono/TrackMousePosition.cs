using UnityEngine;

public class TrackMousePosition : MonoBehaviour
{
    [SerializeField] private CharacterData data;

    private Camera mainCamera;
    private Plane plane = new(Vector3.up, 0);

    private void FixedUpdate ()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if ( !plane.Raycast(ray, out float distance) )
        {
            return;
        }
    }

    void Start ()
    {
        mainCamera = Camera.main;
    }
}