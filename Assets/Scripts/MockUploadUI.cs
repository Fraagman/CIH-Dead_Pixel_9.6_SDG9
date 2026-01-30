using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MockUploadUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject uploadPanel;
    public Slider progressBar;
    public Text statusText; // Legacy Text, or use TMPro if preferred

    [Header("Settings")]
    public float uploadDuration = 1.5f;

    void Start()
    {
        if (uploadPanel != null) uploadPanel.SetActive(false);
    }

    [ContextMenu("Test Upload")]
    public void TriggerUploadSequence()
    {
        StartCoroutine(UploadRoutine());
    }

    IEnumerator UploadRoutine()
    {
        if (uploadPanel == null) yield break;

        uploadPanel.SetActive(true);
        if (progressBar != null) progressBar.value = 0;
        if (statusText != null) statusText.text = "Uploading to Cloud...";

        float timer = 0f;
        while (timer < uploadDuration)
        {
            timer += Time.deltaTime;
            if (progressBar != null)
                progressBar.value = Mathf.Lerp(0f, 1f, timer / uploadDuration);
            
            yield return null;
        }

        if (progressBar != null) progressBar.value = 1f;
        if (statusText != null) statusText.text = "Success!";

        yield return new WaitForSeconds(0.5f);

        uploadPanel.SetActive(false);
    }
}
