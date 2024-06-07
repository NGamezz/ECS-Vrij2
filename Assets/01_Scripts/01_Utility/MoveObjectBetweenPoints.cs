using Cysharp.Threading.Tasks;
using UnityEngine;

public class MoveObjectBetweenPoints : MonoBehaviour
{
    [SerializeField] private Transform[] wayPoints;

    [SerializeField] private float moveSpeed = 2;

    private int currentWaypointIndex = 0;

    private Transform ownTransform;
    private bool movingTowardsTarget = false;

    void Start ()
    {
        ownTransform = transform;
        MoveTowards().Forget();
    }

    private void FixedUpdate ()
    {
        if ( !movingTowardsTarget )
        {
            MoveTowards().Forget();
        }
    }

    private async UniTaskVoid MoveTowards ()
    {
        if ( wayPoints.Length < 1 )
        {
            this.enabled = false;
            return;
        }

        movingTowardsTarget = true;
        while ( Vector3.Distance(ownTransform.position, wayPoints[currentWaypointIndex].position) > 2.0f )
        {
            var direction = wayPoints[currentWaypointIndex].position - ownTransform.position;

            ownTransform.Translate(moveSpeed * Time.deltaTime * direction.normalized);

            await UniTask.NextFrame(cancellationToken: this.GetCancellationTokenOnDestroy());
        }
        movingTowardsTarget = false;

        currentWaypointIndex = (currentWaypointIndex + 1) % wayPoints.Length;
    }
}