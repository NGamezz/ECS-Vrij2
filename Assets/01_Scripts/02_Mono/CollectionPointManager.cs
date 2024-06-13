using UnityEngine;
using UnityEngine.Events;

public class CollectionPointManager : MonoBehaviour
{
    [SerializeField] private int amountOfCompletedPoints = 0;
    [SerializeField] private int requiredAmountOfCompletedPoints = 3;

    [SerializeField] private UnityEvent UponCompletionOfPoints;

    private void OnEnable ()
    {
        EventManager.AddListener(EventType.UponDesiredSoulsAmount, ActivateCompletion, this);
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
}