// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using UnityEngine;
using UnityEditor;
using System.IO;

namespace VIRNECT {
/// <summary>
/// Editor preference for Track settings
/// </summary>
[CustomEditor(typeof(TrackSettings))]
public class TrackSettingsEditor : Editor
{
#region General

    /// <summary>
    /// Request the track settings of the project
    /// </summary>
    /// <returns>TrackSettings if existent, null otherwise</returns>
    public static TrackSettings GetSettings() { return Resources.Load<TrackSettings>(Constants.settingsPath); }

    /// <summary>
    /// Indicates if the settings file exists
    /// </summary>
    /// <param name="verbose">If a Error log should be logged on an error</param>
    /// <returns>If a settings file exists or not</returns>
    public static bool HasSettings(bool verbose = true)
    {
        TrackSettings settings = GetSettings();
        if (settings != null)
            return true;
        else if (verbose)
            Debug.LogError("There is no settings file in \"" + Constants.settingsPath + "\". Please add settings with via the VIRNET Track menu.");

        return false;
    }

#endregion

#region License Key

    /// <summary>
    /// Create a new TrackSettings ScriptableObject in the resources folder via a menu item
    /// </summary>
    [MenuItem(EditorCommons.menuRoot + "/Track Settings", false, 100)]
    public static void FocusSettingsObject()
    {
        TrackSettings settings = GetSettings();

        // Create new settings asset
        if (settings == null)
        {
            TrackSettings asset = ScriptableObject.CreateInstance<TrackSettings>();
            Directory.CreateDirectory(Path.GetDirectoryName(Constants.settingsAssetFullPath));
            AssetDatabase.CreateAsset(asset, Constants.settingsAssetFullPath);
            AssetDatabase.SaveAssets();
            settings = asset;
        }

        // Focus settings asset in inspector
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = settings;
    }

    /// <summary>
    /// Status messages for info field
    /// </summary>
    private const string validationMessageKeyValid = "License key is valid";
    private const string validationMessageKeyInvalid = "License key is not valid";
    private const string validationMessageKeyNotSet = "No license key set";
    private const string validationMessageKeyPerformingCheck = "Checking license key";

#endregion

#region UI

    // Reference to serialized license key field
    SerializedProperty licenseKey;

    // Reference to UI license key input
    string visibleKey = "";

    // Buffer that survives domain reset, to reduce license manager calls
    string checkedKey = "";

    // Reference to serialized license key field
    SerializedProperty performPreBuildSceneCheck;
    bool visiblePreBuildSceneCheck = true;

    void OnEnable()
    {
        // Load data references
        licenseKey = serializedObject.FindProperty("LicenseKey");
        visibleKey = licenseKey.stringValue;

        performPreBuildSceneCheck = serializedObject.FindProperty("PerformPreBuildSceneCheck");
        visiblePreBuildSceneCheck = performPreBuildSceneCheck.boolValue;

        // Do not access library when game is running
        if (Application.isPlaying)
            return;

        // Check key status
        CheckKey(visibleKey);
    }

    /// <summary>
    /// Calls the license manager only if a new key is entered
    /// </summary>
    /// <param name="key">License key to check</param>
    private void CheckKey(string key)
    {
        // Important check to suppress library calls after domain reset
        if (checkedKey == key)
            return;
        LicenseManager.CheckAPIKey(key);
        checkedKey = key;
    }

    /// <summary>
    /// Called if GUI needs to be refreshed
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Do not allow interaction during playmode
        if (Application.isPlaying)
            GUI.enabled = false;

        GUILayout.BeginVertical();

        // Header
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("TRACK SETTINGS");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        // Section license
        GUILayout.Label("License", EditorStyles.boldLabel);

        // Status Feedback with info box
        string validationMessage = "";
        MessageType helpBoxType = MessageType.None;
        switch (LicenseManager.KeyState)
        {
        case LicenseManager.KeyStates.NotSet:
            validationMessage = validationMessageKeyNotSet;
            GUI.backgroundColor = Color.yellow;
            helpBoxType = MessageType.Warning;
            break;

        case LicenseManager.KeyStates.Valid:
            validationMessage = validationMessageKeyValid;
            GUI.backgroundColor = Color.green;
            helpBoxType = MessageType.Info;
            break;
        case LicenseManager.KeyStates.Invalid:
            validationMessage = validationMessageKeyInvalid;
            GUI.backgroundColor = Color.red;
            helpBoxType = MessageType.Error;
            break;
        case LicenseManager.KeyStates.PerformingCheck:
            validationMessage = validationMessageKeyPerformingCheck;
            GUI.backgroundColor = Color.yellow;
            helpBoxType = MessageType.Warning;
            GUI.enabled = false;
            break;
        }
        EditorGUILayout.HelpBox("  " + validationMessage, helpBoxType, true);
        GUI.backgroundColor = Color.white;

        // Key input field
        visibleKey = EditorGUILayout.TextField("License Key", visibleKey);

        // Set key button
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Set license key", EditorStyles.miniButton))
        {
            // Only update persisted value when button is clicked
            serializedObject.Update();
            licenseKey.stringValue = visibleKey;
            serializedObject.ApplyModifiedProperties();

            // Update UI
            CheckKey(visibleKey);
        }
        GUILayout.EndHorizontal();

        // Section build settings
        GUILayout.Space(5);
        GUILayout.Label("Build Settings:", EditorStyles.boldLabel);

        if (!visiblePreBuildSceneCheck)
            EditorGUILayout.HelpBox("The PreBuild check intercepts the build pipeline and sanitizes every scene defined in the build configuration. It is highly recommended to enable this option.",
                                    MessageType.Info, true);

        visiblePreBuildSceneCheck = GUILayout.Toggle(visiblePreBuildSceneCheck, "Perform PreBuild check for all scenes");
        serializedObject.Update();
        performPreBuildSceneCheck.boolValue = visiblePreBuildSceneCheck;
        serializedObject.ApplyModifiedProperties();

        GUILayout.EndVertical();
    }

#endregion
}

}