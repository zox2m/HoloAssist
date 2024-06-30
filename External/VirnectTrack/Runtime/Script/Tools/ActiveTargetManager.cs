using System.Collections.Generic;
using UnityEngine;

namespace VIRNECT {
namespace TOOLS {
/// <summary>
/// Provides a debug UI to activate or deactivate (ignore) each individual target
/// </summary>
[ExecuteAlways]
public class ActiveTargetManager : MonoBehaviour
{
    /// <summary>
    /// Targets to show, will be filled with all scene targets if empty
    /// </summary>
    public TrackTarget[] targets;

    /// <summary>
    /// Width of the GUI button
    /// </summary>
    public int buttonWidth = 200;

    /// <summary>
    /// Height of the GUI button
    /// </summary>
    public int buttonHeight = 45;

    /// <summary>
    /// Vertical space between GUI buttons
    /// </summary>
    public int buttonSpace = 10;

    /// <summary>
    /// Indentation for left side
    /// </summary>
    public int buttonX = 10;

    /// <summary>
    /// Indentation from top
    /// </summary>
    public int buttonY = 10;

    /// <summary>
    /// Font size of buttons
    /// </summary>
    public int fontSize = 30;

    /// <summary>
    /// Since all measurements are in pixel, they can be easily scaled by this factor for Android
    /// </summary>
    public int androidMultiplyer = 3;

    /// <summary>
    /// Will load all scene targets if targets array is undefined
    /// </summary>
    public void Start()
    {
        if (targets.Length == 0)
        {
            // Get all scene targets
            TrackTarget[] targetObjects = Resources.FindObjectsOfTypeAll<TrackTarget>();
            List<TrackTarget> list = new List<TrackTarget>();
            foreach (TrackTarget target in targetObjects)
                if (target.gameObject.scene == gameObject.scene)
                    list.Add(target);
            targets = list.ToArray();
        }

#if !UNITY_EDITOR && UNITY_ANDROID
        buttonWidth *= androidMultiplyer;
        buttonHeight *= androidMultiplyer;
        buttonSpace *= androidMultiplyer;
        buttonX *= androidMultiplyer;
        fontSize *= androidMultiplyer;
#endif
    }

    /// <summary>
    /// Draw a button for each target.
    /// Clicking the button toggles the "ignore" state of a target
    ///
    /// The button color determines the state:
    /// Green  : Tracked
    /// Orange : Active but not tracked
    /// Red    : Deactivated for tracking
    /// </summary>
    public void OnGUI()
    {
        int y = buttonY;
        foreach (TrackTarget target in targets)
        {
            GUI.color = target.ignoreForTracking ? Color.red : (target.gameObject.activeSelf ? Color.green : Color.yellow);
            GUI.skin.button.fontSize = fontSize;
            if (GUI.Button(new Rect(buttonX, y, buttonWidth, buttonHeight), target.name))
                if (Application.IsPlaying(gameObject))
                    // Toggle ignore state
                    target.ignoreForTracking = !target.ignoreForTracking;
                else
                    // Button function is executed on editor focus, not on click
                    Debug.LogWarning("Buttons are deactivated in edit mode");
            y += buttonSpace + buttonHeight;
        }
    }
}

}
}
