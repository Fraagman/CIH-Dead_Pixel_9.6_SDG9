using UnityEngine;
using UnityEngine.UI;

public class StatusIndicator : MonoBehaviour
{
    public DataManager dataManager;
    public Image statusImage;
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    public Color pendingColor = Color.yellow;

    void Start()
    {
        if (dataManager != null)
        {
            dataManager.OnFetchStatus.AddListener(UpdateStatus);
        }
        
        if (statusImage != null)
        {
            statusImage.color = pendingColor; // Start as yellow/pending
        }
    }

    void OnDestroy()
    {
        if (dataManager != null)
        {
            dataManager.OnFetchStatus.RemoveListener(UpdateStatus);
        }
    }

    void UpdateStatus(bool success)
    {
        if (statusImage == null) return;
        
        statusImage.color = success ? successColor : failColor;
    }
}
