// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using System.IO;

namespace VIRNECT {
/// <summary>
/// Main script to manage the configuration and runtime behavior of the tracking framework
/// </summary>
[DefaultExecutionOrder(30000)]
[DisallowMultipleComponent]
public class TrackManager : TrackBehaviour
{
    /// <summary>
    /// Singleton reference
    /// </summary>
    public static TrackManager Instance = null;

    /// <summary>
    /// Reference to main scene camera
    /// </summary>
    public Camera ARCamera;

    /// <summary>
    /// Placeholder for the license key set at runtime
    /// </summary>
    private static string runTimeLicenseKey = "";

    /// <summary>
    /// Method to override the license key of the track setting.
    /// Needs to be set before start is called
    /// </summary>
    /// <param name="key">License Key</param>
    public static void SetRuntimeLicenseKey(string key) { runTimeLicenseKey = key; }

    /// <summary>
    /// Possible image sources to select from
    /// </summary>
    public enum ImageSources
    {
        Camera,
        FileSequence,
        ExternalScript
    }
    ;

    /// <summary>
    /// Selected Image source
    /// </summary>
    public ImageSources imageSource = ImageSources.Camera;

    /// <summary>
    /// USB camera ID
    /// </summary>
    public int cameraID = 0;

    /// <summary>
    /// Camera identifier for calibration JSON
    /// </summary>
    public string calibrationJSONcameraID = "cam0";

    /// <summary>
    /// Resolution identifier for calibration JSON
    /// </summary>
    public string calibrationJSONresolution = "640x480";

    /// <summary>
    /// Path to file sequence
    /// </summary>
    public string fileSequencePath = "";

    /// <summary>
    /// Path to file sequence specific calibration file
    /// </summary>
    public string fileSequenceCalibrationPath = "";

    /// <summary>
    /// GameObject containing ExternalImageSourceInterface component
    /// </summary>
    public GameObject externalImageSourceInterface = null;

    /// <summary>
    /// Indicates if the tracking framework got initialized correctly
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Indicates if the tracking framework is running
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Target names to process
    /// </summary>
    private string[] targetNames = {};

    /// <summary>
    /// List of target GameObjects in the scene
    /// </summary>
    private Dictionary<string, TrackTarget> targets = new Dictionary<string, TrackTarget>();

    /// <summary>
    /// Map recorder script if attached
    /// </summary>
    private MapRecorder mapRecorder;

    /// <summary>
    /// Map target directory to use
    /// </summary>
    private string customTargetDirectory = null;

    /// <summary>
    /// Set the directory
    /// </summary>
    /// <param name="path">Custom target directory</param>
    public void OverrideTargetDirectory(string path) { customTargetDirectory = path; }

    /// Indicates if there is an active initialized and running tracking framework instance
    /// </summary>
    /// <returns>If TRACK framework is active</returns>
    public static bool IsActive()
    {
        if (Instance == null)
            return false;
        return Instance.IsInitialized && Instance.IsRunning;
    }

    /// <summary>
    /// Experimental feature
    /// Indicates if the track framework should be driven by Unity or the Custom Thread controller
    /// </summary>
#if UNITY_EDITOR || UNITY_STANDALONE
    private bool useThreadController = false;
#elif UNITY_ANDROID
    private bool useThreadController = false;

    /// <summary>
    /// Android life-cycle integration
    /// </summary>
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            LibraryInterface.OnResume();
    }

    /// <summary>
    /// Android life-cycle integration
    /// </summary>
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            LibraryInterface.OnPause();
    }

    /// <summary>
    /// Camera permission check in Android platform
    /// </summary>
    /// <returns></returns>
    private IEnumerator PermissionChecking()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);

                yield return new WaitForSeconds(0.2f);
                yield return new WaitUntil(() => Application.isFocused == true);

                if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    NotificationUI.DisplayNotification("Error", "Cannot run TRACK framework without camera permission.", true);
                    yield break;
                }

                if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite) || !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
                {
                    NotificationUI.DisplayNotification("Error", "Cannot run TRACK framework without storage permission.", true);
                    yield break;
                }

                Initialize();
                yield break;
            }
            else
                Initialize();

            yield break;
        }
    }
#endif

    /// <summary>
    /// Start thread controller if enabled
    /// Set application target frame rate
    /// On Android check if permission are granted
    /// Initialize Framework
    /// </summary>
    private void Start()
    {
        // Set singleton reference
        Instance = this;

        if (useThreadController)
            gameObject.AddComponent<AsyncManager>();
        else
            Application.targetFrameRate = Constants.targetFrameRate;

        mapRecorder = GetComponent<MapRecorder>();

#if (UNITY_EDITOR || UNITY_STANDALONE)
        Initialize();
#elif (UNITY_ANDROID)
        StartCoroutine(PermissionChecking());
#endif
    }

    /// <summary>
    /// Initializes TrackManager and native Track framework
    /// </summary>
    private void Initialize()
    {
        Debug.Log("Starting VIRNECT Track v" + LibraryInterface.GetFrameworkVersion());

        // Reset flags
        IsInitialized = false;
        IsRunning = false;

#if UNITY_STANDALONE || UNITY_EDITOR
        // Enable framework logging in debug builds
        if (Debug.isDebugBuild)
            LibraryInterface.EnableDLLDebugLog();
#endif

        // Get all scene targets
        TrackTarget[] targetObjects = Resources.FindObjectsOfTypeAll<TrackTarget>();
        foreach (TrackTarget target in targetObjects)
            if (target.gameObject.scene == gameObject.scene)
                targets.Add(target.targetName, target);

        // Create target name array
        targetNames = new string[targets.Keys.Count];
        targets.Keys.CopyTo(targetNames, 0);

        // Initialize native Framework
        bool frameworkInitialized = InitializeTrackingFramework();
        if (!frameworkInitialized)
        {
            Debug.LogError("Cannot initialize Track framework");
            NotificationUI.DisplayNotification("Initialization Error", "Cannot initialize Track framework", true);
            return;
        }

        // Setup image source
        bool imageSourceInitialized = false;

#if UNITY_EDITOR || UNITY_STANDALONE

        if (imageSource.Equals(ImageSources.Camera))
        {
            int usedID = 0;
            if (cameraID >= 0)
                usedID = cameraID;
            else
                Debug.LogError("A negative camera ID is not allowed");

            // Initialize Camera
            imageSourceInitialized = LibraryInterface.ActivateUSBCamera(cameraID);
            if (!imageSourceInitialized)
                NotificationUI.DisplayNotification("Camera Error", "Could not initialize USB camera connection", true);
        }
        else if (imageSource.Equals(ImageSources.FileSequence))
        {
            // Initialize FileSequenceLoader
            imageSourceInitialized = LibraryInterface.ActivateFileSequence(fileSequencePath);
            if (!imageSourceInitialized)
                NotificationUI.DisplayNotification("File Error", "Could not initialize File Sequence Loader", true);
        }
        else if (imageSource.Equals(ImageSources.ExternalScript))
        {
            ExternalImageSourceInterface extInterface = externalImageSourceInterface.GetComponent<ExternalImageSourceInterface>();
            if (extInterface != null)
                imageSourceInitialized = LibraryInterface.ActivateExternalImageSource(extInterface.GetTexturePointer());
            else
                Debug.LogError("No external image source script attached");
        }

#else
        // Android starts back facing camera with onResume
        LibraryInterface.OnResume();
        imageSourceInitialized = true;
#endif

        IsInitialized = frameworkInitialized && imageSourceInitialized;

        // autostart framework
        IsRunning = IsInitialized;

        // Initialize background rendering
        ARCamera.GetComponent<TrackCamera>().Initialize();
    }

    /// <summary>
    /// Free all resources
    /// </summary>
    void OnDestroy()
    {
        IsRunning = false;
        IsInitialized = false;
        LibraryInterface.Cleanup();
    }

    /// <summary>
    /// Sets up framework configuration and initializes the tracking framework
    /// </summary>
    /// <returns>Success or failure of initialization</returns>
    private bool InitializeTrackingFramework()
    {
        string key = runTimeLicenseKey;
        if (runTimeLicenseKey.Length == 0)
        {
            // Check license key
            TrackSettings keyStore = Resources.Load<TrackSettings>(Constants.settingsPath);
            if (!keyStore)
            {
                Debug.LogError("Cannot initialize framework. No License key provided. Please enter a valid license key");
                return false;
            }
            key = keyStore.LicenseKey;
        }

        if (!LibraryInterface.SetLicenseKey(key))
        {
            // Compose UI notification message:
            string msg = "License key of TRACK framework could not be validated.";

            // Add network message
            if (Application.internetReachability == NetworkReachability.NotReachable)
                msg += "\nNo Internet connectivity.";

            NotificationUI.DisplayNotification("Error", msg, true);
            Debug.LogError("License key invalid. Please enter a valid license key");
            return false;
        }

#if UNITY_STANDALONE || UNITY_EDITOR
        // Load camera calibration from calibration file
        string path = Application.dataPath + Constants.calibrationPath;

        // Override path if file sequence is activated
        if (imageSource.Equals(ImageSources.FileSequence))
            if (fileSequenceCalibrationPath.Length != 0)
            {
                path = fileSequenceCalibrationPath;
                Debug.Log("Using calibration file for File Sequence: " + fileSequenceCalibrationPath);
            }
            else
                Debug.LogWarning("No calibration file for File Sequence provided. Using camera calibration." + path);

        if (File.Exists(path))
        {
            if (!LibraryInterface.LoadCameraCalibration(path, calibrationJSONcameraID, calibrationJSONresolution))
                return false;
        }
        else
            Debug.LogWarning("Camera calibration file \"" + Constants.calibrationPath + "\" does not exist");

        if (ARCamera != null)
        {
            TrackCamera cameraScript = ARCamera.GetComponent<TrackCamera>();
            if (cameraScript != null)
                LibraryInterface.UseAutomaticUSBCameraSetting(cameraScript.autoAdjustCameraSettings);
            else
            {
                Debug.LogError("Camera has no \"VirnectCamera\" script attached");
                return false;
            }
        }
        else
            Debug.LogError("No scene camera attached");
#endif

        string targetDataPath = Application.dataPath + Constants.targetRootDirectory;

#if UNITY_ANDROID && !UNITY_EDITOR
        // Get persisted target data path (copies files if necessary)
        targetDataPath = AndroidTargetHelper.ExtractTargets(targetNames);
#endif

        // Set configuration

        if (!String.IsNullOrEmpty(customTargetDirectory))
            LibraryInterface.SetTargetDataPath(customTargetDirectory);
        else
            LibraryInterface.SetTargetDataPath(targetDataPath);
        LibraryInterface.SetTargetNames(targetNames);

        //// Optional Map recording
        if (mapRecorder != null && mapRecorder.isActiveAndEnabled)
        {
            // Prepare mapTargetFolder
            if (!Directory.Exists(mapRecorder.GetPlatformDependentSavePath()))
                Directory.CreateDirectory(mapRecorder.GetPlatformDependentSavePath());

            // Retrieve Initialization target
            string initializationName = mapRecorder.InitializationTarget.targetName;
            // Override scene targets
            LibraryInterface.SetTargetNames(new string[] { initializationName });
            // Update local target to new map
            targetNames = new string[] { mapRecorder.NewMapTargetName, initializationName };
            // Duplicate reference to scene target
            targets.Add(mapRecorder.NewMapTargetName, targets[initializationName]);
            targets.Remove(initializationName);
            // Enable map recording
            if (!LibraryInterface.EnableMapTargetGeneration(true, mapRecorder.NewMapTargetName, mapRecorder.GetPlatformDependentSavePath()))
                return false;
        }

        // Initialize Framework
        return LibraryInterface.InitializeFramework();
    }

    /// <summary>
    /// Update function controlled via Unity
    /// </summary>
    public void Update()
    {
        // Call process function
        if (!useThreadController)
            if (IsInitialized && IsRunning)
                RunFramework();

        // Call TrackUpdate function for each Target to apply any track changes
        foreach (TrackTarget target in targets.Values)
        {
            target.gameObject.SetActive(false);
            target.UnityUpdate();
        }
    }

    /// <summary>
    /// Update function controlled via Async Manager
    /// </summary>
    public override void TrackUpdate()
    {
        if (useThreadController)
            if (IsInitialized && IsRunning)
                RunFramework();
    }

    /// <summary>
    /// Executes a tracking cycle
    /// </summary>
    /// <returns>Success or failure of processing the frame</returns>
    private bool RunFramework()
    {
        UpdateTargetInfo();
        UpdateDeviceOrientation();

#if (UNITY_EDITOR || UNITY_STANDALONE)
        // Process next frame
        IsRunning = LibraryInterface.Process();
#elif (UNITY_ANDROID)
        // No need to call process / process function is driven JNI internally
#endif
        // Retrieve and update tracking result data
        if (IsRunning)
        {
            // Invalidate old tracking result
            foreach (TrackTarget target in targets.Values)
                target.InvalidateLastResult();

            // Update new tracking result
            foreach (TrackerResult target in LibraryInterface.GetTrackingResult(targetNames))
                if (targets.ContainsKey(target.mTargetName))
                    targets[target.mTargetName].UpdateTrackingState(target);
        }
        return IsRunning;
    }

    /// <summary>
    /// Helper method to log TargetInfo
    /// </summary>
    public void PrintTargetInfo()
    {
        Debug.Log("Target Info:");
        foreach (TargetInformation info in LibraryInterface.GetTargetInfo(targetNames))
            Debug.Log(info);
    }

    /// <summary>
    /// Update target meta information
    /// </summary>
    public void UpdateTargetInfo()
    {
        // Check all targets if update is necessary
        bool oneIsDirty = false;
        foreach (KeyValuePair<string, TrackTarget> target in targets)
        {
            target.Value.UpdateInfo();
            if (target.Value.isDirty)
            {
                oneIsDirty = true;
                target.Value.isDirty = false;
            }
        }

        // Do not perform update if not necessary
        if (!oneIsDirty)
            return;

        // Get internal framework info
        TargetInformation[] frameworkInfo = LibraryInterface.GetTargetInfo(targetNames);

        // Apply information from scene to framework
        foreach (KeyValuePair<string, TrackTarget> target in targets)
        {
            int index = Array.FindIndex(frameworkInfo, ti => ti.mTargetName == target.Key);
            if (index == -1)
                continue;
            ref TargetInformation info = ref frameworkInfo[index];
            info.mIgnore = target.Value.ignoreForTracking;
            info.mStatic = target.Value.isTargetStatic;
        }

        // Apply new information to framework
        if (!LibraryInterface.SetTargetInfo(frameworkInfo))
            Debug.Log("FAILED updating target info");

        // Get updated framework info
        TargetInformation[] frameworkInfoNew = LibraryInterface.GetTargetInfo(targetNames);
        foreach (KeyValuePair<string, TrackTarget> target in targets)
        {
            // Apply information from framework to objects
            int index = Array.FindIndex(frameworkInfoNew, ti => ti.mTargetName == target.Key);
            if (index == -1)
                continue;

            if (mapRecorder != null && mapRecorder.isActiveAndEnabled)
            {
                if (target.Key == mapRecorder.NewMapTargetName)
                {
                    TargetInformation info = frameworkInfoNew[index];
                    info.mType = TargetType.MAP;
                    target.Value.UpdateGameObject(info);
                    continue;
                }
            }

            target.Value.UpdateGameObject(frameworkInfoNew[index]);
        }
    }

    /// <summary>
    /// Maps and sets the device orientation value from UNITY to TRACK
    /// Sets LANDSCAPE_LEFT as standard if UNITY device orientation does not match with VIRNECT device orientation
    /// </summary>
    private void UpdateDeviceOrientation()
    {
        DeviceOrientation orientation;
        switch (Input.deviceOrientation)
        {
            case UnityEngine.DeviceOrientation.LandscapeLeft: 
                orientation = DeviceOrientation.LANDSCAPE_LEFT;
                break;
            case UnityEngine.DeviceOrientation.LandscapeRight:
                orientation = DeviceOrientation.LANDSCAPE_RIGHT;
                break;
            case UnityEngine.DeviceOrientation.Portrait:
                orientation = DeviceOrientation.PORTRAIT;
                break;
            case UnityEngine.DeviceOrientation.PortraitUpsideDown:
                orientation = DeviceOrientation.PORTRAIT_INVERSE;
                break;
            default:
                orientation = DeviceOrientation.LANDSCAPE_LEFT;
                break;
        }

        DeviceOrientation currentOrientation = LibraryInterface.GetDeviceOrientation();

        if(currentOrientation != orientation)
            LibraryInterface.SetDeviceOrientation(orientation);
    }
}
}
