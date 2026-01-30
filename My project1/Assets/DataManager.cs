using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

public class DataManager : MonoBehaviour
{
    [System.Serializable]
    public class ReportData
    {
        public string id;
        public string type;
        public float lat;
        public float lng;
        public string status;
    }

    [System.Serializable]
    private class ReportWrapper
    {
        public ReportData[] items;
    }

    public List<ReportData> reports = new List<ReportData>();
    public bool isFetching = false;
    
    [Header("Events")]
    public UnityEvent<bool> OnFetchStatus; // True = Success, False = Fail

    [Header("Configuration")]
    public string supabaseUrl = "https://gozytubnvbnofbraophc.supabase.co";
    public string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imdvenl0dWJudmJub2ZicmFvcGhjIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Njk3NjIxNjYsImV4cCI6MjA4NTMzODE2Nn0.kWMFN6M3gr4OKZsQI47e0jPCebPoN-W67-Vi0AMgbCM";

    [ContextMenu("Fetch Now")]
    public void FetchNow()
    {
        if (!isFetching)
            StartCoroutine(FetchReports());
    }

    IEnumerator FetchReports()
    {
        isFetching = true;
        string url = supabaseUrl + "/rest/v1/reports";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("apikey", supabaseKey);
            www.SetRequestHeader("Authorization", "Bearer " + supabaseKey);
            
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error fetching reports: {www.error}");
                OnFetchStatus?.Invoke(false);
            }
            else
            {
                string json = www.downloadHandler.text;

                // Unity's JsonUtility cannot parse top-level arrays directly.
                // We wrap the JSON in an object if it starts with '['.
                if (json.Trim().StartsWith("["))
                {
                    json = "{\"items\":" + json + "}";
                }

                try
                {
                    ReportWrapper wrapper = JsonUtility.FromJson<ReportWrapper>(json);
                    if (wrapper != null && wrapper.items != null)
                    {
                        reports = new List<ReportData>(wrapper.items);
                        Debug.Log($"Successfully fetched {reports.Count} reports.");
                        OnFetchStatus?.Invoke(true);
                    }
                    else
                    {
                        Debug.LogWarning("Parsed JSON but found no items or format was incorrect.");
                        OnFetchStatus?.Invoke(false);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"JSON Parse Error: {e.Message}");
                    OnFetchStatus?.Invoke(false);
                }
            }
        }
        isFetching = false;
    }

    public void PostReport(string type, float lat, float lng, string status)
    {
        StartCoroutine(SendPostRequest(type, lat, lng, status));
    }

    IEnumerator SendPostRequest(string type, float lat, float lng, string status)
    {
        string url = supabaseUrl + "/rest/v1/reports";
        // Simple JSON construction to avoid a whole new class for one-off send
        string jsonPayload = $"{{\"type\": \"{type}\", \"lat\": {lat}, \"lng\": {lng}, \"status\": \"{status}\"}}";

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("apikey", supabaseKey);
            www.SetRequestHeader("Authorization", "Bearer " + supabaseKey);

            // Debug.Log($"Posting new report to {url}...");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"PostReport Error: {www.error} - {www.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"PostReport Success: {www.responseCode} - {www.downloadHandler.text}");
                // Optional: Auto-fetch to show new item immediately
                // FetchNow(); 
            }
        }
    }
}
