using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EnableTeleportManager : MonoBehaviour
{
    [SerializeField] private InputAction teleporterActivationAction;

    [SerializeField] private float radius = 7.0f;

    [SerializeField] private UnityEvent OnTeleport;

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
        Debug.Log("Activate Portals.");

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
                var teleporter = teleports[index];

                playerMeshTransform.position = teleporter.GetSpawnPosition();
                teleporter.Teleport();

                return;
            }
        }
    }

    private void OnDisable ()
    {
        teleporterActivationAction.performed -= CheckActivation;
        teleporterActivationAction.Disable();
    }
}