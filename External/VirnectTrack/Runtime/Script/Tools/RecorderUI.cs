// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using System;
using System.IO;
using UnityEngine;

namespace VIRNECT {
namespace TOOLS {
/// <summary>
/// Provides a debug UI to access the Recorder functionality
/// </summary>
[ExecuteAlways]
public class RecorderUI : ToolUIHelper
{
    /// <summary>
    /// Root path on desktop platforms
    /// </summary>
    private static string editorRootPathFolderName = "../Recordings/";

    /// <summary>
    /// Root path on android platforms
    /// </summary>
    private static string androidRootPathPrefix = "/sdcard/";

    /// <summary>
    /// Root path on android platforms
    /// </summary>
    private static string androidRootPathFolderName = "TRACK_recordings/";

    [Header("Configuration")]
    /// <summary>
    /// Root path on mobile platforms
    /// </summary>
    public string editorRootPath = editorRootPathFolderName;

    /// <summary>
    /// Root path on mobile platforms
    /// </summary>
    public string androidRootPath = androidRootPathPrefix + androidRootPathFolderName;

    /// <summary>
    /// If the standard recording format is PGM or JPG
    /// </summary>
    public bool recordPGMFormat = false;

    /// <summary>
    /// Fraction of button size for splitting Record and Format button
    /// </summary>
    private float fraction = 0.7f;

    /// <summary>
    /// Sanitize paths
    /// </summary>
    void OnValidate()
    {
        if (Uri.IsWellFormedUriString(editorRootPath, UriKind.RelativeOrAbsolute))
            editorRootPath = editorRootPathFolderName;

        if (androidRootPath.Length == 0)
            androidRootPath = androidRootPathPrefix + androidRootPathFolderName;

        if (!androidRootPath.StartsWith(androidRootPathPrefix))
            androidRootPath = androidRootPathPrefix + androidRootPath;
    }

    /// <summary>
    /// Draw debug buttons for recorder interface
    /// </summary>
    public void OnGUI()
    {
        Layout();

        bool recordingIsActive = TrackManager.IsActive() && LibraryInterface.IsRecording();

        GUI.color = recordingIsActive && Convert.ToInt32(Time.time) % 2 == 0 ? Color.red : Color.white;
        GUI.skin.button.fontSize = fontSize;

        // Toggle recording button
        if (GUI.Button(new Rect(x, y, width * fraction, height), recordingIsActive ? "Stop Recording " : "Start Recording"))
        {
            Debug.LogError("RECORDING PRESSED");
            if (recordingIsActive)
            {
                if (TrackManager.Instance.IsRunning)
                    LibraryInterface.StopRecording();
            }
            else
            {
                try
                {
                    string verifyedRootPath = "../";

#if (UNITY_EDITOR || UNITY_STANDALONE)
                    verifyedRootPath = Path.GetFullPath(editorRootPath);
#elif (UNITY_ANDROID)
                    verifyedRootPath = androidRootPath;
#endif
                    if (TrackManager.Instance.IsRunning)
                        LibraryInterface.StartRecording(verifyedRootPath, recordPGMFormat);
                    else
                        Debug.LogError("Cannot start recording: TRACK framework not running.");
                } catch (Exception e)
                {
                    Debug.LogError("Cannot start recording: " + e.Message);
                }
            }
        }

        // Disable format toggle during recording
        if (recordingIsActive)
            GUI.enabled = false;

        // Provide interface to change format during runtime
        if (GUI.Button(new Rect(x + width * fraction, y, width * (1 - fraction), height), (recordPGMFormat ? "PGM" : "JPG")))
        {
            recordPGMFormat = !recordPGMFormat;
        }
    }
}
}
}
