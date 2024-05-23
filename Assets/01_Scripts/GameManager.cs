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

        Debug.Log(QualitySettings.vSyncCount);
    }

    private void OnDisable ()
    {
        WorldManager.ClearAllEvents();
    }
}