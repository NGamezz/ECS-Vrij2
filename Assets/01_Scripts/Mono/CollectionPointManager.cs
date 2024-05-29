using UnityEngine;
using UnityEngine.Events;

public class CollectionPointManager : MonoBehaviour
{
    [SerializeField] private int amountOfCompletedPoints = 0;

    [SerializeField] private int requiredAmountOfCompletedPoints = 3;

    [SerializeField] private UnityEvent UponCompletionOfPoints;

    private CollectionPoint[] collectionPoints;

    private void OnEnable ()
    {
        EventManager.AddListener(EventType.UponDesiredSoulsAmount, ActivateCompletion);
    }

    private void ActivateCompletion ()
    {
        amountOfCompletedPoints++;

        if ( amountOfCompletedPoints < requiredAmountOfCompletedPoints )
            return;

        UponCompletionOfPoints?.Invoke();
        EventManager.InvokeEvent(EventType.PortalActivation);
        EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, "Portal Has Been Unlocked."));
    }

    private void OnDisable ()
    {
        EventManager.RemoveListener(EventType.UponDesiredSoulsAmount, ActivateCompletion);
    }

    private void Start ()
    {
        collectionPoints = FindObjectsByType<CollectionPoint>(FindObjectsSortMode.None);

        foreach ( var point in collectionPoints )
        {
            point.OnStart();
        }
    }
}