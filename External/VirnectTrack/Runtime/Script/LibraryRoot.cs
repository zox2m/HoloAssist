// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using System.Runtime.InteropServices;
using UnityEngine;

namespace VIRNECT {
/// <summary>
/// Interface declaring access to functions of the dynamic libraries for native platforms
/// </summary>
public class LibraryRoot
{
#region Library declaration

    // Define library reference for different platforms
#if UNITY_STANDALONE || UNITY_EDITOR
    const string dll = "VARAPI-Shared"; // VARAPI-Shared.dll
#elif UNITY_ANDROID
    /// <summary>
    /// Android main context
    /// </summary>
    protected static AndroidJavaObject activityContext = null;

    /// <summary>
    /// VIRNECT TRACK class
    /// </summary>
    protected static AndroidJavaClass trackClass = null;

    /// <summary>
    /// Initialize Android plug-ins
    /// </summary>
    /// <returns>Success or failure of getting context and class</returns>
    protected static bool InitializeAndroidPlugins()
    {
        if (trackClass != null)
            return true;

        using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
        }

        trackClass = new AndroidJavaClass("com.virnect.Track");

        if (trackClass == null)
        {
            Debug.LogError("TRACK - Failed to get Android Library. Please check library is in plugins/Android folder");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Helper to access java array functions
    /// </summary>
    protected static AndroidJavaClass javaArrayClass = new AndroidJavaClass("java.lang.reflect.Array");

    /// <summary>
    /// Reusable target information array. Use getTargetInformationArray to get controlled reference
    /// </summary>
    protected static AndroidJavaObject targetInformationArray = null;

    /// <summary>
    /// Meta information about targetInformationArray to reduce JNI calls
    /// </summary>
    protected static int targetInformationArraySize = -1;

    /// <summary>
    /// Retrieve the reusable target information array. The size might be bigger than the size variable
    /// </summary>
    /// <param name="size">Minimum size</param>
    protected static AndroidJavaObject getTargetInformationArray(int size)
    {
        if (size > targetInformationArraySize)
        {
            Debug.LogWarning("Initializing AndroidJavaObject getTargetInformationArray");
            targetInformationArray = javaArrayClass.CallStatic<AndroidJavaObject>("newInstance", new AndroidJavaClass("com/virnect/Common$TargetInformation"), size);
            targetInformationArraySize = size;

            // Populate with initial content
            for (int i = 0; i < size; i++)
                javaArrayClass.CallStatic("set", targetInformationArray, i, new AndroidJavaObject("com/virnect/Common$TargetInformation", "initial"));
        }
        return targetInformationArray;
    }

    /// <summary>
    /// Reusable tracking result array. Use getTrackingResultArray to get controlled reference
    /// </summary>
    protected static AndroidJavaObject trackingResultArray = null;

    /// <summary>
    /// Meta information about trackingResultArray to reduce JNI calls
    /// </summary>
    protected static int trackingResultArraySize = -1;

    /// <summary>
    /// Retrieve the reusable tracking result array. The size might be bigger than the requested size variable
    /// </summary>
    /// <param name="size">Minimum size</param>
    protected static AndroidJavaObject getTrackingResultArray(int size)
    {
        if (size > trackingResultArraySize)
        {
            Debug.LogWarning("Initializing AndroidJavaObject trackingResultArraySize");
            trackingResultArray = javaArrayClass.CallStatic<AndroidJavaObject>("newInstance", new AndroidJavaClass("com/virnect/Common$TrackerResult"), size);
            trackingResultArraySize = size;

            // Populate with initial content
            for (int i = 0; i < size; i++)
                javaArrayClass.CallStatic("set", trackingResultArray, i, new AndroidJavaObject("com/virnect/Common$TrackerResult", "initial"));
        }
        return trackingResultArray;
    }

#else
    const string dll = "";
#endif

#endregion
#region Logging

    /// <summary>
    /// Reference to Unity debug logger
    /// </summary>
    protected static readonly ILogger gLogger = Debug.unityLogger;

    /// <summary>
    /// Logger tag for Virnect
    /// </summary>
    protected static readonly string gTAG = "[VIRNECT TRACK UNITY]";

    /// <summary>
    /// Unified error log for reporting "no implementation" on various platforms
    /// </summary>
    /// <param name="methodName">Name of calling method</param>
    protected static void LogNotDefined(string methodName) { gLogger.LogError(gTAG, "Method \"" + methodName + "\" not implemented for Platform " + Application.platform.ToString()); }

#if UNITY_STANDALONE || UNITY_EDITOR

    /// <summary>
    /// Logger tag for Virnect library logs
    /// </summary>
    protected static readonly string gTAGDLL = "[VIRNECT]";

    /// <summary>
    /// Delegate definition of logging callback interface
    /// </summary>
    /// <param name="message">Message to log</param>
    /// <param name="level">log level</param>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    protected delegate void UnityLogCallback(string message, int level);

    /// <summary>
    /// Method to invoke logger callback function from library
    /// </summary>
    /// <param name="callback">Callback function of type UnityLogCallback</param>
    [DllImport(dll, EntryPoint = "registerUnityLogCallback")]
    protected static extern void RegisterDebugCallback(UnityLogCallback callback);

    /// <summary>
    /// Callback method invoked when the DLL calls the log method
    /// </summary>
    /// <param name="message">Message to log</param>
    /// <param name="level">log level</param>
    [AOT.MonoPInvokeCallback(typeof(UnityLogCallback))]
    protected static void UnityLogCallbackMethod(string message, int level)
    {
        LogType type = LogType.Log;
        switch (level)
        {
        case 1: type = LogType.Log; break;
        case 2: type = LogType.Warning; break;
        case 3: type = LogType.Error; break;
        default: type = LogType.Log; break;
        }
        gLogger.Log(type, gTAGDLL, message);
    }

#endif

#endregion

#if UNITY_STANDALONE || UNITY_EDITOR
#region Initialization

    /// <summary>
    /// Set the license for the tracking framework
    /// </summary>
    /// <param name="key">License key</param>
    /// <returns>If the key is valid or not</returns>
    [DllImport(dll, EntryPoint = "setLicenseKey")]
    protected static extern bool InternalSetLicenseKey(string key);

    [DllImport(dll, EntryPoint = "getRenderEventFunc")]
    protected static extern System.IntPtr InternalGetRenderEventFunc();

    [DllImport(dll, EntryPoint = "getPrepareExternalInputTextureEventFunc")]
    protected static extern System.IntPtr InternalGetPrepareExternalInputTextureEventFunc();

    /// <summary>
    /// Set texture ID into native
    /// EntryPoint is setTextureID
    /// <param name="textureID">Texture2D's Native Ptr</param>
    /// </summary>
    [DllImport(dll, EntryPoint = "setTextureID")]
    protected static extern void InternalSetTextureFromUnity(System.IntPtr textureID);

    /// <summary>
    /// Get the version of the tracking framework
    /// char* is marshaled as LPStr(pointer to a NULL-terminated ANSI character array)
    /// </summary>
    /// <returns>The framework version as string</returns>
    [DllImport(dll, EntryPoint = "getFrameworkVersion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    [return:MarshalAs(UnmanagedType.LPStr)]
    protected static extern string InternalGetFrameworkVersion();

    /// <summary>
    /// Load the camera calibration form a JSON file
    /// </summary>
    /// <param name="path">Path to the JSON file</param>
    /// <param name="cameraID">Camera identifier in JSON file</param>
    /// <param name="resolution">Resolution identifier in JSON file</param>
    /// <returns>Success or failure of loading</returns>
    [DllImport(dll, EntryPoint = "loadCameraCalibration", CallingConvention = CallingConvention.Cdecl)]
    protected static extern bool InternalLoadCameraCalibration(string path, string cameraID, string resolution);

    [DllImport(dll, EntryPoint = "useAutomaticUSBCameraSetting")]
    protected static extern bool InternalUseAutomaticUSBCameraSetting(bool enable);

    /// <summary>
    /// Set the path to the target binaries. Needs to be set before framework initialization
    /// </summary>
    /// <param name="path">Path to binary file</param>
    [DllImport(dll, EntryPoint = "setTargetDataPath")]
    protected static extern bool InternalSetTargetDataPath(string path);

    /// <summary>
    /// Set the targets to track. Needs to be set before framework initialization
    /// </summary>
    /// <param name="targets">Names of targets to track</param>
    /// <param name="size">Amount of targets to track</param>
    [DllImport(dll, EntryPoint = "setTargetNames", CallingConvention = CallingConvention.Cdecl)]
    protected static extern bool InternalSetTargetNames(string[] targets, int size);

    /// <summary>
    /// Enables or disables the map target generation
    /// </summary>
    /// <param name="enable">State to be set</param>
    /// <param name="targetName">name of the map target</param>
    /// <param name="savePath">name of the map target</param>
    /// <returns>Success or failure of enabling the feature</returns>
    [DllImport(dll, EntryPoint = "enableMapTargetGeneration", CallingConvention = CallingConvention.Cdecl)]
    protected static extern bool InternalEnableMapTargetGeneration(bool enable, string targetName, string savePath);

    /// <summary>
    /// Initialize Tracking Framework. Set the framework configuration (camera calibration, target binaries, target names)
    /// </summary>
    /// <returns>Success or failure of initialization</returns>
    [DllImport(dll, EntryPoint = "initializeFramework")]
    protected static extern bool InternalInitializeFramework();

    /// <summary>
    /// Set the USB camera as active image source
    /// </summary>
    /// <param name="id">ID of camera (e.g. 0,1)</param>
    /// <returns>Success or failure of USB camera initialization</returns>
    [DllImport(dll, EntryPoint = "activateUSBCamera")]
    protected static extern bool InternalActivateUSBCamera(int id);

    /// <summary>
    /// Set a file sequence as active image source
    /// </summary>
    /// <param name="path">Path to image file sequence</param>
    /// <returns>Success or failure of file sequence initialization</returns>
    [DllImport(dll, EntryPoint = "activateFileSequence")]
    protected static extern bool InternalActivateFileSequence(string path);

    /// <summary>
    /// Set a Texture as active image source
    /// </summary>
    /// <param name="textureID">Input Texture2D's Native Ptr</param>
    /// <returns>Success or failure of file sequence initialization</returns>
    [DllImport(dll, EntryPoint = "activateExternalImageSource")]
    protected static extern bool InternalActivateExternalImageSource(System.IntPtr textureID);

    /// <summary>
    /// Get name of the the camera device with the given ID.
    /// </summary>
    /// <param name="deviceName">StringBuilder acting as a memory buffer where the name will be written</param>
    /// <param name="deviceId">Camera Device ID of which the name is requested</param>
    /// <param name="bufferSize">StringBuilder buffer allocated memory size in bytes</param>
    /// <returns>True if the name was written on the buffer successfully. False otherwise</returns>
    [DllImport(dll, EntryPoint = "fetchUSBCameraName", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
    protected static extern bool InternalFetchUSBCameraName([In, Out] [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder deviceName, int deviceId, int bufferSize);

    /// <summary>
    /// Returns number of available USB camera devices
    /// </summary>
    /// <returns>Number of camera devices</returns>
    [DllImport(dll, EntryPoint = "getNumAvailableDevices")]
    protected static extern int InternalGetNumAvailableDevices();

    /// <summary>
    /// Fetch the TargetInformation metadata from every track file present in the given directory path
    /// </summary>
    /// <param name="targetInfo">Array of TargetInformation. Should be pre initialized with the respective target name, and the remaining information will be loaded and stored here.</param>
    /// <param name="size">Size of the TargetInformation array (or number of Targets (.track files) available in the directory)</param>
    /// <param name="dirPath">String containing the file-system path where to look for the .track files</param>
    /// <returns>True if this operation is executed successfully. False otherwise</returns>
    [DllImport(dll, EntryPoint = "fetchTargetInfoFromDir", CharSet = CharSet.Ansi)]
    protected static extern bool InternalFetchTargetInfoFromDir([In, Out] TargetInformation[] targetInfo, int size, string dirPath);

#endregion
#region Process

    /// <summary>
    /// Requests a frame from the image source and tries to process it.
    /// </summary>
    /// <returns>Success or failure of frame processing</returns>
    [DllImport(dll, EntryPoint = "process")]
    protected static extern bool InternalProcess();

    /// <summary>
    /// Set the values of the tracking results into the given TrackingTarget array
    /// </summary>
    /// <param name="resultArray">Preinitialized Tracking Targets</param>
    /// <param name="size">Amount of targets</param>
    [DllImport(dll, EntryPoint = "getTrackingResult")]
    protected static extern void InternalGetTrackingResult([In, Out] TrackerResult[] resultArray, int size);

    /// <summary>
    /// Fills the given TargetInformation array
    /// </summary>
    /// <param name="resultArray">Preinitialized target information</param>
    /// <param name="size">Amount of targets</param>
    [DllImport(dll, EntryPoint = "getTargetInfo")]
    protected static extern void InternalGetTargetInfo([In, Out] TargetInformation[] resultArray, int size);

    /// <summary>
    /// Set target information in the framework
    /// </summary>
    /// <param name="inputArray">Information to set</param>
    /// <param name="size">Size of inputArray</param>
    /// <returns>Success or failure of applying all values</returns>
    [DllImport(dll, EntryPoint = "setTargetInfo")]
    protected static extern bool InternalSetTargetInfo([In, Out] TargetInformation[] inputArray, int size);

    /// <summary>
    /// Get current framework state
    /// </summary>
    /// <returns>Framework state</returns>
    [DllImport(dll, EntryPoint = "getFrameworkState")]
    protected static extern FrameworkState InternalGetFrameworkState();

    /// <summary>
    /// Get Map point version
    /// </summary>
    /// <param name="targetName">Name of map target</param>
    /// <returns>Version of map points</returns>
    [DllImport(dll, EntryPoint = "getMapPointsVersion")]
    protected static extern int InternalGetMapPointsVersion(string targetName);

    /// <summary>
    /// Get size of Map points array
    /// </summary>
    /// <param name="targetName">Name of map target</param>
    /// <returns>Amount of Map points array</returns>
    [DllImport(dll, EntryPoint = "getMapPointCount")]
    protected static extern int InternalGetMapPointCount(string targetName);

    /// <summary>
    /// Get Map points array
    /// </summary>
    /// <param name="points">Map points array of correct size</param>
    /// <param name="targetName">Name of map target</param>
    /// <returns>Map points array</returns>
    [DllImport(dll, EntryPoint = "getMapPoints")]
    protected static extern void InternalGetMapPoints(Vec3[] points, string targetName);

    /// <summary>
    /// Provides the current camera calibration parameters
    /// </summary>
    /// <param name="calibration">Camera calibration to populate</param>
    [DllImport(dll, EntryPoint = "getCameraCalibration")]
    protected static extern void InternalGetCameraCalibration([In, Out] CameraCalibration calibration);

    /// <summary>
    /// Saves the target map
    /// </summary>
    /// <returns>Success or failure of saving the target</returns>
    [DllImport(dll, EntryPoint = "saveMapTarget")]
    protected static extern bool InternalSaveMapTarget();

    /// <summary>
    /// Set the device orientation (thread safe)
    /// </summary>
    /// <param name="deviceOrientation">New device orientation</param>
    [DllImport(dll, EntryPoint = "setDeviceOrientation")]
    protected static extern void InternalSetDeviceOrientation(DeviceOrientation deviceOrientation);

    /// <summary>
    /// Get the current device orientation (thread safe)
    /// </summary>
    /// <returns>Current device orientation</returns>
    [DllImport(dll, EntryPoint = "getDeviceOrientation")]
    protected static extern DeviceOrientation InternalGetDeviceOrientation();

#endregion
#region Cleanup

    /// <summary>
    /// Stop the tracking framework
    /// </summary>
    /// <returns>Success or failure of stopping</returns>
    [DllImport(dll, EntryPoint = "stop")]
    protected static extern bool InternalStop();

    /// <summary>
    /// Destruct and free all resources
    /// </summary>
    /// <returns>Success or failure of destruction</returns>
    [DllImport(dll, EntryPoint = "cleanup")]
    protected static extern bool InternalCleanup();

#endregion
#region RecordingTool

    /// <summary>
    /// Will record the camera input stream. For each call of the process function, the current frame is saved as an individual JPG or PGM image
    /// </summary>
    /// <param name="folderPath">Root path to save recording. Sub folders will be generated</param>
    /// <param name="pgmFormat">If the images should be saved as PGM instead of JPG</param>
    /// <returns>If the recording started successfully</returns>
    [DllImport(dll, EntryPoint = "startRecording")]
    protected static extern bool InternalStartRecording(string path, bool pgmFormat);

    /// <summary>
    /// Will stop the recording
    /// </summary>
    /// <returns>If the recording stopped successfully</returns>
    [DllImport(dll, EntryPoint = "stopRecording")]
    protected static extern bool InternalStopRecording();

    /// <summary>
    /// Indicates if the recording is currently active
    /// </summary>
    /// <returns>True if the recording is active</returns>
    [DllImport(dll, EntryPoint = "isRecording")]
    protected static extern bool InternalIsRecording();

#endregion
#endif
}
}
