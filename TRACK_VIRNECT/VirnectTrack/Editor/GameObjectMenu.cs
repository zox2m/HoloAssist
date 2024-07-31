// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using UnityEngine;
using UnityEditor;

namespace VIRNECT {

/// <summary>
/// Editor Menu items to add Virnect GameObjects
/// </summary>
public class GameObjectMenu
{
    /// <summary>
    /// Menu option to add and automatically configure a TrackingManager prefab
    /// </summary>
    [MenuItem(EditorCommons.menuRoot + "/" + EditorCommons.menuGameObjects + "/TrackManager", false, 200)]
    static void InstantiateTrackManager()
    {
        // Check if a GameObject with VirnectTrackingFramework already exists
        TrackManager vtf = FindTrackingManager();
        if (vtf)
        {
            // Open Dialog if TrackManager already exists
            if (EditorUtility.DisplayDialog("TrackManager already exists", "Do you want to replace this TrackManager?\nAll customizations will be reset.", "Replace", "Abort"))
                // Replace existing manager, Destroy GameObject
                Object.DestroyImmediate(vtf.gameObject);
            else
                return;
        }

        // Create new TrackingManager with the TrackingManager prefab
        Object frameworkInstance = PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/TrackManager"));
        Selection.activeObject = frameworkInstance;
        EditorUtility.SetDirty(frameworkInstance);

        // Automatically set camera reference
        TrackManager nvtf = ((GameObject)frameworkInstance).GetComponent<TrackManager>();

        nvtf.ARCamera = Camera.main;

        // add TrackCamera component
        if (!nvtf.ARCamera.gameObject.GetComponent<TrackCamera>())
            nvtf.ARCamera.gameObject.AddComponent<TrackCamera>();
    }

    /// <summary>
    /// Recursive function to find any GameObject that has a VirnectTrackingFramework component
    /// </summary>
    /// <param name="root">GameObject to search</param>
    /// <returns>VirnectTrackingFramework component if found</returns>
    static TrackManager FindTrackingManager(GameObject root = null)
    {
        if (!root)
        {
            // Search all root objects in scene:
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (GameObject o in scene.GetRootGameObjects())
            {
                TrackManager rvtf = FindTrackingManager(o);
                if (rvtf)
                    return rvtf;
            }
        }
        else
        {
            // Search all children of this object
            TrackManager gvtf = root.GetComponent<TrackManager>();
            if (gvtf)
                return gvtf;
            else
                foreach (Transform child in root.transform)
                {
                    TrackManager cvtf = FindTrackingManager(child.gameObject);
                    if (cvtf)
                        return cvtf;
                }
        }
        return null;
    }

    /// <summary>
    /// Menu option to add a target prefab
    /// </summary>
    [MenuItem(EditorCommons.menuRoot + "/" + EditorCommons.menuGameObjects + "/Target", false, 201)]
    static void InstantiateTarget() { SpawnTrackPrefab(Constants.targetPrefabPath); }

    /// <summary>
    /// Menu option to add a RecorderUI prefab
    /// </summary>
    [MenuItem(EditorCommons.menuRoot + "/" + EditorCommons.menuTools + "/Recorder UI", false, 301)]
    static void InstantiateRecorderUI() { SpawnTrackPrefab(Constants.recorderPrefabPath); }

    /// <summary>
    /// Menu option to add a ActiveTargetManagerUI
    /// </summary>
    [MenuItem(EditorCommons.menuRoot + "/" + EditorCommons.menuTools + "/Active Target Manager UI", false, 302)]
    static void InstantiateActiveTargetManagerUI() { SpawnTrackPrefab(Constants.activeTargetManagerPrefabPath); }

    /// <summary>
    /// Menu option to add a StaticTargetManagerUI
    /// </summary>
    [MenuItem(EditorCommons.menuRoot + "/" + EditorCommons.menuTools + "/Static Target Manager UI", false, 303)]
    static void InstantiateStaticTargetManagerUI() { SpawnTrackPrefab(Constants.staticTargetManagerPrefabPath); }

    /// <summary>
    /// Menu option to add a Map Target visualization
    /// </summary>
    [MenuItem(EditorCommons.menuRoot + "/" + EditorCommons.menuMap + "/Map Target Visualization", false, 402)]
    static void InstantiateActiveMapRecorder()
    {
        // Check if a target got selected
        GameObject selected = Selection.activeGameObject;

        if (selected == null || selected.GetComponent<TrackTarget>() == null)
        {
            EditorUtility.DisplayDialog("No Track Target selected", "Please select a track target in the scene hierarchy before adding the Map Target Visualization", "OK");
            return;
        }
        else
            SpawnTrackPrefab(Constants.mapVisualizerPrefabPath, Selection.activeGameObject.transform);
    }

    /// <summary>
    /// Menu option to add a Map Recorder Script
    /// </summary>
    [MenuItem(EditorCommons.menuRoot + "/" + EditorCommons.menuMap + "/Map Recorder", false, 402)]
    static void InstantiateMapRecorder()
    {
        // Check existence of map recorder
        TrackManager vtf = FindTrackingManager();
        if (vtf != null && vtf.GetComponent<MapRecorder>() == null)
        {
            vtf.gameObject.AddComponent<MapRecorder>();
            Selection.activeGameObject = vtf.gameObject;
        }
    }

    /// <summary>
    /// Menu option to add a Map Recorder UI
    /// </summary>
    [MenuItem(EditorCommons.menuRoot + "/" + EditorCommons.menuMap + "/Map Recorder UI", false, 403)]
    static void InstantiateMapRecorderUI()
    {
        SpawnTrackPrefab(Constants.mapRecorderPrefabPath);

        // Check existence of map recorder
        TrackManager vtf = FindTrackingManager();
        if (vtf != null && vtf.GetComponent<MapRecorder>() == null)
        {
            // Open Dialog if TrackManager has no map recorder
            if (EditorUtility.DisplayDialog("TrackManager has no MapRecorder", "To use the Map recording function, the MapRecorder script needs to be attached to the TrackManager",
                                            "Attach MapRecorder to TrackManager", "Abort"))
            {
                InstantiateMapRecorder();
            }
        }
    }

    /// <summary>
    /// Unified helper function to spawn a prefab in the scene
    /// </summary>
    /// <param name="resourcePath">Runtime available resource path to requested prefab</param>
    static void SpawnTrackPrefab(string resourcePath, Transform parent = null)
    {
        Object o = PrefabUtility.InstantiatePrefab(Resources.Load(resourcePath), parent);
        Selection.activeObject = o;
        EditorUtility.SetDirty(o);
    }
}
}