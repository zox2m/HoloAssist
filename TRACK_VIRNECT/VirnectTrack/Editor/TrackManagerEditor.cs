using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ARFOUNDATION_PRESENT
using UnityEngine.XR.ARFoundation;
#endif

namespace VIRNECT
{
    /// <summary>
    /// Editor preference for Track manager
    /// </summary>
    [CustomEditor(typeof(TrackManager))]
    public class TrackManagerEditor : Editor
    {
        // References to serialized fields
        SerializedProperty camera;
        SerializedProperty cameraID;
        SerializedProperty isFusionMode;
        SerializedProperty imageSource;
        SerializedProperty fileSquencePath;
        SerializedProperty fileSequenceCalibrationPath;
        SerializedProperty externalImageSourceInterface;
        SerializedProperty calibrationJSONresolution;

        string[] imageSourceNames;
        string[] usbCameraNames;
        int imageSourceDropdownIndex = 0;
        int cameraNameDropdownIndex = 0;
        int cameraResolutionDropdownIndex = 1;

        bool calibrationFileExists = false;
        bool calibrationFileContainsResolution = false;

        /// <summary>
        /// Sets usbCameraNames via LibraryInterface
        /// </summary>
        void UpdateCameraNames() { usbCameraNames = LibraryInterface.GetDeviceNames(); }

        /// <summary>
        /// Prepare connection to serializedObject
        /// </summary>
        void OnEnable()
        {
            // Load data reference to entity
            camera = serializedObject.FindProperty("ARCamera");
            cameraID = serializedObject.FindProperty("cameraID");
            isFusionMode = serializedObject.FindProperty("isFusionMode");
            imageSource = serializedObject.FindProperty("imageSource");
            fileSquencePath = serializedObject.FindProperty("fileSequencePath");
            fileSequenceCalibrationPath = serializedObject.FindProperty("fileSequenceCalibrationPath");
            externalImageSourceInterface = serializedObject.FindProperty("externalImageSourceInterface");
            calibrationJSONresolution = serializedObject.FindProperty("calibrationJSONresolution");

            imageSourceNames = Enum.GetNames(typeof(TrackManager.ImageSources));
            imageSourceDropdownIndex = imageSource.intValue;

            cameraResolutionDropdownIndex = Array.IndexOf(EditorCommons.resolutions, calibrationJSONresolution.stringValue);
            EvaluateCalibration(cameraResolutionDropdownIndex);

            UpdateCameraNames();
            cameraNameDropdownIndex = cameraID.intValue;
        }

        /// <summary>
        /// Checks if calibration file exists and given index is present in the calibration file.
        /// </summary>
        /// <param name="index">Index of resolution represented in EditorCommons.resolutions list</param>
        /// <returns>True if both conditions are fulfilled</returns>
        bool EvaluateCalibration(int index)
        {
            // Guard file access for UI feedback
            try
            {
                string calibrationPath = Application.dataPath + Constants.calibrationPath;
                calibrationFileExists = File.Exists(calibrationPath);
                calibrationFileContainsResolution = LibraryInterface.LoadCameraCalibration(calibrationPath, "cam0", EditorCommons.resolutions[index]);
            }
            catch
            {
                calibrationFileExists = false;
                calibrationFileContainsResolution = false;
            }
            return calibrationFileExists && calibrationFileContainsResolution;
        }

        /// <summary>
        /// Custom UI, alerts the user when no camera is attached, or the attached camera has no TrackCamera script attached
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUILayout.BeginVertical();
            
#if ARFOUNDATION_PRESENT
#if UNITY_IOS
            EditorGUI.BeginDisabledGroup(true);
            isFusionMode.boolValue = EditorGUILayout.Toggle("Fusion mode", true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.HelpBox("iOS target only supports fusion mode", MessageType.Info, false);
#else
            isFusionMode.boolValue = EditorGUILayout.Toggle("Fusion mode", isFusionMode.boolValue);
#endif
#elif UNITY_IOS
            EditorGUILayout.HelpBox("ARFoundation should be installed if target platform is iOS", MessageType.Error, false);
#endif
            // Camera reference
            EditorGUILayout.PropertyField(camera, new GUIContent("Scene Camera"));

            // Alert if empty
            if (camera.objectReferenceValue == null)
                EditorGUILayout.HelpBox("Please reference the main scene camera.", MessageType.Error, false);
            else
            {
                // Alert if no TrackCamera component is attached
                Camera c = (Camera)camera.objectReferenceValue;
#if ARFOUNDATION_PRESENT
                if (isFusionMode.boolValue)
                {
                    if (c.GetComponent<ARCameraManager>() == null)
                        // Alert when no script attached
                        EditorGUILayout.HelpBox("The referenced camera has no ARCameraManager script attached!", MessageType.Error, false);
                }
                else
#endif
                {
                    if (c.GetComponent<TrackCamera>() == null)
                    {
                        // Alert when no script attached
                        EditorGUILayout.HelpBox("The referenced camera has no TrackCamera script attached!", MessageType.Error, false);
                        if (GUILayout.Button("Add TrackCamera script to \"" + c.gameObject.name + "\"", EditorStyles.miniButton))
                        {
                            c.gameObject.AddComponent<TrackCamera>();
                            EditorUtility.SetDirty(c);
                        }
                    }
                }
            }

#if ARFOUNDATION_PRESENT
            if (isFusionMode.boolValue)
            {
                GUILayout.EndVertical();
                // Apply values
                serializedObject.ApplyModifiedProperties();
                return;
            }
#endif

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Image Source");

            // ImageSource DropDown
            int indexNew = EditorGUILayout.Popup("ImageSource", imageSourceDropdownIndex, imageSourceNames);
            if (indexNew != imageSourceDropdownIndex)
            {
                imageSource.enumValueIndex = indexNew;
                imageSourceDropdownIndex = indexNew;
                serializedObject.ApplyModifiedProperties();
            }

            // ImageSource specific UI
            if ((TrackManager.ImageSources)imageSource.enumValueIndex == TrackManager.ImageSources.Camera)
            {
                // USB Camera Name dropdown
                if (usbCameraNames.Length == 0 || usbCameraNames[0] == null)
                {
                    EditorGUILayout.HelpBox("No USB Camera available", MessageType.Error, true);
                }
                else
                {
                    int indexCameraID = EditorGUILayout.Popup("USB Camera", cameraNameDropdownIndex, usbCameraNames);
                    if (indexCameraID != cameraNameDropdownIndex)
                    {
                        cameraID.intValue = indexCameraID;
                        cameraNameDropdownIndex = indexCameraID;
                        serializedObject.ApplyModifiedProperties();
                        UpdateCameraNames();
                    }

                    // Show info for Android
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                        EditorGUILayout.HelpBox("The selected USB camera only applies to the Editor.\nOn ANDROID the back-facing camera will be used.", MessageType.Info, false);
                }

                // Resolution DropDown
                int indexRes = EditorGUILayout.Popup("Resolution", cameraResolutionDropdownIndex, EditorCommons.resolutions);
                if (indexRes != cameraResolutionDropdownIndex)
                {
                    calibrationJSONresolution.stringValue = EditorCommons.resolutions[indexRes];
                    cameraResolutionDropdownIndex = indexRes;
                    serializedObject.ApplyModifiedProperties();
                    if (!EvaluateCalibration(indexRes))
                        EditorGUILayout.HelpBox("Selected resolution is not defined in the calibration.json", MessageType.Error, false);
                }

                if (!calibrationFileExists)
                    EditorGUILayout.HelpBox("Camera calibration file \"" + Constants.calibrationPath + "\" does not exist", MessageType.Error, false);
                else if (!calibrationFileContainsResolution)
                    EditorGUILayout.HelpBox("Selected camera resolution is not defined in the calibration file.", MessageType.Error, false);
            }
            else if ((TrackManager.ImageSources)imageSource.enumValueIndex == TrackManager.ImageSources.FileSequence)
            {
                // FileSequence UI

                // Not on Android warning
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                    EditorGUILayout.HelpBox("The file sequence reader is not available on mobile platforms. \nOn ANDROID the back-facing camera is used.", MessageType.Warning, false);

                // Path input field
                EditorGUILayout.PropertyField(fileSquencePath, new GUIContent("File Sequence Path"));

                // Path feedback
                if (fileSquencePath.stringValue.Length == 0)
                    EditorGUILayout.HelpBox("Please provide a File Sequence Path", MessageType.Error, false);
                else
                {
                    if (!Directory.Exists(fileSquencePath.stringValue))
                        EditorGUILayout.HelpBox("The given path does not exist", MessageType.Error, false);
                    else if (Directory.GetFiles(fileSquencePath.stringValue).Length == 0)
                        EditorGUILayout.HelpBox("The given path is empty", MessageType.Error, false);
                    else
                        serializedObject.ApplyModifiedProperties();
                }

                // Calibration
                EditorGUILayout.PropertyField(fileSequenceCalibrationPath, new GUIContent("File Sequence Calibration Path"));
                // Path feedback
                if (fileSequenceCalibrationPath.stringValue.Length == 0)
                    EditorGUILayout.HelpBox("Please provide a calibration file for this specific sequence. Otherwise your USB camera calibration will be used. The image might not be displayed correctly.",
                                            MessageType.Warning, false);
                else if (!File.Exists(fileSequenceCalibrationPath.stringValue))
                    EditorGUILayout.HelpBox("The given file does not exist", MessageType.Error, false);
                else if (Path.GetExtension(fileSequenceCalibrationPath.stringValue) != ".json")
                    EditorGUILayout.HelpBox("The given file is not a JSON file", MessageType.Error, false);
                else
                    serializedObject.ApplyModifiedProperties();
            }
            else if ((TrackManager.ImageSources)imageSource.enumValueIndex == TrackManager.ImageSources.ExternalScript)
            {
                EditorGUILayout.PropertyField(externalImageSourceInterface, new GUIContent("ExternalImageSourceInterface"));

                if (externalImageSourceInterface.objectReferenceValue == null || ((GameObject)externalImageSourceInterface.objectReferenceValue).GetComponent<ExternalImageSourceInterface>() == null)
                    EditorGUILayout.HelpBox("Requires a GameObject with a component implementing the 'ExternalImageSourceInterface'", MessageType.Error, false);
            }

            GUILayout.EndVertical();
            // Apply values
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Helper method to get all TrackManager used in a scene
        /// </summary>
        /// <param name="scene">Scene to search</param>
        /// <returns>List of used TrackManager in scene</returns>
        public static List<TrackManager> GetAllSceneManagers(Scene scene)
        {
            List<TrackManager> managers = new List<TrackManager>();
            TrackManager[] targetObjects = Resources.FindObjectsOfTypeAll<TrackManager>();
            foreach (TrackManager manager in targetObjects)
                if (manager.gameObject.scene == scene)
                    managers.Add(manager);

            return managers;
        }

        /// <summary>
        /// Verifies the correct configuration of the given manager
        /// </summary>
        /// <param name="manager">Manager script to check</param>
        /// <returns>Error string if error detected</returns>
        internal static string VerifyConfiguration(TrackManager manager)
        {
            // Ensure camera is attached
            Camera camera = manager.ARCamera;
            if (camera == null)
            {
                return "\"" + manager.gameObject.name + " \" has no scene camera attached";
            }
#if ARFOUNDATION_PRESENT
            if (manager.isFusionMode)
            {
                // Ensure that referenced camera has ARFoundation's ARCameraManager script attached
                ARCameraManager cameraScript = camera.GetComponent<ARCameraManager>();
                if (cameraScript == null)
                    return "\"" + manager.gameObject.name + " \" referenced camera has no \"ARCameraManager\" component attached.";
            }
            else
#endif
            {
                // Ensure that referenced camera has VirnectCamera script attached
                TrackCamera cameraScript = camera.GetComponent<TrackCamera>();
                if (cameraScript == null)
                    return "\"" + manager.gameObject.name + " \" referenced camera has no \"TrackCamera\" component attached.";
            }

            // All checks passed
            return null;
        }
    }
}
