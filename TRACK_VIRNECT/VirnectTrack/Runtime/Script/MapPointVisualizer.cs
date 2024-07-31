// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

namespace VIRNECT {
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Visualizes map points of a MAP target. Script needs to be attached to a child of a TrackTarget
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MapPointVisualizer : MonoBehaviour
{
    /// <summary>
    /// Point color
    /// </summary>
    [Tooltip("The color of the feature points.")]
    public Color PointColor;

    /// <summary>
    /// Point size
    /// </summary>
    private int defaultSize = 3;

    /// <summary>
    /// The mesh.
    /// </summary>
    private Mesh mesh;

    /// <summary>
    /// The mesh renderer.
    /// </summary>
    private MeshRenderer meshRenderer;

    /// <summary>
    /// The unique identifier for the shader _Color property.
    /// </summary>
    private int colorId;

    /// <summary>
    /// The property block.
    /// </summary>
    private MaterialPropertyBlock propertyBlock;

    /// <summary>
    /// The cached color of the points.
    /// </summary>
    private Color cachedColor;

    /// <summary>
    /// The cached feature points.
    /// </summary>
    private LinkedList<PointInfo> cachedPoints;

    /// <summary>
    /// Current map version
    /// </summary>
    private int version = 0;

    /// <summary>
    /// Initialize scene references
    /// </summary>
    public void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        mesh = GetComponent<MeshFilter>().mesh;

        if (mesh == null)
            mesh = new Mesh();

        mesh.Clear();

        cachedColor = PointColor;
        colorId = Shader.PropertyToID("_Color");

        propertyBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(colorId, cachedColor);
        meshRenderer.SetPropertyBlock(propertyBlock);

        cachedPoints = new LinkedList<PointInfo>();

        Resolution resolution = Screen.currentResolution;

        defaultSize *= resolution.height / 480;
    }

    /// <summary>
    /// The Unity Update() method.
    /// </summary>
    public void Update()
    {
        if (LibraryInterface.GetFrameworkState() != FrameworkState.RUNNING)
            return;

        if (cachedColor != PointColor)
            UpdateColor();

        AddPointsToCacheIfneeded();
    }

    /// <summary>
    /// Clears all cached feature points.
    /// </summary>
    private void ClearCachedPoints()
    {
        cachedPoints.Clear();
        mesh.Clear();
    }

    /// <summary>
    /// Updates the color of the feature points.
    /// </summary>
    private void UpdateColor()
    {
        cachedColor = PointColor;
        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", cachedColor);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    /// <summary>
    /// Adds points incrementally to the cache, by selecting points at random each frame.
    /// </summary>
    private void AddPointsToCacheIfneeded()
    {
        string targetName = GetComponentInParent<TrackTarget>().targetName;
        int version = LibraryInterface.GetMapPointsVersion(targetName);

        if (this.version >= version)
            return;

        this.version = version;

        cachedPoints.Clear();

        Vec3[] mapPoints = LibraryInterface.GetMapPoints(targetName);

        foreach (Vec3 point in mapPoints)
        {
            cachedPoints.AddLast(new PointInfo(new Vector3((float)point.X, (float)point.Y, (float)point.Z), new Vector2(defaultSize, defaultSize)));
        }

        UpdateMesh();
    }
    /// <summary>
    /// Updates the mesh, adding the feature points.
    /// </summary>
    private void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = cachedPoints.Select(p => p.Position).ToArray();
        mesh.uv = cachedPoints.Select(p => p.Size).ToArray();
        mesh.SetIndices(Enumerable.Range(0, cachedPoints.Count).ToArray(), MeshTopology.Points, 0);
    }

    /// <summary>
    /// Contains the information of a feature point.
    /// </summary>
    private struct PointInfo
    {
        /// <summary>
        /// The position of the point.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The size of the point.
        /// </summary>
        public Vector2 Size;

        public PointInfo(Vector3 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }
    }

    /// <summary>
    /// Scene sanitizer verification
    /// MapPointVisualizer needs to be child of target
    /// </summary>
    /// <param name="scene">Scene to analyze</param>
    /// <returns>Success or failure of verifying setup</returns>
    public static bool VerifySetup(Scene scene)
    {
        foreach (MapPointVisualizer visualizer in Resources.FindObjectsOfTypeAll<MapPointVisualizer>())
            if (visualizer.gameObject.scene == scene && visualizer.GetComponentInParent<TrackTarget>() == null)
            {
                Debug.LogError("MapPointVisualizer does not have a TrackTarget as parent.");
                return false;
            }

        return true;
    }
}
}
