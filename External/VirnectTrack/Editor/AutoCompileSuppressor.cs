using UnityEditor;
using UnityEngine;

namespace VIRNECT {
/// <summary>
/// This script disables autoReloading (recompiling) of resources
/// during playmode to prevent editor application crash
/// </summary>
[InitializeOnLoad]
public class AutoCompileSuppressor : MonoBehaviour
{
    /// <summary>
    /// Static constructor is executed when starting play mode
    /// </summary>
    static AutoCompileSuppressor()
    {
        // Register callback method for PlayMode changes
        EditorApplication.playModeStateChanged += PlayModeInterceptor;
    }

    /// <summary>
    /// PlayModeInterceptor is called by Unity right before entering PlayMode
    /// </summary>
    /// <param name="state">PlayModeStateChange indicates if the PlayMode starts or ends</param>
    private static void PlayModeInterceptor(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
            EditorApplication.LockReloadAssemblies();

        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.UnlockReloadAssemblies();
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
    }
}
}
