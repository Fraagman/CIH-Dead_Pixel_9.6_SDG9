using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class DatabaseTester : MonoBehaviour
{
    [Header("Supabase Settings")]
    public string supabaseUrl;
    public string supabaseAnonKey;

    [ContextMenu("Send Test Report")]
    public void SendTestReport()
    {
        StartCoroutine(PostRequest());
    }

    IEnumerator PostRequest()
    {
        if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseAnonKey))
        {
            Debug.LogError("DatabaseTester: URL or Key is missing!");
            yield break;
        }

        string url = supabaseUrl + "/rest/v1/reports";
        string jsonPayload = "{\"type\": \"Test\", \"lat\": 21.1458, \"lng\": 79.0882, \"status\": \"pending\"}";

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            // Headers
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("apikey", supabaseAnonKey);
            www.SetRequestHeader("Authorization", "Bearer " + supabaseAnonKey);

            Debug.Log($"Sending test report to {url}...");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"DatabaseTester Error: {www.error} - {www.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"DatabaseTester Success: {www.responseCode} - {www.downloadHandler.text}");
            }
        }
    }
}
