using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VIRNECT {
/// <summary>
/// Abstract class that provides a TrackUpdate method thats controlled by the AsyncManager
/// </summary>
public abstract class TrackBehaviour : MonoBehaviour
{
    /// <summary>
    /// Virtual update method that is executed in a separate thread controlled by the AsyncManager
    /// GameObject components and Unity functions cannot be accessed directly in this method
    /// </summary>
    public abstract void TrackUpdate();
}

/// <summary>
/// Controller to execute all TrackBehaviour TrackUpdate functions in an independent thread
/// </summary>
public class AsyncManager : MonoBehaviour
{
    /// <summary>
    /// Stopwatch instance to measure time
    /// </summary>
    private static Stopwatch stopwatch;

    /// <summary>
    /// Returns the current time in milliseconds
    /// </summary>
    /// <returns>Time stamp in milliseconds</returns>
    private static double CT()
    {
        if (stopwatch == null)
        {
            // initialize StopWatch if null
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    /// <summary>
    /// Helper method to get all TrackBehaviour used in a scene
    /// </summary>
    /// <param name="scene">Scene to search</param>
    /// <returns>Dictionary of used TrackBehaviour in the scene</returns>
    private static List<TrackBehaviour> GetAllTrackBehaviours(Scene scene)
    {
        List<TrackBehaviour> targets = new List<TrackBehaviour>();
        TrackBehaviour[] targetObjects = Resources.FindObjectsOfTypeAll<TrackBehaviour>();
        foreach (TrackBehaviour target in targetObjects)
            if (target.gameObject.scene == scene)
                targets.Add(target);

        return targets;
    }

    /// <summary>
    /// List of all TrackBehaviour instances in the current scene
    /// </summary>
    private TrackBehaviour[] TrackBehaviours;

    /// <summary>
    /// Instance of the main thread
    /// </summary>
    private Thread thread = null;

    /// <summary>
    /// Runtime variable for the thread loop
    /// </summary>
    private bool execute = true;

    /// <summary>
    /// Target FPS for tread loop
    /// </summary>
    private int targetFPS = Constants.targetFrameRate;

    /// <summary>
    /// Initializes the AsyncManager and start thread execution
    /// </summary>
    void Start()
    {
        // Address all TrackBehaviour in the scene
        TrackBehaviours = GetAllTrackBehaviours(gameObject.scene).ToArray();

        // Start the thread
        execute = true;
        thread = new Thread(Run);
        thread.Name = "TRACK_MAIN";
        thread.IsBackground = true;
        thread.Start();
    }

    /// <summary>
    /// Stop execution of thread
    /// </summary>
    private void OnDestroy() { execute = false; }

    /// <summary>
    /// Helper to count the amount of executions of the main loop
    /// </summary>
    private int updateCounter = 0;

    /// <summary>
    /// Helper to calculate the time difference when measuring the main loop FPS
    /// </summary>
    private double lastTimeUpdated = 0;

    /// <summary>
    /// Resulting measurement of FPS for the main loop
    /// </summary>
    private int measuredThreadFPS = 0;

    /// <summary>
    /// Combined execution time of all registered TrackUpdate methods
    /// </summary>
    private double loadDuration = 0;

    /// <summary>
    /// Thread method containing the FPS controlled main loop
    /// </summary>
    private void Run()
    {
        while (execute)
        {
            // Save start of loop
            double start = CT();

            // Execute TrackUpdate method for each TrackBehaviour
            foreach (TrackBehaviour tb in TrackBehaviours)
                tb.TrackUpdate();

            // Simulate additional processing load in debug mode
            if (debugMode)
                Thread.Sleep(Math.Max(0, Math.Min(int.MaxValue, threadLoadSleep)));

            // Control FPS of loop
            double end = CT();
            loadDuration = end - start;

            // Sleep until the target frame time is reached
            Thread.Sleep(Math.Max(0, Math.Min(int.MaxValue, (int)((1000.0 / targetFPS) - loadDuration))));

            // Measure actual FPS of the update loop each second
            if (debugMode)
            {
                updateCounter++;
                if (end - lastTimeUpdated >= 1000.0)
                {
                    measuredThreadFPS = updateCounter;
                    lastTimeUpdated = end;
                    updateCounter = 0;
                }
            }
        }
        UnityEngine.Debug.Log(thread.Name + " THREAD ENDED");
    }

#region Debug

    /// <summary>
    /// Debug mode to enable OnGui output and simulate load on unity main update loop and internal loop
    /// </summary>
    private bool debugMode = false;

    /// <summary>
    /// Millisecond to simulate load on unity update method
    /// </summary>
    private int updateSleep = 10;

    /// <summary>
    /// Millisecond to simulate load on thread main loop
    /// </summary>
    private int threadLoadSleep = 10;

    /// <summary>
    /// Helper to buffer unity FPS measurement
    /// </summary>
    private double unityFPS = 0.0;

    /// <summary>
    /// Draw debug output
    /// </summary>
    void OnGUI()
    {
        if (!debugMode)
            return;

        if (GUI.Button(new Rect(5, 10, 120, 40), "FPS + 5"))
            targetFPS += 5;

        if (GUI.Button(new Rect(130, 10, 120, 40), "FPS - 5"))
            targetFPS -= 5;

        GUI.skin.label.fontSize = 20;
        GUI.color = Color.red;

        GUI.Label(new Rect(5, 60, 400, 40), $"Unity FPS: {Math.Round(unityFPS)}");
        GUI.Label(new Rect(5, 90, 400, 40), $"Framework FPS (target {targetFPS}): {measuredThreadFPS}");
        GUI.Label(new Rect(5, 120, 400, 40), $"Framework duration (ms): {Math.Round(loadDuration, 3)}");
    }

    /// <summary>
    /// Only needed to debug Unity FPS and simulate load on unity update thread
    /// </summary>
    void Update()
    {
        if (!debugMode)
            return;

        // FPS delta time
        unityFPS = 1.0 / Time.deltaTime;

        // Simulate unity load
        Thread.Sleep(Math.Max(0, Math.Min(int.MaxValue, updateSleep)));
    }

#endregion
}
}
