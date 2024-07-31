using System.Collections.Generic;
using UnityEngine;

namespace VIRNECT {
namespace TOOLS {
/// <summary>
/// Provides a debug UI to specify an individual target as static in the scene
/// </summary>
[ExecuteAlways]
public class StaticTargetManager : MonoBehaviour
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
    public int buttonX = 220;

    /// <summary>
    /// Indentation from top
    /// </summary>
    public int buttonY = 10;

    /// <summary>
    /// Font size of buttons
    /// </summary>
    public int fontSize = 15;

    /// <summary>
    /// Since all measurements are in pixels, they can be easily scaled by this factor for Android
    /// </summary>
    public int androidMultiplier = 3;

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
        buttonWidth *= androidMultiplier;
        buttonHeight *= androidMultiplier;
        buttonSpace *= androidMultiplier;
        buttonX *= androidMultiplier;
        fontSize *= androidMultiplier;
#endif
    }

    /// <summary>
    /// Draw a button for each target.
    /// Clicking the button toggles the "static" state of a target
    ///
    /// The button color determines the state:
    /// Red    : Target is movable (non-static) in the scene
    /// Green  : Target is Static in the scene
    /// </summary>
    public void OnGUI()
    {
        int y = buttonY;
        foreach (TrackTarget target in targets)
        {
            GUI.color = target.isTargetStatic ? Color.green : Color.red;
            GUI.skin.button.fontSize = fontSize;
            if (GUI.Button(new Rect(buttonX, y, buttonWidth, buttonHeight), target.name))
                if (Application.IsPlaying(gameObject))
                    // Toggle static state
                    target.isTargetStatic = !target.isTargetStatic;
                else
                    // Button function is executed on editor focus, not on click
                    Debug.LogWarning("Buttons are deactivated in edit mode");
            y += buttonSpace + buttonHeight;
        }
    }
}

}
}
