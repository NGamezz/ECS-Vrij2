using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake ()
    {
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
        Application.targetFrameRate = Mathf.CeilToInt((float)Screen.currentResolution.refreshRateRatio.value);
    }

    private void OnDisable ()
    {
        WorldManager.ClearAllEvents();
    }
}