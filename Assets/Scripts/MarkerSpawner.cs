using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerSpawner : MonoBehaviour
{
    public GameObject markerPrefab;
    public DataManager dataManager;

    [Header("Map Settings")]
    public float originLat;
    public float originLng;
    public float scale = 111319.9f; // Meters per degree roughly at equator

    // Note: For Cesium/ArcGIS, use their specific Georeference components instead of this manual math.

    [ContextMenu("Spawn Markers")]
    public void SpawnMarkers()
    {
        if (dataManager == null)
        {
            Debug.LogError("DataManager reference is missing!");
            return;
        }

        if (markerPrefab == null)
        {
            Debug.LogError("Marker Prefab is missing!");
            return;
        }

        ClearMarkers();

        if (dataManager.reports == null || dataManager.reports.Count == 0)
        {
            Debug.LogWarning("No reports found in DataManager. Try fetching data first.");
            return;
        }

        foreach (var report in dataManager.reports)
        {
            // Simple linear projection math
            float x = (report.lng - originLng) * scale * Mathf.Cos(originLat * Mathf.Deg2Rad);
            float z = (report.lat - originLat) * scale;

            Vector3 position = new Vector3(x, 0, z);

            GameObject marker = Instantiate(markerPrefab, transform);
            marker.transform.localPosition = position;
            marker.name = $"Marker_{report.id}";
        }

        Debug.Log($"Spawned {transform.childCount} markers.");
    }

    [ContextMenu("Clear Markers")]
    public void ClearMarkers()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        Debug.Log("Markers cleared.");
    }
}
