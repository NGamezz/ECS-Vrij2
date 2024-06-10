using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnableTeleportManager : MonoBehaviour
{
    [SerializeField] private InputAction teleporterActivationAction;

    [SerializeField] private float radius = 5.0f;

    private EnableTeleport[] teleports;
    private Vector3[] teleporterPositions;

    private Transform playerMeshTransform;

    private void Start ()
    {
        teleports = GetComponentsInChildren<EnableTeleport>();
        teleporterPositions = new Vector3[teleports.Length];
        playerMeshTransform = FindAnyObjectByType<PlayerMesh>().GetTransform();
    }

    public void EnableChildTeleporters ()
    {
        for ( int i = 0; i < teleports.Length; ++i )
        {
            teleporterPositions[i] = teleports[i].transform.position;
        }

        teleporterActivationAction.performed += CheckActivation;
        teleporterActivationAction.Enable();
    }

    private void CheckActivation ( InputAction.CallbackContext ctx )
    {
        for ( int i = 0; i < teleporterPositions.Length; i++ )
        {
            if ( Vector3.Distance(playerMeshTransform.position, teleporterPositions[i]) < radius )
            {
                int index = (i + 1) % teleports.Length;

                playerMeshTransform.position = teleports[index].GetSpawnPosition();
                return;
            }
        }
    }

    private void OnDisable ()
    {
        teleporterActivationAction.performed -= CheckActivation;
        teleporterActivationAction.Disable();
    }

    private void OnDrawGizmos ()
    {
        if ( !EditorApplication.isPlaying )
            return;

        foreach ( var teleporter in teleports )
        {
            Gizmos.DrawWireSphere(teleporter.transform.position, radius);
        }
    }
}