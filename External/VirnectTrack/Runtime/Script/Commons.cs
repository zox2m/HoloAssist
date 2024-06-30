// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VIRNECT {
/// <summary>
/// Constant values and common classes
/// </summary>
public class Constants
{
    /// <summary>
    /// Target frame rate for unity
    /// </summary>
    public const int targetFrameRate = 60;

    /// <summary>
    /// Product name for sanitizer logs
    /// </summary>
    public const string productNameLog = "VIRNECT Track";

    /// <summary>
    /// Camera calibration path
    /// </summary>
    public const string calibrationPath = "/StreamingAssets/trackAssets/calibration.json";

    /// <summary>
    /// Relative settings asset path
    /// </summary>
    public const string settingsPath = "Track/track.settings";

    /// <summary>
    /// Track settings full path
    /// </summary>
    public const string settingsAssetFullPath = "Assets/Resources/Track/track.settings.asset";

    /// <summary>
    /// Maximum amount of registrable targets
    /// </summary>
    public const int maxTargets = 100;

    /// <summary>
    /// Maximum concurrent targets
    /// </summary>
    public const int maxConcurrent = 3;

    /// <summary>
    /// Root directory of Targets
    /// </summary>
    public const string targetRootDirectory = "/StreamingAssets/trackAssets/trackTargets/";

    /// <summary>
    /// APK internal root directory of targets for android
    /// lower case folder is mandatory for android. Will be compressed otherwise
    /// </summary>
    public const string targetAPKDirectoryAndroid = "!/assets/trackAssets/trackTargets/";

    /// <summary>
    /// Root directory of targets for android
    /// </summary>
    public const string targetRootDirectoryAndroid = "/trackAssets/trackTargets/";

    /// <summary>
    /// Root directory of recorded map targets for android
    /// </summary>
    public const string mapTargetRootDirectoryAndroid = targetRootDirectoryAndroid + "mapTargets/";

    /// <summary>
    /// Target file extension
    /// </summary>
    public const string targetFileExtension = ".track";

    /// <summary>
    /// Package prefab path
    /// </summary>
    public const string targetPrefabPath = "Prefabs/TrackTarget";

    /// <summary>
    /// Tag for target GameObjects
    /// </summary>
    public const string targetTag = "TrackTarget";

    /// <summary>
    /// Path to notification UI prefab
    /// </summary>
    public const string notificationPrefabPath = "Prefabs/NotificationUI";

    /// <summary>
    /// Path to recorder UI prefab
    /// </summary>
    public const string recorderPrefabPath = "Prefabs/TrackRecorderUI";

    /// <summary>
    /// Path to active target manager UI prefab
    /// </summary>
    public const string activeTargetManagerPrefabPath = "Prefabs/TrackActiveTargetManagerUI";

    /// <summary>
    /// Path to static target manager UI prefab
    /// </summary>
    public const string staticTargetManagerPrefabPath = "Prefabs/StaticTargetManagerUI";

    /// <summary>
    /// Path to map recorder UI prefab
    /// </summary>
    public const string mapRecorderPrefabPath = "Prefabs/TrackMapRecorderUI";

    /// <summary>
    /// Path to map visualizer game object prefab
    /// </summary>
    public const string mapVisualizerPrefabPath = "Prefabs/TrackMapPointVisualizer";
}

/// <summary>
///  Type of target
/// </summary>
public enum TargetType
{
    UNDEFINED = 0, ///< Target type is not known
    IMAGE = 1,     ///< Target type is Image
    QRCODE = 2,    ///< Target type is QR code
    CADMODEL = 3,  ///< Target type is CAD model
    MAP = 4,       ///< Target type is Map
    PLANE = 5,     ///< Target type is Plane surface
    BOX = 6,       ///< Target type is Box
    CYLINDER = 7,  ///< Target type is Cylinder
    OBJECT3D = 8   ///< Target type is Object3D
}
;

/// <summary>
/// Tracking state of target
/// </summary>
public enum TrackingState
{
    NOT_TRACKED = 0, ///< Target is not tracked
    TRACKED = 1      ///< Target is tracked
}
;

/// <summary>
/// State of the Tracking Framework
/// </summary>
public enum FrameworkState
{
    NOT_STARTED = 0, ///< Framework is not started
    RUNNING = 1,     ///< Framework is running now
    PAUSED = 2,      ///< Framework is paused
    FINISHED = 3     ///< Framework ended
}
;

/// <summary>
/// Orientation enumeration of the device/camera
/// Sorted clockwise starting with landscape_left, X represents the home button of a smartphone
///
///      1
///    0 X 2
///      3
/// </summary>
public enum DeviceOrientation
{
    LANDSCAPE_LEFT = 0,  ///< screen width > screen height and home button facing right
    PORTRAIT = 1,        ///< screen width <= screen height and home button facing bottom
    LANDSCAPE_RIGHT = 2, ///< screen width > screen height and home button facing left
    PORTRAIT_INVERSE = 3 ///< screen width <= screen height and home button facing top
}
;

/// <summary>
/// Represents one target as a tracking result
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct TrackerResult
{
    public string mTargetName; ///< Target Name
    public UInt64 mFrameID;    ///< FrameID
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public double[] mTranslation; ///< Position in relation to camera
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
    public double[] mRotationMatrix; ///< 3x3 rotation matrix
    public TargetType mType;         ///< Image, QRCode, CAD
    public TrackingState mStatus;    ///< Tracking Status

    public Quaternion rotation ///< Getter of rotation
    {
        get {
            Vector3 forward;
            forward.x = (float)mRotationMatrix[2];
            forward.y = (float)mRotationMatrix[5];
            forward.z = -(float)mRotationMatrix[8];

            Vector3 upwards;
            upwards.x = (float)mRotationMatrix[1];
            upwards.y = (float)mRotationMatrix[4];
            upwards.z = -(float)mRotationMatrix[7];

            return Quaternion.LookRotation(forward, upwards);
        }
    }

    public Vector3 position ///< Getter of position
    {
        get {
            return new Vector3((float)mTranslation[0], (float)mTranslation[1], -(float)mTranslation[2]);
        }
    }

    /// <summary>
    /// Constructor for TrackerResult
    /// </summary>
    /// <param name="name">Target name</param>
    public TrackerResult(string name)
    {
        mTargetName = name;
        mType = 0;
        mStatus = 0;
        mFrameID = 0;
        mTranslation = new double[3] { 0.0, 0.0, 0.0 };
        mRotationMatrix = new double[9] { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };
    }

    /// <summary>
    /// Human readable representation of TrackerResult
    /// </summary>
    /// <returns>Multi lined string</returns>
    public override string ToString()
    {
        return "Tracking target: " + mTargetName + "\n" + "Type: " + mType + "\n" + "Status: " + mStatus + "\n" + "FrameID: " + mFrameID + "\n" + "Position: " + mTranslation + "\n" + "Rotation: \n" +
               rotationToString(mRotationMatrix);
    }

    /// <summary>
    /// Converts a 3x3 rotation matrix in a human readable string
    /// </summary>
    /// <param name="r">rotation matrix</param>
    /// <returns>Multi line string representation of rotation matrix</returns>
    private string rotationToString(double[] r)
    {
        return "[" + r[0] + " ," + r[1] + " ," + r[2] + "]\n" + "[" + r[3] + " ," + r[4] + " ," + r[5] + "]\n" + "[" + r[6] + " ," + r[7] + " ," + r[8] + "]";
    }
}

/// <summary>
/// A three dimensional vector
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Vec3
{
    public double X; ///< X component
    public double Y; ///< Y component
    public double Z; ///< Z component

    /// <summary>
    /// Constructor for Vec3
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="z">Z coordinate</param>
    public Vec3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Human readable representation of Vec3
    /// </summary>
    /// <returns>Values in brackets</returns>
    public override string ToString() { return "[" + X + " | " + Y + " | " + Z + "]"; }
}

/// <summary>
/// Represents the dimensions of an object
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Dimensions
{
    public Vec3 mMin; ///< min components
    public Vec3 mMax; ///< max components

    /// <summary>
    /// Constructor for Dimensions
    /// </summary>
    /// <param name="min">Minimal size</param>
    /// <param name="max">Maximum size</param>
    public Dimensions(Vec3 min, Vec3 max)
    {
        mMin = min;
        mMax = max;
    }

    /// <summary>
    /// Human readable representation of Dimensions
    /// </summary>
    /// <returns>Values in brackets</returns>
    public override string ToString() { return "[" + mMin + " , " + mMax + "]"; }
}

/// <summary>
/// Contains target meta information
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct TargetInformation
{
    public string mTargetName;     ///< Target Name
    public Dimensions mDimensions; ///< Dimensions of the Target
    public TargetType mType;       ///< Image, QRCode, CAD
    [MarshalAs(UnmanagedType.U1)]
    public bool mIgnore;           ///< Image, QRCode, CAD
    [MarshalAs(UnmanagedType.U1)]
    public bool mStatic;           ///< Whether the target is static or dynamic in the scene

    /// <summary>
    /// Constructor for TargetInformation
    /// </summary>
    /// <param name="name">Target name</param>
    public TargetInformation(string name)
    {
        mTargetName = name;
        mType = 0;
        mDimensions = new Dimensions(new Vec3(0.0, 0.0, 0.0), new Vec3(0.0, 0.0, 0.0));
        mIgnore = false;
        mStatic = false;
    }

    /// <summary>
    /// Human readable representation of TargetInformation
    /// </summary>
    /// <returns>Multi lined string</returns>
    public override string ToString() { return $"Tracking target: {mTargetName} \nType: {mType} \nDimensions: {mDimensions} \nIgnore: {mIgnore} \nStatic: {mStatic} \n"; }
}

/// <summary>
/// Intrinsic camera calibration
/// The Camera calibration model is defined by the intrinsic camera parameters:
/// - Resolution: used during Calibration
/// - Focal length: focal length of the lense
/// - Principal point: position of point without distortion
/// - Lense distortion: Our lens distortion model is based on the typical radian and tangential distortions.
///   Parameters [r1, r2, t1, t2, r3], r = radial, t = tangential direction</summary>
[StructLayout(LayoutKind.Sequential)]
public class CameraCalibration
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public Int32[] mResolution = new int[] { 640, 480 }; ///< Image resolution
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public double[] mFocalLength = new double[] { 600.0, 600.0 }; ///< Focal length
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public double[] mPrincipalPoint = new double[] { 320.0, 240.0 }; ///< Principal point
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public double[] mDistortion = new double[] { 0.0, 0.0, 0.0, 0.0, 0.0 }; ///< Lens distortion coefficients
};
}
