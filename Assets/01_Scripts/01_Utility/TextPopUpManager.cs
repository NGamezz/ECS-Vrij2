using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public struct TextPopup
{
    public float Duration;
    public string Text;

    public TextPopup ( float duration, string text )
    {
        Duration = duration;
        Text = text;
    }
}

public class TextPopUpManager : MonoBehaviour
{
    [SerializeField] private TMP_Text popupTextObject;

    private readonly Queue<TextPopup> textPopupQueue = new();

    private Coroutine currentPopupRoutine = null;

    public void QueuePopup ( TextPopup popup )
    {
        textPopupQueue.Enqueue(popup);
    }

    private void Dequeue ()
    {
        currentPopupRoutine ??= StartCoroutine(PlayPopup(textPopupQueue.Dequeue()));
    }

    private void OnDisable ()
    {
        EventManagerGeneric<TextPopup>.RemoveListener(EventType.OnTextPopupQueue, QueuePopup);
    }

    private void OnEnable ()
    {
        EventManagerGeneric<TextPopup>.AddListener(EventType.OnTextPopupQueue, QueuePopup);
    }

    private IEnumerator PlayPopup ( TextPopup popup )
    {
        popupTextObject.gameObject.SetActive(true);
        popupTextObject.text = popup.Text;

        yield return Utility.Yielders.Get(popup.Duration);

        popupTextObject.gameObject.SetActive(false);
        popupTextObject.text = "";

        currentPopupRoutine = null;
    }

    private void FixedUpdate ()
    {
        if ( textPopupQueue.Count > 0 )
            Dequeue();
    }
}
