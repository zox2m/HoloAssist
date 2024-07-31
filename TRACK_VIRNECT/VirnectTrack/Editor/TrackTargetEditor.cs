// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace VIRNECT {
/// <summary>
/// The custom editor window for TrackTarget
/// Provides a responsive interface, informing the developer about wrong target configuration
/// </summary>
[CustomEditor(typeof(TrackTarget))]
public class TrackTargetEditor : Editor
{
    // References to serialized fields
    SerializedProperty nameField;
    SerializedProperty hideField;
    SerializedProperty ignoreField;
    SerializedProperty staticField;
    SerializedProperty onStartTrackingEventField;
    SerializedProperty onStoppedTrackingEventField;

    List<string> selectables = new List<string>(); // Possible target names to select
    int index = -1;                                // Selected dropdown index
    TrackTarget duplicate = null;                  // If a GameObject with the same setting is present in the scene

    bool foldoutEventsFields = true;  // used to toggle the state of the foldout menu of the events

    /// <summary>
    /// Prepare connection to serializedObject and perform initial checks before showing UI
    /// </summary>
    void OnEnable()
    {
        // Load data reference to entity
        nameField = serializedObject.FindProperty("targetName");
        hideField = serializedObject.FindProperty("hidePlaceholder");
        ignoreField = serializedObject.FindProperty("ignoreForTracking");
        staticField = serializedObject.FindProperty("isTargetStatic");
        onStartTrackingEventField = serializedObject.FindProperty("onTrackingStarted");
        onStoppedTrackingEventField = serializedObject.FindProperty("onTrackingStopped");

        // Get selectable target names
        selectables = TargetManagerWindow.GetTargetNames();

        // Determine selected index of target-name drop down
        index = selectables.IndexOf(nameField.stringValue);

        // Check state of selection
        duplicate = CheckForDuplicate((TrackTarget)serializedObject.targetObject);
    }

    /// <summary>
    /// Provides visual feedback about the target selection
    /// Shows errors or warnings if miss configured
    /// Checks settings before persisting value
    /// </summary>
    public override void OnInspectorGUI()
    {
        GUILayout.BeginVertical();

        // Check if target data path exists and provide UI to fix path error
        string path = Application.dataPath + Constants.targetRootDirectory;
        if (!Directory.Exists(path))
        {
            GUI.backgroundColor = Color.red;
            EditorGUILayout.HelpBox("Path\"" + path + "\" does not exist!", MessageType.Error, true);
            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Create missing directory", EditorStyles.miniButton))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
            }
            GUILayout.EndVertical();
            return;
        }

        // Check scene target state:
        //
        string errorMessage = TargetManagerWindow.VerifySceneSetup(((TrackTarget)serializedObject.targetObject).gameObject.scene);

        // Show general error if set
        if (errorMessage != null)
        {
            GUI.backgroundColor = Color.red;
            EditorGUILayout.HelpBox(errorMessage, MessageType.Error, true);
            GUI.backgroundColor = Color.white;
        }

        // Disable GUI if nothing to select
        GUI.enabled = selectables.Count > 0;

        if (nameField.stringValue.Length == 0 && index == -1)
        {
            // No name selected warning
            GUI.backgroundColor = Color.yellow;
            EditorGUILayout.HelpBox("There is no target selected. Please select a target from the list below.", MessageType.Warning, true);
        }
        else
        {
            // Check selected target state:
            errorMessage = VerifyTargetNameSelection((TrackTarget)serializedObject.targetObject, duplicate);

            // Show selection specific error if set
            if (errorMessage != null)
            {
                GUI.backgroundColor = Color.red;
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error, true);
            }
        }

        // Validation finished
        // Display UI

        // Display DropDown
        // If changed, only update index, set real name value after duplicate check
        int indexNew = EditorGUILayout.Popup("Target", index, selectables.Cast<string>().ToArray());
        if (indexNew != index)
        {
            string selectedTargetName = (string)selectables[indexNew];
            // Check for duplicates
            duplicate = CheckForDuplicate((TrackTarget)serializedObject.targetObject, selectedTargetName);

            // Update if no duplicate detected
            if (duplicate == null)
                UpdateName(selectedTargetName);

            index = indexNew;
        }
#if UNITY_IOS
        EditorGUILayout.HelpBox("Track iOS only supports Image/QR/Shape type", MessageType.Info, true);
#endif
        // Reset interface color after drop down
        GUI.backgroundColor = Color.white;

        // Other fields
        bool hide = EditorGUILayout.Toggle("Hide target visualization", hideField.boolValue);
        bool ignore = EditorGUILayout.Toggle("Ignore target during tracking", ignoreField.boolValue);
        bool targetStatic = EditorGUILayout.Toggle("Is static in scene", staticField.boolValue);

        // Add listeners
        EditorGUILayout.Space();
        GUILayout.EndVertical();

        // Apply values
        serializedObject.Update();

        // The events have to be applied here, otherwise the + and - button do not work
        if (foldoutEventsFields = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutEventsFields, "Tracking Events"))
        {
            EditorGUILayout.PropertyField(onStartTrackingEventField, true);
            EditorGUILayout.PropertyField(onStoppedTrackingEventField, true);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        hideField.boolValue = hide;
        ignoreField.boolValue = ignore;
        staticField.boolValue = targetStatic;
        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Searches all scene targets for duplicates
    /// </summary>
    /// <param name="unique">TrackTarget that should be unique</param>
    /// <param name="pendingName">Selected name to check for, overrides targetName of unique parameter</param>
    /// <returns>TrackTarget with same targetName if found or null</returns>
    private static TrackTarget CheckForDuplicate(TrackTarget unique, string pendingName = null)
    {
        if (unique == null)
            return unique;

        // Set own name to compare if no name is given
        string targetNameToCheck = pendingName;
        if (targetNameToCheck == null)
            targetNameToCheck = unique.targetName;

        foreach (TrackTarget target in TargetManagerWindow.GetAllSceneTargets(unique.gameObject.scene))
        {
            // Escape self reference
            if (target == unique)
                continue;

            // Detect usage in same scene
            if (target.targetName == targetNameToCheck)
                return target;
        }
        return null;
    }

    /// <summary>
    /// Persist DropDown selection
    /// </summary>
    /// <param name="name">Selected name</param>
    private void UpdateName(string name)
    {
        serializedObject.Update();
        nameField.stringValue = name;
        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Verify the correct setup of the target
    /// </summary>
    /// <param name="target">Target to check</param>
    /// <param name="duplicate">Override duplicate entity for duplicate detection. Needed for local single target check</param>
    /// <returns>Error string if error detected</returns>
    public static string VerifyTargetNameSelection(TrackTarget target, TrackTarget duplicate = null)
    {
        // Check if duplicated selection is present in the same scene as the target
        TrackTarget duplicateToCheck = duplicate;

        // Search duplicate if not given (used for build sanitation)
        if (duplicateToCheck == null)
            duplicateToCheck = CheckForDuplicate(target);

        // Return message if duplicate detected
        if (duplicateToCheck != null)
            return "Target name \"" + duplicateToCheck.targetName + "\" is already used by \"" + duplicateToCheck.name + "\". Selection is not saved. Please select another target.";

        // Check if target is assigned
        if (target.targetName.Length == 0)
            return target.gameObject.name + " has no target selected";

        // Check if assigned target exists
        if (TargetManagerWindow.GetTargetNames().Find(item => item.Equals(target.targetName)) == null)
            return "Selected Target \"" + target.targetName + "\" no longer exists in \"" + Constants.targetRootDirectory + "\"";

        // All checks passed
        return null;
    }
}
}