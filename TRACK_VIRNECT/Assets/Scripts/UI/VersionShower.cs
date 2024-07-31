// Copyright (C) 2024 VIRNECT CO., LTD.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VIRNECT;

public class VersionShower : MonoBehaviour
{
    [SerializeField] Text versionText;

    /// <summary>
    /// Requests the version of the Track Library being used
    /// </summary>
    public void GetTrackVersion()
    {
        var version = LibraryInterface.GetFrameworkVersion();
        versionText.text = "Version: " + version;
    }

}
