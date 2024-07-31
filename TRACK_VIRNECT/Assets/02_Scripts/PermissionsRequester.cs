// Copyright (C) 2024 VIRNECT CO., LTD.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

public class PermissionsRequester : MonoBehaviour
{
    [SerializeField] UnityEvent onAllPermissionsGranted;


    bool canContinue = false;   // flag to control coroutine flow

    void Awake()
    {
#if UNITY_ANDROID
        StartCoroutine(RequestCameraPermissionSequence());
#else
        // call what ever was assigned as an event at the end
        onAllPermissionsGranted.Invoke();
#endif
    }

#if UNITY_ANDROID
    /// <summary>
    /// Function to check if user has already granted permission to use the camera, and if not request it
    /// </summary>
    private IEnumerator RequestCameraPermissionSequence()
    {
        // Check if user has already granted the permission to access the camera
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += QuitAppCallback;
            callbacks.PermissionGranted += CanContinueCallback;
            callbacks.PermissionDeniedAndDontAskAgain += QuitAppCallback;
            canContinue = false;
            Permission.RequestUserPermission(Permission.Camera, callbacks);
            yield return new WaitUntil(() => canContinue);
        }

        // Check if user has already granted the permission for external storage
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += QuitAppCallback;
            callbacks.PermissionGranted += CanContinueCallback;
            callbacks.PermissionDeniedAndDontAskAgain += QuitAppCallback;
            canContinue = false;
            Permission.RequestUserPermission(Permission.ExternalStorageRead, callbacks);
            yield return new WaitUntil(() => canContinue);
        }

        // Check if user has already granted the permission for external storage
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += QuitAppCallback;
            callbacks.PermissionGranted += CanContinueCallback;
            callbacks.PermissionDeniedAndDontAskAgain += QuitAppCallback;
            canContinue = false;
            Permission.RequestUserPermission(Permission.ExternalStorageWrite, callbacks);
            yield return new WaitUntil(() => canContinue);
        }

        // call what ever was asssigned as an event at the end
        onAllPermissionsGranted.Invoke();
    }

    /// <summary>
    /// Called when the permision was granted and we are allowed to continue with the next one
    /// </summary>
    internal void CanContinueCallback(string permissionType)
    {
        canContinue = true;
    }
    
    /// <summary>
    /// Called when the permission was not granted and we have to quit the app
    /// </summary>
    internal void QuitAppCallback(string permissionType)
    {
        ShowAndroidToastMessage(permissionType + " not granted, quitting");
        Application.Quit();
    }

    /// <summary>
    /// Shows an Android Toast on the UI
    /// <param name="message">Message string to show in the toast.</param>
    /// </summary>
    private void ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
#endif
}
