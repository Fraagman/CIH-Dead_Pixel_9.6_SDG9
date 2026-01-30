using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_XR_HANDS
using UnityEngine.XR.Hands; // Requires com.unity.xr.hands package
using UnityEngine.SubsystemsImplementation.Extensions;
#endif

public class HandGestureController : MonoBehaviour
{
    [Header("References")]
    public PotholeVision potholeVision;
    public DataManager dataManager;
    public MockUploadUI uploadUI;

    [Header("Settings")]

    public float pinchThreshold = 0.02f; // 2cm
    public float gestureCooldown = 2.0f;
    
    private float lastGestureTime;
#if USE_XR_HANDS
    XRHandSubsystem m_HandSubsystem;
#endif

    void Start()
    {
#if USE_XR_HANDS
        GetHandSubsystem();
#endif
    }

    void Update()
    {
        // 1. Editor Fallback
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TrySubmitReport();
        }

        // 2. XR Hand Tracking
#if USE_XR_HANDS
        if (m_HandSubsystem != null && m_HandSubsystem.running)
        {
            // Check Right Hand
            CheckHand(m_HandSubsystem.rightHand);
            // Check Left Hand
            CheckHand(m_HandSubsystem.leftHand);
        }
        else
        {
            // Try to reconnect subsystem if lost
            GetHandSubsystem();
        }
#endif
    }


#if USE_XR_HANDS
    void GetHandSubsystem()
    {
        var subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        if (subsystems.Count > 0)
        {
            m_HandSubsystem = subsystems[0];
        }
    }

    void CheckHand(XRHand hand)
    {
        if (!hand.isTracked) return;

        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);

        if (thumbTip.TryGetPose(out Pose thumbPose) && indexTip.TryGetPose(out Pose indexPose))
        {
            float distance = Vector3.Distance(thumbPose.position, indexPose.position);

            if (distance < pinchThreshold)
            {
                TrySubmitReport(); // Logic Bridge
            }
        }
    }
#endif

    void TrySubmitReport()
    {
        // Cooldown check
        if (Time.time - lastGestureTime < gestureCooldown) return;

        // Vision Bridge Check
        if (potholeVision != null && potholeVision.isPotholeDetected)
        {
             OnGestureSubmit();
             lastGestureTime = Time.time;
        }
    }

    void OnGestureSubmit()
    {
        Debug.Log("Gesture Captured: Sending Report...");
        
        // Mock GPS Data (Use Input.location in real mobile build)
        // 21.1458, 79.0882 is our 'Zero' point, let's add slight random offset to simulate field movement
        float lat = 21.1458f + Random.Range(-0.0001f, 0.0001f);
        float lng = 79.0882f + Random.Range(-0.0001f, 0.0001f);

        if (dataManager != null)
        {
            // Send Data
            dataManager.PostReport("Pothole", lat, lng, "pending");
            Debug.Log($"Bridge Active: Sending to {dataManager.supabaseUrl}/rest/v1/reports");
        }

        // Trigger UI Polish
        if (uploadUI != null)
        {
            uploadUI.TriggerUploadSequence();
        }

        // Visual Feedback
        StartCoroutine(SuccessFeedback());
    }

    IEnumerator SuccessFeedback()
    {
        if (potholeVision != null && potholeVision.reticleImage != null)
        {
            Color original = potholeVision.reticleImage.color;
            potholeVision.reticleImage.color = Color.blue; // Flash Blue
            potholeVision.reticleImage.transform.localScale = Vector3.one * 1.5f; // Expand

            yield return new WaitForSeconds(0.5f);

            potholeVision.reticleImage.color = original;
            potholeVision.reticleImage.transform.localScale = Vector3.one;
        }
    }
}
