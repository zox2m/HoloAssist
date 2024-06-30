// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace VIRNECT {
/// <summary>
/// The BuildSanitizer runs before a build is executed
/// It verifies the ProjectSettings and (if defined) also all scenes included in the build
/// </summary>
public class BuildSanitizer : IPreprocessBuildWithReport
{
    /// <summary>
    /// Name to display in error messages
    /// </summary>
    private const string productName = Constants.productNameLog;

    /// <summary>
    /// Execute this prebuild process first
    /// </summary>
    public int callbackOrder
    {
        get {
            return 0;
        }
    }

    /// <summary>
    /// Called before the build starts
    /// Any Debug.LogError call will intercept the build process
    /// </summary>
    /// <param name="report">Current build report for information</param>
    public void OnPreprocessBuild(BuildReport report)
    {
        // Check general project setup
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
        case BuildTarget.Android: CheckBuildSettingsAndroid(); break;
        case BuildTarget.StandaloneWindows: Debug.LogError(productName + " does not support x86 architecture. Please choose x86_64 architecture"); break;
        case BuildTarget.StandaloneWindows64: CheckBuildSettingsWindowsAndLinux(); break;
        case BuildTarget.StandaloneLinux64: CheckBuildSettingsWindowsAndLinux(); break;
        default: Debug.LogError(productName + " does not support \"+EditorUserBuildSettings.activeBuildTarget+\" platform"); break;
        }

        // Check settings
        if (TrackSettingsEditor.HasSettings())
        {
            // Check all scenes if set
            if (TrackSettingsEditor.GetSettings().PerformPreBuildSceneCheck)
                CheckBuildScenes();
        }
    }

    /// <summary>
    /// Ensures correct project settings for Windows
    /// - 64 bit
    /// - IL2CPP
    /// - Graphics API = OpenGLCore​
    /// </summary>
    /// <returns>If the settings are ok</returns>
    public static bool CheckBuildSettingsWindowsAndLinux()
    {
        bool isOk = true;

        // Check graphics settings
        GraphicsDeviceType[] graphicSettings = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
        if (graphicSettings.Length != 1 || graphicSettings[0] != GraphicsDeviceType.OpenGLCore)
        {
            Debug.LogError(productName + " requires Graphics API OpenGLCore only");
            isOk = false;
        }

        // Scripting backend IL2CPP ​
        if (PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup) != ScriptingImplementation.IL2CPP)
        {
            Debug.LogError(productName + " requires IL2CPP scripting backend");
            isOk = false;
        }

        return isOk;
    }

    /// <summary>
    /// Ensures correct project settings for Android
    /// - Minimum API Level = SDK 24 / Android 7.0
    /// - Scripting backend = IL2CPP
    /// - C++ Compiler Configuration = Release
    /// - Target Architecture = ARM64
    /// - Graphics API = OpenGLES3​
    /// - ForceInternetPermission = true
    /// - forceSDCardPermission = true
    /// </summary>
    private void CheckBuildSettingsAndroid()
    {
        // Min SDK version 24 / Android 7.0
        if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24)
            Debug.LogError(productName + ": Minimum Android SDK version needs to be " + AndroidSdkVersions.AndroidApiLevel24.ToString());

        // Scripting backend IL2CPP ​
        if (PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup) != ScriptingImplementation.IL2CPP)
            Debug.LogError(productName + " requires IL2CPP scripting backend");

        // C++ Compiler Configuration set to Release
        if (PlayerSettings.GetIl2CppCompilerConfiguration(BuildTargetGroup.Android) != Il2CppCompilerConfiguration.Release)
            Debug.LogError(productName + " requires C++ Compiler Configuration to be Release");

        // Target Architectures ​ARM64 only
        if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
            Debug.LogError(productName + " requires Android Target Architecture to be ARM64 only");

        // Graphics API to OpenGLES3​
        GraphicsDeviceType[] graphicSettings = PlayerSettings.GetGraphicsAPIs(EditorUserBuildSettings.activeBuildTarget);
        if (graphicSettings.Length != 1 || graphicSettings[0] != GraphicsDeviceType.OpenGLES3)
            Debug.LogError(productName + " requires Graphics API OpenGLES3 only");

        // Force InternetPermission
        if (!PlayerSettings.Android.forceInternetPermission)
            Debug.LogError(productName + " requires Internet permission to verify the license key");

        // Force FileAccessPermission
        if (!PlayerSettings.Android.forceSDCardPermission)
            Debug.LogError(productName + " requires SDCardPermission to access target data");
    }

    /// <summary>
    /// Loads every scene in the editor and runs the scene sanitizer
    /// </summary>
    private void CheckBuildScenes()
    {
        // Preserve opened scene
        Scene scene = SceneManager.GetActiveScene();

        // Check if scene is saved
        if (scene.isDirty)
        {
            if (EditorUtility.DisplayDialog(
                    "Save changes",
                    "All scene changes need to be saved for the VIRNECT TRACK Prebuild check to work.\n\nNote: This check can be disabled in the Track Settings (VIRNECT Track | Track Settings)",
                    "Save Scenes", "Abort Build"))
                EditorSceneManager.SaveOpenScenes();
            else
            {
                Debug.LogError(productName + ": All scenes need to be saved to run VIRNECT TRACK Prebuild check. Build aborted by user.");
                return;
            }
        }

        string currentScenePath = null;
        if (scene != null)
            currentScenePath = scene.path;

        // Create empty list to count all used targets
        List<string> usedTargets = new List<string>();

        // Check currently opened scene that might not be in build configuration
        if (!SceneSanitizer.VerifyCorrectSceneSetup(SceneManager.GetActiveScene(), scene.path, usedTargets))
            Debug.LogError(productName + ": Scene \"" + scene.path + "\" is not configured correctly");

        // Check all build enabled scenes in the build configuration
        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            // Only check enabled scenes
            if (buildScene.enabled)
            {
                // Open scene in editor to load all cross references
                EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);

                if (!SceneSanitizer.VerifyCorrectSceneSetup(SceneManager.GetActiveScene(), buildScene.path, usedTargets))
                    Debug.LogError(productName + ": Scene \"" + buildScene.path + "\" is not configured correctly");
            }
        }

        // Identify unused targets:
        List<string> allTargets = TargetManagerWindow.GetTargetNames();

        // Check target usage
        if (allTargets.Count > usedTargets.Count)
        {
            string unused = "";
            foreach (string targetName in allTargets)
                if (!usedTargets.Contains(targetName))
                    unused = unused + targetName + "\n";
            Debug.LogWarning(productName + ": The following targets are unused in you current build configuration:\n" + unused + "Please consider removing them to reduce the application size.");
        }

        // Restore opened scene
        EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
    }
}

}
