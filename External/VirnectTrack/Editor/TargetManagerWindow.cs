// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VIRNECT {
/// <summary>
/// New window for setting Target manager
/// </summary>
public class TargetManagerWindow : EditorWindow
{
    // List of folder names without "Track_" prefix
    private static List<string> TargetNames = new List<string>();

    // An error message to be displayed in the UI
    private static string UIErrorMessage = "";

    /// <summary>
    /// Provides logic to open the window via Menu item. MenuItem currently disabled
    /// </summary>
    //[MenuItem(EditorCommons.menuRoot + "/Target Manager")]
    public static void ShowWindow()
    {
        var window = GetWindow<TargetManagerWindow>();
        window.titleContent = new GUIContent("Target Manager");
        window.minSize = new Vector2(100, 100);
    }

    /// <summary>
    /// Loads the targets when the UI is shown
    /// </summary>
    private void OnEnable() { UpdateTargetList(); }

    /// <summary>
    /// Dynamically load target names form resources directory
    /// </summary>
    private static void UpdateTargetList()
    {
        TargetNames.Clear();

        string path = Application.dataPath + Constants.targetRootDirectory;
        if (!Directory.Exists(path))
        {
            Debug.LogError(Constants.productNameLog + ": Path \"" + path + "\" does not exist");
            return;
        }

        // Placeholder for potential errors
        string errorList = "";

        foreach (string file in Directory.GetFiles(path))
        {
            string fullFileName = file.Substring(file.LastIndexOf(Path.AltDirectorySeparatorChar) + 1);

            // Verify target name
            if (fullFileName.EndsWith(Constants.targetFileExtension))
                TargetNames.Add(fullFileName.Substring(0, fullFileName.Length - Constants.targetFileExtension.Length));
            else
                errorList += fullFileName + "\n";
        }

        if (errorList.Length != 0)
            UIErrorMessage = "An error appeared during target loading. The following file could not be identified as target:\n" + errorList;
        else
            UIErrorMessage = "";
    }

    /// <summary>
    /// Public method to get all available target names of the resources
    /// </summary>
    /// <returns>List of selectable Target names</returns>
    public static List<string> GetTargetNames()
    {
        UpdateTargetList();
        return TargetNames;
    }

    /// <summary>
    /// Rudimentary layout for target manager: currently used for debugging only
    /// </summary>
    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Target Manager:", EditorStyles.largeLabel);

        foreach (string name in TargetNames)
            GUILayout.Label(" - " + name);

        if (GUILayout.Button("Update"))
            UpdateTargetList();

        if (UIErrorMessage.Length != 0)
        {
            GUI.backgroundColor = Color.red;
            EditorGUILayout.HelpBox(UIErrorMessage, MessageType.Error, true);
        }

        GUILayout.EndVertical();
    }

    /// <summary>
    /// Helper method to get all targets used in a scene
    /// </summary>
    /// <param name="scene">Scene to search</param>
    /// <returns>Dictionary of used targets in scene</returns>
    public static List<TrackTarget> GetAllSceneTargets(Scene scene)
    {
        List<TrackTarget> targets = new List<TrackTarget>();
        TrackTarget[] targetObjects = Resources.FindObjectsOfTypeAll<TrackTarget>();
        foreach (TrackTarget target in targetObjects)
            if (target.gameObject.scene == scene)
                targets.Add(target);

        return targets;
    }

    /// <summary>
    /// Verify the correct setup of a scene
    /// </summary>
    /// <param name="scene">Scene to check</param>
    /// <returns>Error string if error detected</returns>
    public static string VerifySceneSetup(Scene scene)
    {
        // No targets
        if (GetTargetNames().Count == 0)
            return "There are no targets to select. Please add targets to \"" + Constants.targetRootDirectory + "\".";

        // To much targets
        if (GetAllSceneTargets(scene).Count > Constants.maxTargets)
            return "The maximum number of targets per scene (" + Constants.maxTargets + ") is reached.";

        // All checks passed
        return null;
    }
}

}
