using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class BuildTools
{
    [MenuItem("TrafficCity/Build Android APK")]
    public static void BuildAndroid()
    {
        // 1. Configure Player Settings
        PlayerSettings.companyName = "Fraagman";
        PlayerSettings.productName = "CityWatchAR";
        PlayerSettings.bundleVersion = "1.0";
        
        // Android Specifics
        EditorUserBuildSettings.buildAppBundle = false; // We want APK for local testing
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29; // Android 10
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64; // Modern phones
        
        // Graphics APIs - Remove Vulkan (Use OpenGLES3)
        // Check if Vulkan is present and remove it if so, to avoid crashing ARCore on some devices
        var graphics = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
        // Note: setting manually to OpenGLES3 is safer for compatibility
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] { 
            UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 
        });

        // 2. Build Options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/SampleScene.unity" }; // Assuming main scene is SampleScene (User should verify)
        // If file exists, update name
        // We look for enabled scenes in Build Settings
        if (EditorBuildSettings.scenes.Length > 0)
        {
             System.Collections.Generic.List<string> enabledScenes = new System.Collections.Generic.List<string>();
             foreach(var scene in EditorBuildSettings.scenes)
             {
                 if (scene.enabled) enabledScenes.Add(scene.path);
             }
             buildPlayerOptions.scenes = enabledScenes.ToArray();
        }

        buildPlayerOptions.locationPathName = "Builds/CityWatchAR.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        Debug.Log("🔨 Configuring and Building Android APK...");

        // 3. Execute Build
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"✅ Build Succeeded: {summary.totalSize / 1024 / 1024} MB");
            EditorUtility.RevealInFinder(buildPlayerOptions.locationPathName);
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError($"❌ Build Failed: {summary.totalErrors} errors.");
        }
    }
}
