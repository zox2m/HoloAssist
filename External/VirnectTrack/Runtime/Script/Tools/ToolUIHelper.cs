using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VIRNECT {
namespace TOOLS {
/// <summary>
/// This class contains variables for Tool UI
/// </summary>
[ExecuteAlways]
public class ToolUIHelper : MonoBehaviour
{
    [Header("Layout")]
    /// <summary>
    /// Since all measurements are in pixel, they can be easily scaled by this factor for Android
    /// </summary>
    public float size = 1;

    /// <summary>
    /// Since all measurements are in pixel, they can be easily scaled by this factor for Android
    /// </summary>
    public int androidMultiplyer = 3;

    /// <summary>
    /// Width of the GUI buttons
    /// </summary>
    private int _width = 220;

    /// <summary>
    /// Height of the GUI buttons
    /// </summary>
    private int _height = 35;

    /// <summary>
    /// Space to the right and bottom edge
    /// </summary>
    private int _space = 10;

    /// <summary>
    /// Font size
    /// </summary>
    private int _fontSize = 20;

    /// <summary>
    /// If the buttons should be attached to the left or right side of the screen
    /// </summary>
    public bool AnchorLeft = false;

    /// <summary>
    /// If the buttons should be attached to the top or bottom side of the screen
    /// </summary>
    public bool AnchorTop = false;

    /// <summary>
    /// Calculated X values dependent on scale
    /// </summary>
    protected int x;

    /// <summary>
    /// Calculated Y value dependent on scale
    /// </summary>
    protected int y;

    /// <summary>
    /// Calculated width dependent on scale
    /// </summary>
    protected int width;

    /// <summary>
    /// Calculated height dependent on scale
    /// </summary>
    protected int height;

    /// <summary>
    /// Calculated space dependent on scale
    /// </summary>
    protected int space;

    /// <summary>
    /// Calculated font size dependent on scale
    /// </summary>
    protected int fontSize;

    /// <summary>
    /// Simple size adjust for mobile interface
    /// </summary>
    public void Start()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        _width *= androidMultiplyer;
        _height *= androidMultiplyer;
        _space *= androidMultiplyer;
        _fontSize *= androidMultiplyer;
#endif
    }

    /// <summary>
    /// Adjust layout
    /// </summary>
    protected void Layout()
    {
        // Apply scaling
        width = (int)(size * _width);
        height = (int)(size * _height);
        space = (int)(size * _space);
        fontSize = (int)(size * _fontSize);

        // Adjust anchor position
        y = AnchorTop ? space : Screen.height - height - space;
        x = AnchorLeft ? space : Screen.width - width - space;
    }
}
}
}
