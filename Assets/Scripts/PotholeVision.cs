using UnityEngine;
using UnityEngine.UI;

public class PotholeVision : MonoBehaviour
{
    [Header("UI References")]
    public Image reticleImage;

    [Header("Detection Settings")]
    public LayerMask roadLayer;
    public float detectionConfidenceThreshold = 0.8f;
    
    [Header("Status")]
    public bool isPotholeDetected = false;

    [Header("Visual Feedback")]
    public Color scanningColor = Color.white;
    public Color detectingColor = Color.red;
    public Color lockedColor = Color.green;

    void Start()
    {
        if (roadLayer == 0)
        {
             Debug.LogWarning("PotholeVision: Road Layer is not set! Please check the Inspector.");
        }
    }

    void Update()
    {
        if (reticleImage == null) return;

        ScanCamera();
    }

    void ScanCamera()
    {
        // Raycast from the center of the screen
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, roadLayer))
        {
            // Simulate AI confidence check
            float confidence = Random.value;

            if (confidence > detectionConfidenceThreshold)
            {
                // Successful detection
                isPotholeDetected = true;
                reticleImage.color = lockedColor;
                // Debug.Log("AI Vision: Pothole Target Locked!");
            }
            else
            {
                // Detecting but low confidence
                isPotholeDetected = false;
                reticleImage.color = detectingColor;
               // Debug.Log("AI Vision: Analyzing surface...");
            }
        }
        else
        {
            // Not looking at road
            isPotholeDetected = false;
            reticleImage.color = scanningColor;
        }
    }
}
