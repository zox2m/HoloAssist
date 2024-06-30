// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VIRNECT {
/// <summary>
/// SceneSanitizer runs before entering the PlayMode
/// If the configuration is not valid, the PlayMode can not be stared
/// The same verification logic is also used when build checks are executed
/// </summary>
[InitializeOnLoad]
public class SceneSanitizer
{
    /// <summary>
    /// Name to display in error messages
    /// </summary>
    private const string productName = Constants.productNameLog;

    /// <summary>
    /// Static constructor is executed when starting play mode
    /// </summary>
    static SceneSanitizer()
    {
        // Register callback method for PlayMode changes
        EditorApplication.playModeStateChanged += PlayModeInterceptor;
    }

    /// <summary>
    /// PlayModeInterceptor is called by Unity right before entering PlayMode
    /// If scene setup is not configured correctly, starting the PlayMode will be prevented
    /// </summary>
    /// <param name="state">PlayModeStateChange indicates if the PlayMode starts or ends</param>
    private static void PlayModeInterceptor(PlayModeStateChange state)
    {
        // Only continue when entering PlayMode
        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        // Check Settings
        if (!TrackSettingsEditor.HasSettings())
        {
            EditorApplication.isPlaying = false;
            Debug.LogError(productName + ": Entering PlayMode is not possible. No track.settings file.");
        }

        // Check current scene configuration
        Scene scene = SceneManager.GetActiveScene();
        if (!VerifyCorrectSceneSetup(scene, scene.path))
        {
            // If not approved abort starting of PlayMode
            EditorApplication.isPlaying = false;
            Debug.LogError(productName + ": Entering PlayMode is not possible. Scene is not configured correctly");
        }
#if UNITY_EDITOR_WIN
        // Also check necessary player settings for windows (equals build settings)
        if (!BuildSanitizer.CheckBuildSettingsWindowsAndLinux())
        {
            // If not configured correctly abort entering play mode
            EditorApplication.isPlaying = false;
            Debug.LogError(productName + ": Entering PlayMode is not possible. Player settings are not configured correctly");
        }
#endif
    }

    /// <summary>
    /// Checks the configuration of the TargetManager and the Targets inside the scene
    /// </summary>
    /// <param name="scene">Scene to check</param>
    /// <param name="displayName">Scene name to log</param>
    /// <param name="usedTargets">List to append used target names of this scene</param>
    /// <returns>If the scene setup is valid</returns>
    public static bool VerifyCorrectSceneSetup(Scene scene, string displayName, List<string> usedTargets = null)
    {
        // Prefix for each log message
        string prefix = productName + ": Scene \"" + displayName + "\": ";

        // Only check loaded scenes (empty cross references otherwise)
        if (!scene.IsValid())
        {
            Debug.LogError(prefix + "Cannot check scene when not loaded in the Editor");
            return false;
        }
        else
            Debug.Log(prefix + "Performing scene check");

        // Check Track Manager
        bool managerExists = false;
        List<TrackManager> managers = TrackManagerEditor.GetAllSceneManagers(scene);

        // Ensure unique Track Manager GameObject
        if (managers.Count == 1)
        {
            managerExists = true;

            // Check manager setup
            string msg = TrackManagerEditor.VerifyConfiguration(managers[0]);
            if (msg != null)
            {
                Debug.LogError(prefix + "Configuration of \"TrackManager\" is incorrect: " + msg);
                return false;
            }
        }
        else if (managers.Count > 1)
        {
            Debug.LogError(prefix + "Contains more than one \"TrackManager\". Please ensure that only one \"TrackManager\" exists");
            return false;
        }

        // Check scene targets
        List<TrackTarget> targets = TargetManagerWindow.GetAllSceneTargets(scene);

        // Verify that either none (TrackManager and Target) or both exist
        if (targets.Count == 0 && managerExists)
        {
            Debug.LogError(prefix + "Contains a \"TrackManager\" but no \"Target\". Please add a \"Target\" using the \"VIRNECT Track\" menu");
            return false;
        }
        else if (targets.Count != 0 && !managerExists)
        {
            Debug.LogError(prefix + "Contains one or more \"Target\" but no \"TrackManager\". Please add a \"TrackManager\" using the VIRNECT Track menu");
            return false;
        }
        else if (targets.Count == 0 && !managerExists)
        {
            // Scene neither has TrackManager nor any Target
            return true;
        }

        // Check correct configuration of each Target

        // Check general scene condition
        string error = TargetManagerWindow.VerifySceneSetup(scene);
        if (error != null)
        {
            Debug.LogError(prefix + error);
            return false;
        }

        // Check each Target in scene
        foreach (TrackTarget target in targets)
        {

            // Add to global target list:
            if (usedTargets != null)
                if (!usedTargets.Contains(target.targetName))
                    usedTargets.Add(target.targetName);

            // Check configuration
            error = TrackTargetEditor.VerifyTargetNameSelection(target);
            if (error != null)
            {
                Debug.LogError(prefix + error);
                return false;
            }
        }

        // Check configuration of MapPointVisualizers
        if (!MapPointVisualizer.VerifySetup(scene))
            return false;

        // Check configuration of MapPRecorder
        if (!MapRecorder.VerifySetup(scene))
            return false;

        // Check configuration of MapRecorderUI
        if (!TOOLS.MapRecorderUI.VerifySetup(scene))
            return false;

        // All checks passed
        return true;
    }
}

}
