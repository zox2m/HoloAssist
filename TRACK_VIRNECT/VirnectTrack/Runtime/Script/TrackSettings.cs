// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using UnityEngine;

namespace VIRNECT {
/// <summary>
/// Data container to hold all global framework settings
/// </summary>
public class TrackSettings : ScriptableObject
{
    /// <summary>
    /// License key for this project
    /// </summary>
    public string LicenseKey = "";

    /// <summary>
    /// If the BuildSanitizer should check all scenes
    /// </summary>
    public bool PerformPreBuildSceneCheck = true;
}

}