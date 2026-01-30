using System.Collections;
using UnityEngine;

public class RealTimeSync : MonoBehaviour
{
    public DataManager dataManager;
    public MarkerSpawner markerSpawner;
    
    public float updateInterval = 10f;
    public bool enableLiveUpdates = true;

    void Start()
    {
        if (dataManager == null || markerSpawner == null)
        {
            Debug.LogError("RealTimeSync: DataManager or MarkerSpawner reference is missing!");
            return;
        }
        
        StartCoroutine(AutoRefresh());
    }

    IEnumerator AutoRefresh()
    {
        while (true)
        {
            if (enableLiveUpdates)
            {
                // Trigger fetch
                dataManager.FetchNow();

                // Wait for fetch to complete
                yield return new WaitUntil(() => dataManager.isFetching == false);

                // Update markers
                markerSpawner.SpawnMarkers();

                // Wait interval
                yield return new WaitForSeconds(updateInterval);
            }
            else
            {
                // If paused, just wait a frame before checking again
                yield return null;
            }
        }
    }
}
