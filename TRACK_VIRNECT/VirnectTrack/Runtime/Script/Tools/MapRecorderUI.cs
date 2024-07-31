// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace VIRNECT {
namespace TOOLS {
/// <summary>
/// Helper UI to provide UI for saving map recording
/// </summary>
[ExecuteAlways]
public class MapRecorderUI : ToolUIHelper
{
    /// <summary>
    /// Display GUI button
    /// </summary>
    public void OnGUI()
    {
        Layout();

        GUI.skin.button.fontSize = fontSize;
        if (GUI.Button(new Rect(x, y, width, height), "Save Map Recording"))
            SaveMap();
    }

    /// <summary>
    /// Public function to call
    /// </summary>
    public void SaveMap()
    {
        bool success = LibraryInterface.SaveMapTarget();
        string msg = "Successfully saved map target";

        if (!success)
            msg = "An error occurred during saving of the MAP target";

        Debug.Log(msg);
        NotificationUI.DisplayNotification("Map Target Recording", msg, !success);
    }

    /// <summary>
    /// Scene sanitizer verification
    /// MapPointVisualizer needs to be child of target
    /// </summary>
    /// <param name="scene">Scene to check</param>
    /// <returns>Success or failure of verifying setup</returns>
    public static bool VerifySetup(Scene scene)
    {
        foreach (MapRecorderUI recorderUI in Resources.FindObjectsOfTypeAll<MapRecorderUI>())
            if (recorderUI.gameObject.scene == scene)
            {
                foreach (MapRecorder recorder in Resources.FindObjectsOfTypeAll<MapRecorder>())
                    if (recorder.gameObject.scene == scene)
                        return true;

                Debug.LogError("MapRecorderUI requires a MapRecorder component attached to the TrackManager");
                return false;
            }

        return true;
    }
}
}
}
