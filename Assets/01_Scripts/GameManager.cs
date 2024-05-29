using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start ()
    {
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif

        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = Mathf.CeilToInt((float)Screen.currentResolution.refreshRateRatio.value);

        //For Testing Purposes.
        //Task.Run(() =>
        //{
        //    void action ()
        //    {
        //        transform.position = transform.position;
        //    }

        //    try
        //    {
        //        action();
        //    }
        //    catch ( UnityException e )
        //    {
        //        Debug.Log($"Caught UnityException, attempting on main thread. Exception : {e.Message}");

        //        MainThreadQueue.Instance.Enqueue(() =>
        //        {
        //            action();
        //        });
        //    }
        //});
    }

    private void OnDisable ()
    {
        WorldManager.ClearAllEvents();
    }
}