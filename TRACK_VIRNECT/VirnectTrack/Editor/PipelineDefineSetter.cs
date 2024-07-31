// Copyright (C) 2024 VIRNECT CO., LTD.
// All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine.Rendering;

namespace VIRNECT
{
    /// <summary>
    /// The PipelineDefineSetter runs when Unity loads or scripts are recompiled.
    /// It determines the current render pipeline and records it as a preprocessor definition in the project settings.
    /// Other scripts can then use these definitions to control what code is included depending on the render pipeline.
    /// </summary>
    [InitializeOnLoad]
    public class PipelineDefineSetter
    {
        enum PipelineType
        {
            Unsupported,    ///< Pipeline type is not known or supported
            BuiltIn,        ///< Built-In Render Pipeline
            URP,            ///< Universal Render Pipeline
            HDRP            ///< High-Definition Render Pipeline
        }

        // The string literals to use for each pipeline definition.
        private const string BUILTIN_DEFINE = "UNITY_PIPELINE_BUILTIN";
        private const string URP_DEFINE = "UNITY_PIPELINE_URP";
        private const string HDRP_DEFINE = "UNITY_PIPELINE_HDRP";

        /// <summary>
        /// Static constructor will run immediately, detect the render pipeline, and apply the correct preprocessor definition.
        /// </summary>
        static PipelineDefineSetter()
        {
            UpdateDefines();
        }

        /// <summary>
        /// Update the preprocessor definitions based on the current render pipeline.
        /// </summary>
        private static void UpdateDefines()
        { 
            switch (GetPipelineType())
            {
                case PipelineType.BuiltIn:
                    RemoveDefines(URP_DEFINE, HDRP_DEFINE);
                    AddDefines(BUILTIN_DEFINE);                    
                    break;
                case PipelineType.URP:
                    RemoveDefines(BUILTIN_DEFINE, HDRP_DEFINE);
                    AddDefines(URP_DEFINE);                    
                    break;
                case PipelineType.HDRP:
                    RemoveDefines(BUILTIN_DEFINE, URP_DEFINE);
                    AddDefines(HDRP_DEFINE);
                    break;
                case PipelineType.Unsupported:
                    RemoveDefines(BUILTIN_DEFINE, URP_DEFINE, HDRP_DEFINE);
                    break;
            }
        }

        /// <summary>
        /// Returns the type of renderpipeline that is currently running
        /// </summary>
        /// <returns></returns>
        private static PipelineType GetPipelineType()
        {
            if (GraphicsSettings.renderPipelineAsset == null)
            {
                // If no renderPipelineAsset is assigned then the built-in render pipeline is active.
                return PipelineType.BuiltIn;
            }
            else 
            { 
                // Check for HDRP
                var pipelineType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
                if (pipelineType.Contains("HDRenderPipelineAsset")) return PipelineType.HDRP;

                // Check for URP (LWRP was replaced by URP in Unity 2019.3)
                if (pipelineType.Contains("UniversalRenderPipelineAsset") || pipelineType.Contains("LightweightRenderPipelineAsset")) return PipelineType.URP;

                // Unsupported type: renderPipelineAsset exists, but is not URP or HDRP.
                return PipelineType.Unsupported;
            }
        }

        /// <summary>
        /// Adds one or more custom definitions.
        /// </summary>
        /// <param name="define">The definitions to add.</param>
        private static void AddDefines(params string[] definesToAdd)
        {
            var defines = GetDefines();
            foreach (string s in definesToAdd) defines.Add(s);
            SetDefines(defines);
        }

        /// <summary>
        /// Removes one or more custom definitions.
        /// </summary>
        /// <param name="define">The definitions to remove.</param>
        private static void RemoveDefines(params string[] definesToRemove)
        {
            var defines = GetDefines();
            defines.RemoveWhere(s => definesToRemove.Contains(s));
            SetDefines(defines);
        }

#if UNITY_2021_2_OR_NEWER
        private static NamedBuildTarget ActiveNamedBuildTarget => NamedBuildTarget.FromBuildTargetGroup(ActiveBuildTargetGroup);
#endif
        private static BuildTargetGroup ActiveBuildTargetGroup => BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

        /// <summary>
        /// Returns the current set of preprocessor definitions.
        /// </summary>
        /// <returns>Set of preprocessor definitions.</returns>
        private static HashSet<string> GetDefines()
        {
#if UNITY_2021_2_OR_NEWER
            PlayerSettings.GetScriptingDefineSymbols(ActiveNamedBuildTarget, out var defines);
#elif UNITY_2020_OR_NEWER
            PlayerSettings.GetScriptingDefineSymbolsForGroup(ActiveBuildTargetGroup, out var defines);
#else
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(ActiveBuildTargetGroup);
            var defines = symbols.Split(';');
#endif
            return defines.ToHashSet();
        }

        /// <summary>
        /// Sets the current set of preprocessor definitions.
        /// </summary>
        /// <param name="="definesSet">Set of preprocessor definitions to apply.</param>
        private static void SetDefines(HashSet<string> definesSet)
        {
#if UNITY_2021_2_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(ActiveNamedBuildTarget, definesSet.ToArray());
#elif UNITY_2020_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbolsForGroup(ActiveBuildTargetGroup, definesSet.ToArray());
#else
            var defines = string.Join(";", definesSet);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(ActiveBuildTargetGroup, defines);        
#endif
        }
    }
}