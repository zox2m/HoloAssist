// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using UnityEngine;
using System;
using System.IO;
using System.Linq;

namespace VIRNECT {
/// <summary>
/// Public interface to access Track libraries for different platforms
/// </summary>
public class LibraryInterface : LibraryRoot
{
#if UNITY_STANDALONE || UNITY_EDITOR

    /// <summary>
    /// Method to activate logging from the DLL
    /// </summary>
    public static void EnableDLLDebugLog() { RegisterDebugCallback(new UnityLogCallback(UnityLogCallbackMethod)); }

#elif UNITY_ANDROID
    /// <summary>
    /// Disables the support surface mode for the JNI.
    /// When using Unity, this mode should always be disabled
    /// </summary>
    public static void DeactivateSupportSurfaceMode()
    {
        if (!InitializeAndroidPlugins())
            return;
        trackClass.CallStatic("setSupportSurfaceMode", false);
    }

    /// <summary>
    /// Start processing JNI thread
    /// </summary>
    public static void OnResume()
    {
        if (!InitializeAndroidPlugins())
            return;
        trackClass.CallStatic("startProcessing");
    }

    /// <summary>
    /// Stop processing JNI thread
    /// </summary>
    public static void OnPause()
    {
        if (!InitializeAndroidPlugins())
            return;
        trackClass.CallStatic("stopProcessing");
    }

    /// <summary>
    /// Force Android application to dump log in console
    /// </summary>
    public static void DumpLog()
    {
        if (!InitializeAndroidPlugins())
            return;

        trackClass.CallStatic("dumpLog");
    }

#endif

#region Initialization

    /// <summary>
    /// Set the license for the tracking framework
    /// </summary>
    /// <param name="key">License key</param>
    /// <returns>If the key is valid or not</returns>
    public static bool SetLicenseKey(string key)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalSetLicenseKey(key);
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return false;

        return trackClass.CallStatic<bool>("setLicenseKey", key);
#else
        LogNotDefined("SetLicenseKey");
        return false;
#endif
    }

    /// <summary>
    /// Render function on GL thread
    /// </summary>
    /// <returns>GetRenderEventFunc function</returns>
    public static System.IntPtr GetRenderEventFunc()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalGetRenderEventFunc();
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return System.IntPtr.Zero;

        return new System.IntPtr(trackClass.CallStatic<long>("GetRenderEventFunc"));
#endif
    }

    /// <summary>
    /// Render function on GL thread to fetch input image
    /// </summary>
    /// <returns>GetPrepareExternalInputTextureEventFunc function</returns>
    public static System.IntPtr GetPrepareExternalInputTextureEventFunc()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalGetPrepareExternalInputTextureEventFunc();
#elif UNITY_ANDROID
        Debug.LogError("ExternalInputTexture is not supported on Android");
        return System.IntPtr.Zero;
#endif
    }

    /// <summary>
    /// Set texture ID into native
    /// <param name="textureID">Texture2D's Native Ptr</param>
    /// </summary>
    public static void SetTextureFromUnity(System.IntPtr textureID)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        InternalSetTextureFromUnity(textureID);
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return;

        trackClass.CallStatic("setTextureID", (int)textureID);
#endif
    }

    /// <summary>
    /// Get the version of the tracking framework
    /// </summary>
    /// <returns>The framework version as string</returns>
    public static string GetFrameworkVersion()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalGetFrameworkVersion();
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return "";

        return trackClass.CallStatic<string>("getFrameworkVersion");
#endif
    }

    /// <summary>
    /// Load the camera calibration form a JSON file
    /// </summary>
    /// <param name="path">Path to the JSON file</param>
    /// <param name="cameraID">Camera identifier in JSON file</param>
    /// <param name="resolution">Resolution identifier in JSON file</param>
    /// <returns>Success or failure of loading</returns>
    public static bool LoadCameraCalibration(string path, string cameraID, string resolution)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalLoadCameraCalibration(path, cameraID, resolution);
#elif UNITY_ANDROID
        LogNotDefined("LoadCameraCalibration");
        return false;
#endif
    }

    /// <summary>
    /// Enables or disables automatic exposure and focus control of the camera. Needs to be set before framework initialization
    /// </summary>
    /// <param name="enable">Value to set</param>
    /// <returns>Success or failure of setting the parameter</returns>
    public static bool UseAutomaticUSBCameraSetting(bool enable)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalUseAutomaticUSBCameraSetting(enable);
#elif UNITY_ANDROID
        LogNotDefined("UseAutomaticUSBCameraSetting");
        return false;
#endif
    }

    /// <summary>
    /// Set the path to the target binaries. Needs to be set before framework initialization
    /// </summary>
    /// <param name="path">Path to binary file</param>
    /// <returns>Success or failure of setting target data path</returns>
    public static bool SetTargetDataPath(string path)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalSetTargetDataPath(path);
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return false;

        return trackClass.CallStatic<bool>("setTargetDataPath", path);
#endif
    }

    /// <summary>
    /// Set the names of all targets to track
    /// </summary>
    /// <param name="targets">Target names</param>
    /// <returns>Success or failure of setting target name list</returns>
    public static bool SetTargetNames(string[] targets)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalSetTargetNames(targets, targets.Length);
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return false;

        return trackClass.CallStatic<bool>("setTargetNames", (object)targets);
#endif
    }

    /// <summary>
    /// Enables or disables the map target generation
    /// </summary>
    /// <param name="enable">State to be set</param>
    /// <param name="targetName">name of the map target</param>
    /// <param name="savePath">name of the map target</param>
    /// <returns>Success or failure of enabling the feature</returns>
    public static bool EnableMapTargetGeneration(bool enable, string targetName, string savePath)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalEnableMapTargetGeneration(enable, targetName, savePath);

#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return false;

        return trackClass.CallStatic<bool>("enableMapTargetGeneration", enable, targetName, savePath);
#endif
    }

    /// <summary>
    /// Initialize Tracking Framework. Set the framework configuration (camera calibration, target binaries, target names)
    /// </summary>
    /// <returns>Success or failure of initialization</returns>
    public static bool InitializeFramework()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalInitializeFramework();
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return false;

        // Disable YUV in Android HAL
        trackClass.CallStatic("enableYUVBuffer", false);
        return trackClass.CallStatic<bool>("initialize", activityContext);
#endif
    }

    /// <summary>
    /// Set the USB camera as active image source
    /// </summary>
    /// <param name="id">ID of camera (e.g. 0,1)</param>
    /// <returns>Success or failure of USB camera initialization</returns>
    public static bool ActivateUSBCamera(int id)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalActivateUSBCamera(id);
#elif UNITY_ANDROID
        Debug.LogError("Activating USB camera is not possible on Android");
        return false;
#endif
    }

    /// <summary>
    /// Set a file sequence as active image source
    /// </summary>
    /// <param name="path">Path to image file sequence</param>
    /// <returns>Success or failure of file sequence initialization</returns>
    public static bool ActivateFileSequence(string path)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalActivateFileSequence(path);
#elif UNITY_ANDROID
        Debug.LogError("Activating file sequence loader is not possible on Android");
        return false;
#endif
    }

    /// <summary>
    /// Set an external image source as active image source
    /// </summary>
    /// <param name="textureID">ID of texture</param>
    /// <returns>Success or failure of file sequence initialization</returns>
    public static bool ActivateExternalImageSource(System.IntPtr textureID)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalActivateExternalImageSource(textureID);
#elif UNITY_ANDROID
        Debug.LogError("Activating external image source is not possible on Android");
        return false;
#endif
    }

    /// <summary>
    /// Get a list of USB camera names
    /// </summary>
    /// <returns>A list of available USB camera names</returns>
    public static string[] GetDeviceNames()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        int nDevices = InternalGetNumAvailableDevices();
        if (nDevices == 0)
            return Array.Empty<string>();

        string[] devices = new string[nDevices];

        for (int idx = 0; idx < nDevices; idx++)
        {
            const int stringCapacity = 256;
            System.Text.StringBuilder deviceNameBuffer = new System.Text.StringBuilder(stringCapacity);
            if (!InternalFetchUSBCameraName(deviceNameBuffer, idx, stringCapacity + 1))
                continue;
            devices[idx] = deviceNameBuffer.ToString();
        }

        return devices;
#elif UNITY_ANDROID
        return Array.Empty<string>();
#endif
    }

    /// <summary>
    /// Inspect the Target root folder and fetch the metadata from every present ".track" file
    /// </summary>
    /// <returns> An array containing all the available TargetInformation </returns>
    public static TargetInformation[] FetchTargetInfoFromFolder()
    {
#if UNITY_STANDALONE || UNITY_EDITOR

        string targetsDirPath = Application.dataPath + Constants.targetRootDirectory;
        string[] trackFiles = Directory.GetFiles(targetsDirPath, "*" + Constants.targetFileExtension);
        int numTargets = trackFiles.Length;

        if (numTargets == 0)
            return Array.Empty<TargetInformation>();

        TargetInformation[] targetInfos = trackFiles.Select(path => new TargetInformation(Path.GetFileNameWithoutExtension(path))).ToArray();
        if (!InternalFetchTargetInfoFromDir(targetInfos, numTargets, targetsDirPath))
            return Array.Empty<TargetInformation>();

        foreach (TargetInformation target in targetInfos.Where(t => t.mType == 0))
        {
            Debug.LogWarning($"This target has UNDEFINED type: {target.mTargetName}");
        }

        return targetInfos;
#elif UNITY_ANDROID
        return Array.Empty<TargetInformation>();
#endif
    }

#endregion

#region Process

    /// <summary>
    /// Requests a frame from the image source and tries to process it.
    /// </summary>
    /// <returns>Success or failure of frame processing</returns>
    public static bool Process()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalProcess();
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return false;

        return trackClass.CallStatic<bool>("process");
#endif
    }

    /// <summary>
    /// Retrieve precessing image's size
    /// </summary>
    /// <returns>Preview image size</returns>
    public static int[] GetPreviewSize()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        // Used preview resolution is defined in calibration file
        CameraCalibration calibration = new CameraCalibration();
        InternalGetCameraCalibration(calibration);
        return calibration.mResolution;
#else
        if (!InitializeAndroidPlugins())
            return new int[] { 0, 0 };
        return trackClass.CallStatic<int[]>("getPreviewSize");
#endif
    }

    /// <summary>
    /// Returns a TrackerResult array
    /// </summary>
    /// <param name="targets">Target names</param>
    /// <returns>TrackerResult array</returns>
    public static TrackerResult[] GetTrackingResult(string[] targets)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        // Instantiate Tracking Results:
        TrackerResult[] trackingResults = new TrackerResult[targets.Length];

        for (int i = 0; i < targets.Length; i++)
            trackingResults[i] = new TrackerResult(targets[i]);

        // Retrieve results from Library
        InternalGetTrackingResult(trackingResults, trackingResults.Length);
        return trackingResults;

#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return null;

        // Restrict amount of targets
        int maxCount = Mathf.Min(Constants.maxConcurrent, targets.Length);
        AndroidJavaObject arrayObject = getTrackingResultArray(maxCount);

        // Retrieve results
        int size = trackClass.CallStatic<int>("getConcurrentTrackingResult", arrayObject, maxCount);
        TrackerResult[] trackingResults = new TrackerResult[size];

        for (int i = 0; i < size; i++)
        {
            AndroidJavaObject jniResult = javaArrayClass.CallStatic<AndroidJavaObject>("get", arrayObject, i);
            TrackerResult trackingResult = new TrackerResult(jniResult.Get<string>("mTargetName"));
            trackingResult.mTranslation = jniResult.Get<double[]>("mTranslation");
            trackingResult.mRotationMatrix = jniResult.Get<double[]>("mRotationMatrix");
            trackingResult.mStatus = (TrackingState)jniResult.Call<int>("getStatus");
            trackingResult.mType = (TargetType)jniResult.Call<int>("getType");
            trackingResults[i] = trackingResult;
        }
        return trackingResults;
#else
        return null;
#endif
    }

    /// <summary>
    /// Returns a TargetInformation array
    /// </summary>
    /// <param name="targets">Target names</param>
    /// <returns>TargetInformation array</returns>
    public static TargetInformation[] GetTargetInfo(string[] targets)
    {
        // Instantiate Tracking Informations:
        TargetInformation[] targetInformations = new TargetInformation[targets.Length];
#if UNITY_STANDALONE || UNITY_EDITOR

        for (int i = 0; i < targets.Length; i++)
            targetInformations[i] = new TargetInformation(targets[i]);

        // Retrieve results from Library
        InternalGetTargetInfo(targetInformations, targetInformations.Length);

#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return null;

        AndroidJavaObject arrayObject = getTargetInformationArray(targets.Length);
        trackClass.CallStatic("getTargetInfo", arrayObject);

        for (int i = 0; i < targets.Length; i++)
        {
            AndroidJavaObject jniResult = javaArrayClass.CallStatic<AndroidJavaObject>("get", arrayObject, i);
            TargetInformation targetInfo = new TargetInformation(targets[i]);

            double[] maxDimension = jniResult.Call<double[]>("getMax");
            targetInfo.mDimensions.mMax.X = maxDimension[0];
            targetInfo.mDimensions.mMax.Y = maxDimension[1];
            targetInfo.mDimensions.mMax.Z = maxDimension[2];

            double[] minDimension = jniResult.Call<double[]>("getMin");
            targetInfo.mDimensions.mMin.X = minDimension[0];
            targetInfo.mDimensions.mMin.Y = minDimension[1];
            targetInfo.mDimensions.mMin.Z = minDimension[2];

            targetInfo.mIgnore = jniResult.Get<bool>("mIgnore");
            targetInfo.mType = (TargetType)jniResult.Call<int>("getType");

            targetInformations[i] = targetInfo;
        }
#endif
        return targetInformations;
    }

    /// <summary>
    /// Set target information in the framework
    /// </summary>
    /// <param name="targetInformation">Information to set</param>
    /// <returns>Success or failure of applying all values</returns>
    public static bool SetTargetInfo(TargetInformation[] targetInformation)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalSetTargetInfo(targetInformation, targetInformation.Length);
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return false;

        AndroidJavaObject arrayObject = getTargetInformationArray(targetInformation.Length);

        for (int i = 0; i < targetInformation.Length; i++)
        {
            AndroidJavaObject info = new AndroidJavaObject("com/virnect/Common$TargetInformation", targetInformation[i].mTargetName);

            info.Call("setMax", targetInformation[i].mDimensions.mMax.X, targetInformation[i].mDimensions.mMax.Y, targetInformation[i].mDimensions.mMax.Z);
            info.Call("setMin", targetInformation[i].mDimensions.mMin.X, targetInformation[i].mDimensions.mMin.Y, targetInformation[i].mDimensions.mMin.Z);

            AndroidJavaClass typeClass = new AndroidJavaClass("com/virnect/Common$TargetType");
            info.Set<AndroidJavaObject>("mType", typeClass.CallStatic<AndroidJavaObject>("valueOf", targetInformation[i].mType.ToString()));
            info.Set<bool>("mIgnore", targetInformation[i].mIgnore);
            javaArrayClass.CallStatic("set", arrayObject, i, info);
        }

        return trackClass.CallStatic<bool>("setTargetInfo", arrayObject);
#endif
    }

    /// <summary>
    /// Get current framework state
    /// </summary>
    /// <returns>Framework state</returns>
    public static FrameworkState GetFrameworkState()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalGetFrameworkState();
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return FrameworkState.NOT_STARTED;

        AndroidJavaObject state = trackClass.CallStatic<AndroidJavaObject>("getFrameworkState");
        return (FrameworkState)state.Call<int>("ordinal");
#endif
    }

    /// <summary>
    /// Get map points version
    /// </summary>
    /// <param name="targetName">Name of map target</param>
    /// <returns>Map points version</returns>
    public static int GetMapPointsVersion(string targetName)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalGetMapPointsVersion(targetName);
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return 0;

        return trackClass.CallStatic<int>("getMapPointsVersion", targetName);
#endif
    }

    /// <summary>
    /// Get map points array
    /// </summary>
    /// <param name="targetName">Name of map target</param>
    /// <returns>Map points array</returns>
    public static Vec3[] GetMapPoints(string targetName)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        int size = InternalGetMapPointCount(targetName);
        Vec3[] points = new Vec3[size];

        InternalGetMapPoints(points, targetName);

        return points;
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return new Vec3[1];

        AndroidJavaObject arrayObject = trackClass.CallStatic<AndroidJavaObject>("getMapPoints", targetName);
        AndroidJavaClass typeClass = new AndroidJavaClass("com/virnect/Common$Vec3");

        int size = javaArrayClass.CallStatic<int>("getLength", arrayObject);
        Vec3[] points = new Vec3[size];

        for (int i = 0; i < size; i++)
        {
            AndroidJavaObject pointJava = javaArrayClass.CallStatic<AndroidJavaObject>("get", arrayObject, i);

            points[i].X = pointJava.Get<double>("x");
            points[i].Y = pointJava.Get<double>("y");
            points[i].Z = pointJava.Get<double>("z");
        }

        return points;
#endif
    }

    /// <summary>
    /// Provides the current camera calibration parameters
    /// </summary>
    /// <returns>Camera calibration of the framework</returns>
    public static CameraCalibration GetCameraCalibration()
    {
        CameraCalibration calibration = new CameraCalibration();
#if UNITY_STANDALONE || UNITY_EDITOR
        InternalGetCameraCalibration(calibration);
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return null;

        // Instantiate Tracking Results:
        AndroidJavaObject calibrationJavaResults = new AndroidJavaObject("com/virnect/hw/Common$CameraCalibration");

        trackClass.CallStatic("getCameraCalibration", calibrationJavaResults);

        calibration.mResolution = calibrationJavaResults.Get<int[]>("mResolution");
        calibration.mFocalLength = calibrationJavaResults.Get<double[]>("mFocalLength");
        calibration.mPrincipalPoint = calibrationJavaResults.Get<double[]>("mPrincipalPoint");
        calibration.mDistortion = calibrationJavaResults.Get<double[]>("mDistortion");
#endif
        return calibration;
    }

    /// <summary>
    /// Saves the target map
    /// </summary>
    /// <returns>Success or failure of saving the target</returns>
    public static bool SaveMapTarget()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalSaveMapTarget();
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return false;

        return trackClass.CallStatic<bool>("saveMapTarget");
#endif
    }

    /// <summary>
    /// Set the device orientation (thread safe)
    /// </summary>
    /// <param name="deviceOrientation">New device orientation</param>
    public static void SetDeviceOrientation(DeviceOrientation deviceOrientation)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        InternalSetDeviceOrientation(deviceOrientation);
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return;

        AndroidJavaClass enumClass = new AndroidJavaClass("com/virnect/Common$DeviceOrientation");
        AndroidJavaObject enumObject = enumClass.GetStatic<AndroidJavaObject>(deviceOrientation.ToString());
        trackClass.CallStatic("setDeviceOrientation", enumObject);
#endif
    }

    /// <summary>
    /// Get the current device orientation (thread safe)
    /// </summary>
    /// <returns>Current device orientation</returns>
    public static DeviceOrientation GetDeviceOrientation()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalGetDeviceOrientation();
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return DeviceOrientation.LANDSCAPE_LEFT;

        AndroidJavaObject state = trackClass.CallStatic<AndroidJavaObject>("getDeviceOrientation");
        return (DeviceOrientation)state.Call<int>("ordinal");
#endif
    }

#endregion

#region Cleanup

    /// <summary>
    /// Stop the tracking framework
    /// </summary>
    /// <returns>Success or failure of stopping</returns>
    public static bool Stop()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        bool success = InternalStop();
        if (success)
            gLogger.Log("Tracking framework stopped");
        else
            gLogger.LogError(gTAG, "An error appeared while stopping the framework");
        return success;
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return false;

        return trackClass.CallStatic<bool>("stop");
#endif
    }

    /// <summary>
    /// Destruct and free all resources
    /// </summary>
    /// <returns>Success or failure of destruction</returns>
    public static bool Cleanup()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        bool success = InternalCleanup();
        if (success)
            gLogger.Log("Tracking framework resources freed");
        else
            gLogger.LogError(gTAG, "An error appeared while freeing all framework resources");
        return success;
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return false;

        return trackClass.CallStatic<bool>("close");
#endif
    }

#endregion

#region RecordingTool

    /// <summary>
    /// Will record the camera input stream. For each call of the process function, the current frame is saved as an individual JPG or PGM image
    /// </summary>
    /// <param name="folderPath">Root path to save recording. Sub folders will be generated</param>
    /// <param name="pgmFormat">If the images should be saved as PGM instead of JPG</param>
    /// <returns>If the recording started successfully</returns>
    public static bool StartRecording(string folderPath, bool pgmFormat = false)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        bool success = InternalStartRecording(folderPath, pgmFormat);
        if (success)
            gLogger.Log("Recording started. Root folder: " + folderPath);
        else
            gLogger.LogError(gTAG, "Can not start recording");
        return success;
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return false;
        return trackClass.CallStatic<bool>("startRecording", folderPath, pgmFormat);
#endif
    }

    /// <summary>
    /// Will stop the recording
    /// </summary>
    /// <returns>If the recording stopped successfully</returns>
    public static bool StopRecording()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        bool success = InternalStopRecording();
        if (success)
            gLogger.Log("Recording stopped");
        else
            gLogger.LogError(gTAG, "Can not start recording");
        return success;
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return false;

        return trackClass.CallStatic<bool>("stopRecording");
#endif
    }

    /// <summary>
    /// Indicates if the recording is currently active
    /// </summary>
    /// <returns>True if the recording is active</returns>
    public static bool IsRecording()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return InternalIsRecording();
#elif UNITY_ANDROID
        if (!InitializeAndroidPlugins())
            return false;

        return trackClass.CallStatic<bool>("isRecording");
#endif
    }

#endregion
}
}
