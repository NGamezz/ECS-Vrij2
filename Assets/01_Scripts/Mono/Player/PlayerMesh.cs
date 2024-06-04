using UnityEngine;

public class PlayerMesh : MonoBehaviour, IPlayerMesh
{
    private Transform _transform;

    private void Awake ()
    {
        _transform = transform;
    }

    public Transform GetTransform ()
    {
        return _transform;
    }
}