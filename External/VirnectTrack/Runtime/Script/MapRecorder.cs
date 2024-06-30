// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VIRNECT {
/// <summary>
/// Script to configure MAP recording parameters.
/// Needs to be places next to TrackManager.
/// Values need to be set before starting the TrackManager
/// </summary>
[RequireComponent(typeof(TrackManager))]
public class MapRecorder : MonoBehaviour
{
    /// <summary>
    /// Reference to initializing image target
    /// </summary>
    [Tooltip("Use an Image Target to initialize MAP recording")]
    public TrackTarget InitializationTarget;

    /// <summary>
    /// Target name of newly recorded map
    /// </summary>
    public string NewMapTargetName = "Recorded_Map";

    /// <summary>
    /// Path to save recorded map target. Privileges required.
    /// </summary>
    [Tooltip("Path for native platforms")]
    public string SavePathNative = "../MapTargets";

    /// <summary>
    /// Option to toggle in between APP-internal folder or external SDcard
    /// </summary>
    [Tooltip("Select false to save map targets in SavePathAndroid")]
    public bool SaveAppInternally = true;

    /// <summary>
    /// Path to save recorded map target on user accessible sdcard. Privileges required.
    /// </summary>
    public string SavePathAndroid = "/sdcard/Documents/MapTargets/";

    /// <summary>
    /// Provides the platform dependent string
    /// </summary>
    /// <returns>Map Target save path for current platform</returns>
    public string GetPlatformDependentSavePath()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (SaveAppInternally)
            return Application.persistentDataPath + Constants.mapTargetRootDirectoryAndroid;
        else
            return SavePathAndroid;
#endif
        return SavePathNative;
    }

    /// <summary>
    /// Scene sanitizer verification
    /// MapRecorder needs to be next to track manager
    /// </summary>
    /// <param name="scene">Scene to analyze</param>
    /// <returns>Success or failure of verifying setup</returns>
    public static bool VerifySetup(Scene scene)
    {
        foreach (MapRecorder recorder in Resources.FindObjectsOfTypeAll<MapRecorder>())
            if (recorder.gameObject.scene == scene)
            {
                if (recorder.GetComponent<TrackManager>() == null)
                {
                    Debug.LogError("MapRecorder needs to be placed next to TrackManager component");
                    return false;
                }

                if (recorder.InitializationTarget == null)
                {
                    Debug.LogError("MapRecorder needs an initialization target to be set");
                    return false;
                }

                if (String.IsNullOrEmpty(recorder.NewMapTargetName))
                {
                    Debug.LogError("MapRecorder needs an new target name to be set");
                    return false;
                }

                if (String.IsNullOrEmpty(recorder.SavePathNative))
                {
                    Debug.LogError("MapRecorder needs a save path");
                    return false;
                }

                if (!recorder.SaveAppInternally && String.IsNullOrEmpty(recorder.SavePathAndroid))
                {
                    Debug.LogError("MapRecorder needs an save path for Android");
                    return false;
                }
            }
        return true;
    }
}
}