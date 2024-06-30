// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using System.Linq;
using UnityEngine;

namespace VIRNECT {
/// <summary>
/// This script identifies a TrackingTarget GameObject for the TrackManager
/// </summary>
public class TrackTarget : MonoBehaviour
{
    /// <summary>
    /// Identifier
    /// </summary>
    public string targetName;

    /// <summary>
    /// Set visibility of placeholder GameObject
    /// </summary>
    public bool hidePlaceholder;

    /// <summary>
    /// Reference to placeholder child object
    /// </summary>
    private GameObject Placeholder = null;

    /// <summary>
    /// Target meta information
    /// </summary>
    private TargetInformation targetInfo;

    /// <summary>
    /// Latest tracking result
    /// </summary>
    private TrackerResult target;

    /// <summary>
    /// Internal state of the 'isIgnored' property, represents framework internal state
    /// </summary>
    private bool _ignoreForTracking = false;

    /// <summary>
    /// Exposed state of the 'isIgnored' property, can be changed by user
    /// </summary>
    public bool ignoreForTracking = false;

    /// <summary>
    /// Internal state of the 'isStatic' property, represents framework internal state
    /// </summary>
    private bool _isTargetStatic = false;

    /// <summary>
    /// Exposed state of the 'isStatic' property, can be changed by user
    /// </summary>
    public bool isTargetStatic = false;

    /// <summary>
    /// Indicates if target info got changed and needs to be updated by the framework
    /// </summary>
    public bool isDirty = true;

    /// <summary>
    /// Indicates if target info got changed by the framework and needs to be updated in the scene
    /// </summary>
    private bool isTargetInfoDirty = false;

    /// <summary>
    /// Lock for exclusive access
    /// </summary>
    private readonly object trackLock = new object();

    /// <summary>
    /// Sets reference to placeholder and applies visibility
    /// </summary>
    void Start()
    {
        MeshRenderer placeholder = GetComponentsInChildren<MeshRenderer>().FirstOrDefault<MeshRenderer>(r => r.CompareTag(Constants.targetTag));
        Placeholder = placeholder.gameObject;
        Placeholder.SetActive(!hidePlaceholder);
    }

    /// <summary>
    /// Update method called by TrackManager during Unity update cycle
    /// Performs actual pose update of GameObject
    /// </summary>
    public void UnityUpdate()
    {
        // Update target info
        if (isTargetInfoDirty)
        {
            ignoreForTracking = targetInfo.mIgnore;
            isTargetStatic = targetInfo.mStatic;

            // Update placeholder dimensions and position
            if (Placeholder.activeSelf)
            {
                Placeholder.transform.localScale =
                    new Vector3((float)(targetInfo.mDimensions.mMax.X - targetInfo.mDimensions.mMin.X), (float)(targetInfo.mDimensions.mMax.Y - targetInfo.mDimensions.mMin.Y),
                                (float)(targetInfo.mDimensions.mMax.Z - targetInfo.mDimensions.mMin.Z));
                Placeholder.transform.localPosition =
                    new Vector3((float)(targetInfo.mDimensions.mMin.X + targetInfo.mDimensions.mMax.X), (float)(targetInfo.mDimensions.mMin.Y + targetInfo.mDimensions.mMax.Y),
                                (float)(targetInfo.mDimensions.mMin.Z + targetInfo.mDimensions.mMax.Z));

                // Compensate axis offset
                if (targetInfo.mType == TargetType.BOX || targetInfo.mType == TargetType.CYLINDER)
                    Placeholder.transform.localPosition = new Vector3(Placeholder.transform.localPosition.x, Placeholder.transform.localPosition.y, (float)(targetInfo.mDimensions.mMax.Z - targetInfo.mDimensions.mMin.Z) / 2f);
            }

            isTargetInfoDirty = false;
        }

        // Update Tracking state
        lock (trackLock)
        {
            // Determine the active state of the GameObject depending on Tracking state and Ignore state for the Tracking result
            bool active = (target.mStatus == TrackingState.TRACKED) && !ignoreForTracking;
            gameObject.SetActive(active);

            if (!active)
                return;

            // Set position in relation to camera
            transform.position = TrackManager.Instance.ARCamera.transform.TransformPoint(target.position);

            // Set rotation in relation to camera
            transform.rotation = TrackManager.Instance.ARCamera.transform.rotation * target.rotation;

#if (UNITY_ANDROID && !UNITY_EDITOR)
            if (Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                // X,Y invert and rotate 180 on Z axis
                transform.position = new Vector3(-transform.position.x, -transform.position.y, transform.position.z);
                transform.rotation = new Quaternion(-transform.rotation.x, -transform.rotation.y, transform.rotation.z, transform.rotation.w) * new Quaternion(0, 0, 1, 0);
            }
            else if (Screen.orientation == ScreenOrientation.Portrait)
            {
                // Change X,Y and Y invert and rotate 90 on Z axis
                transform.position = new Vector3(transform.position.y, -transform.position.x, transform.position.z);
                transform.rotation = new Quaternion(transform.rotation.y, -transform.rotation.x, transform.rotation.z, transform.rotation.w) * Quaternion.FromToRotation(Vector3.up, Vector3.right);
            }
#endif
        }
    }

    /// <summary>
    /// Public method to update any framework relevant configuration change even if the GameObject is deactivated
    /// </summary>
    public void UpdateInfo()
    {
        if (_ignoreForTracking != ignoreForTracking)
        {
            _ignoreForTracking = ignoreForTracking;
            isDirty = true;
        }

        if (_isTargetStatic != isTargetStatic)
        {
            _isTargetStatic = isTargetStatic;
            isDirty = true;
        }
    }

    /// <summary>
    /// Apply TargetInformation information
    /// </summary>
    /// <param name="info">Target information to apply</param>
    public void UpdateGameObject(TargetInformation info)
    {
        targetInfo = info;
        isTargetInfoDirty = true;

        // Force deactivation of placeholder in case of MAP target.
        if (targetInfo.mType == TargetType.MAP)
            Placeholder.SetActive(false);

        if (targetInfo.mType == TargetType.MAP && hidePlaceholder)
        {
            MapPointVisualizer pointVisualizer = GetComponentInChildren<MapPointVisualizer>();
            if (pointVisualizer != null)
                pointVisualizer.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Set tracking information
    /// </summary>
    /// <param name="result">Latest tracking result</param>
    public void UpdateTrackingState(TrackerResult result) { lock (trackLock) target = result; }

    /// <summary>
    /// Sets the tracking state to NOT_TRACKED
    /// </summary>
    public void InvalidateLastResult() { target.mStatus = TrackingState.NOT_TRACKED; }
}
}
