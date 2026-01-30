using System.Collections;
using UnityEngine;

public class TestConnection : MonoBehaviour
{
    public DataManager dataManager;

    // Data from the curl command to verify
    private string expectedType = "Pothole";
    private float expectedLat = 21.1458f;
    private float expectedLng = 79.0882f;

    IEnumerator Start()
    {
        if (dataManager == null)
        {
            Debug.LogError("TestConnection: DataManager reference is missing!");
            yield break;
        }

        Debug.Log("TestConnection: Starting fetch...");
        dataManager.FetchNow();

        // Wait until fetching is complete
        yield return new WaitUntil(() => dataManager.isFetching == false);

        bool found = false;
        if (dataManager.reports != null)
        {
            foreach (var report in dataManager.reports)
            {
                // Check if this report matches the one sent via terminal
                // Using approximate float comparison for lat/lng
                if (report.type == expectedType && 
                    Mathf.Approximately(report.lat, expectedLat) && 
                    Mathf.Approximately(report.lng, expectedLng))
                {
                    found = true;
                    break;
                }
            }
        }

        if (found)
        {
            Debug.Log("INTEGRATION SUCCESS: Data reached Unity!");
        }
        else
        {
            Debug.LogError("INTEGRATION FAILED: Data is in DB but Unity cannot see it.");
            Debug.Log($"Checked {dataManager.reports.Count} reports.");
        }
    }
}
