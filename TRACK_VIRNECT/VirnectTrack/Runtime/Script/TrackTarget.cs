// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace VIRNECT
{
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
        /// Possible ways the target contents can be made visible
        /// </summary>
        private enum EnableMethod { SetActive, Components }

        /// <summary>
        /// Currently the method for making the target contents visible is by enabling and disabling components
        /// </summary>
        private EnableMethod enableMethod = EnableMethod.Components;

        /// <summary>
        /// Saves the state of the tracking of this target during last frame, to know if an onStartTracking, or onStoppedTRacking event should be called
        /// </summary>
        private bool lastFrameTrackingState = false;

        /// <summary>
        /// Event called when the target is found by the framework and tracking starts
        /// </summary>
        public UnityEvent onTrackingStarted;

        /// <summary>
        /// Event called when the target is lost and the framework stops tracking it
        /// </summary>
        public UnityEvent onTrackingStopped;

        /// <summary>
        /// Pose is in stable status after pose updated 10 times
        /// </summary>
        public bool isStablePose
        {
            get
            {
                if (isTargetStatic && target.mStatus == TrackingState.TRACKED)
                    return poseUpdateCount > 10;
                return false;
            }
        }

        /// <summary>
        /// Pose update count
        /// </summary>
        private int poseUpdateCount = 0;

        /// <summary>
        /// Indicates if the isStatic property was true on initialization
        /// </summary>
        bool initializedAsStatic = false;

        /// <summary>
        /// Sets reference to placeholder and applies visibility
        /// </summary>
        void Start()
        {
            MeshRenderer placeholder = GetComponentsInChildren<MeshRenderer>().FirstOrDefault<MeshRenderer>(r => r.CompareTag(Constants.targetTag));
            Placeholder = placeholder.gameObject;
            Placeholder.SetActive(!hidePlaceholder);

            initializedAsStatic = isTargetStatic;
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
                            (targetInfo.mType == TargetType.QRCODE || targetInfo.mType == TargetType.IMAGE)? 0.001f : (float)(targetInfo.mDimensions.mMax.Z - targetInfo.mDimensions.mMin.Z));
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
                EnableTarget(active);

                if (!active){
                    poseUpdateCount = 0;
                    return;
                }

                if(!isStablePose){
                    TrackManager.Instance.UpdateTransform(transform, target);
                    if(TrackManager.Instance.isFusionMode && isTargetStatic)
                        poseUpdateCount++;
                }

                if(initializedAsStatic)
                {
                    isTargetStatic = true;
                    initializedAsStatic = !isStablePose;
                }


            }
        }

        /// <summary>
        /// Makes the contents of the target visible (or invisible) using the currently selected method.
        /// Also calls the onTrackingStarted and onTrackingStopped events
        /// </summary>
        /// <param name="active">Whether to show or hide the target's contents</param>
        private void EnableTarget(bool active)
        {
            switch (enableMethod)
            {
                case EnableMethod.SetActive:
                    gameObject.SetActive(active);
                    break;

                case EnableMethod.Components:
                    gameObject.SetActive(true);
                    foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true)) renderer.enabled = active;
                    foreach (var collider in gameObject.GetComponentsInChildren<Collider>(true)) collider.enabled = active;
                    foreach (var canvas in gameObject.GetComponentsInChildren<Canvas>(true)) canvas.enabled = active;
                    break;

            }

            // Check if any events were triggered during this frame
            CallTrackingStateEvents(active);
        }

        /// <summary>
        /// Based on the current state, and the previous call's state, the corresponding events will be called
        /// </summary>
        /// <param name="targetTracked">if this target is being tracked during this call</param>
        private void CallTrackingStateEvents(bool targetTracked)
        {
            if (targetTracked && !lastFrameTrackingState)
            {
                onTrackingStarted.Invoke();
            }
            else if (!targetTracked && lastFrameTrackingState)
            {
                onTrackingStopped.Invoke();
            }

            lastFrameTrackingState = targetTracked;
        }

        /// <summary>
        /// Public method to update any framework relevant configuration change even if the GameObject is deactivated
        /// </summary>
        public void UpdateInfo()
        {
            if(TrackManager.Instance.isFusionMode){
                // Debug.LogWarning("UpdateInfo can not be called in Fusion mode.");
                return;
            }

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
