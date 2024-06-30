// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using System.Threading;
using UnityEngine;

namespace VIRNECT {
/// <summary>
/// Handles the license request
/// </summary>
public class LicenseManager
{
    /// <summary>
    /// Reset static request state on initialization
    /// </summary>
    static LicenseManager() { KeyState = KeyStates.NotSet; }

    /// <summary>
    /// Represents the state of the key
    /// </summary>
    public enum KeyStates
    {
        NotSet,         // No settings file or empty key
        Invalid,        // Key got declined, may also happen without Internet connection
        Valid,          // Key got validated
        PerformingCheck // A request is currently running
    }

    /// <summary>
    /// Current state of the key
    /// </summary>
    public static KeyStates KeyState { private set; get; }

    /// <summary>
    /// Checks the API key asynchronously, sets the internal keyState
    /// </summary>
    /// <param name="key">API key to check</param>
    public static void CheckAPIKey(string key)
    {
        // Handle empty key
        if (key.Length == 0)
        {
            KeyState = KeyStates.NotSet;
            return;
        }

        if (KeyState == KeyStates.PerformingCheck)
            return;

        // Do not access library when game is running
        if (Application.isPlaying)
            return;

        Thread t = new Thread(delegate() { PerformAsynchronRequest(key);
    });
    t.Start();
}

/// <summary>
/// Thread to check license key with blocking Library method
/// </summary>
/// <param name="key">Key to check</param>
private static void PerformAsynchronRequest(string key)
{
    KeyState = KeyStates.PerformingCheck;

    // Check if API key is valid
    if (LibraryInterface.SetLicenseKey(key))
        KeyState = KeyStates.Valid;
    else
        KeyState = KeyStates.Invalid;
}
}
}
