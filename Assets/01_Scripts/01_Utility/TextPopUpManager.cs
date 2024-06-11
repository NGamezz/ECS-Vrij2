using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
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

    private readonly ConcurrentQueue<TextPopup> textPopupQueue = new();

    private int count = 0;
    private bool running = false;

    public void QueuePopup ( TextPopup popup )
    {
        Interlocked.Increment(ref count);
        textPopupQueue.Enqueue(popup);
    }

    private void Dequeue ()
    {
        if ( !running )
        {
            var succes = textPopupQueue.TryDequeue(out var popup);
            if ( succes )
                PlayPopup(popup).Forget();
        }
    }

    private void OnDisable ()
    {
        EventManagerGeneric<TextPopup>.RemoveListener(EventType.OnTextPopupQueue, QueuePopup);
        textPopupQueue.Clear();
    }

    private void OnEnable ()
    {
        EventManagerGeneric<TextPopup>.AddListener(EventType.OnTextPopupQueue, QueuePopup);
    }

    private async UniTaskVoid PlayPopup ( TextPopup popup )
    {
        running = true;

        popupTextObject.gameObject.SetActive(true);
        popupTextObject.text = popup.Text;

        await UniTask.Delay(TimeSpan.FromSeconds(popup.Duration));

        popupTextObject.gameObject.SetActive(false);
        popupTextObject.text = "";

        Interlocked.Decrement(ref count);
        running = false;
    }

    private void FixedUpdate ()
    {
        if ( count > 0 )
            Dequeue();
    }
}
