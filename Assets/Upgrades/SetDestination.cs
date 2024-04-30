using UnityEngine;

using UnityEngine.AI;
public class SetDestination : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private NavMeshAgent agent;

    void Start ()
    {
        agent.SetDestination(target.transform.position);
    }

    private void FixedUpdate ()
    {
        if ( !agent.hasPath )
            agent.SetDestination(target.transform.position);
    }
}