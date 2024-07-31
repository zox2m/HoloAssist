// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using System;
using UnityEngine;
using UnityEngine.UI;

namespace VIRNECT {

/// <summary>
/// The NotificationUI provides a static interface to display an UI message.
/// The spawning of the predefined UI element can be suppressed by registering
/// a callback function that will be invoked instead.
/// This enables the integration of these messages with custom UI elements.
/// </summary>
public class NotificationUI : MonoBehaviour
{
    /// <summary>
    /// Reference to user defined callback function
    /// </summary>
    private static Action<string, string, bool> CallbackFunction = null;

    /// <summary>
    /// Register a static callback function for the Notification UI
    /// The callback will be invoked, instead of spawning a predefined error message UI
    /// </summary>
    /// <param name="callback">Callback method taking a header and message string as well as and error indicator</param>
    public static void RegisterCallbackFunction(Action<string, string, bool> callback) { CallbackFunction = callback; }

    /// <summary>
    /// Spawns a notification UI prefab or invokes the callback function if registered
    /// </summary>
    /// <param name="header">Message header</param>
    /// <param name="message">Message content</param>
    /// <param name="error">Indicates if the message is an error. UI will have a red taint</param>
    public static void DisplayNotification(string header, string message, bool error = false)
    {
        // Call callback if registered
        if (CallbackFunction != null)
        {
            CallbackFunction(header, message, error);
            return;
        }

        // Spawn popup message prefab
        GameObject gameObject = (GameObject)Instantiate(Resources.Load(Constants.notificationPrefabPath));

        // Set values
        NotificationUI notification = gameObject.GetComponent<NotificationUI>();
        notification.Header.text = header;
        notification.Message.text = message;

        // Color outline if error
        if (error)
            notification.Outline.effectColor = new Color(0.7f, 0.1f, 0.1f, 0.9f);
    }

    /// <summary>
    /// Header of the notification
    /// </summary>
    public Text Header;

    /// <summary>
    /// Content of the notification
    /// </summary>
    public Text Message;

    /// <summary>
    /// Visual outline element to indicate error in red
    /// </summary>
    public Outline Outline;

    /// <summary>
    /// Public method to destroy the current GameObject
    /// </summary>
    public void Destroy() { Destroy(gameObject); }
}
}