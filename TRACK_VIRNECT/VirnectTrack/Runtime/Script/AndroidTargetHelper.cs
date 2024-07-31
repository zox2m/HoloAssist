// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace VIRNECT
{
#if UNITY_ANDROID
    public class AndroidTargetHelper
    {
        /// <summary>
        /// To overcome file access limitations of assets packet into the APK,
        /// target files are copied from inside the APK to the application specific
        /// persistentDataPath in the normal file system
        /// Attention: blocking method
        /// </summary>
        /// <param name="targets">Targets to copy</param>
        /// <returns>Path to access copied targets</returns>
        public static string ExtractTargets(string[] targets)
        {
            // The main directory to save the target files
            string resultPath = Application.persistentDataPath + Constants.targetRootDirectoryAndroid;

            if (!Directory.Exists(resultPath))
                Directory.CreateDirectory(resultPath);

            // Process all targets
            foreach (string targetName in targets)
            {
                // Setup paths
                string fileName = targetName + Constants.targetFileExtension;
                string filePath = "jar:file://" + Application.dataPath + Constants.targetAPKDirectoryAndroid + fileName;
                string goalPath = resultPath + fileName;

                // Copy target files if it does not exist
                if (!File.Exists(goalPath))
                {
                    // Use UnityWebRequest to access files inside APK
                    UnityWebRequest request = UnityWebRequest.Get(filePath);
                    request.SendWebRequest();

                    // Wait until loaded
                    while (!request.isDone)
                        Debug.Log(request.downloadedBytes);

                    // Copy file if loaded correctly
                    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                        Debug.Log(request.error);
                    else
                        File.WriteAllBytes(goalPath, request.downloadHandler.data);
                }
            }

            // Prepare mapTargetFolder
            if (!Directory.Exists(Application.persistentDataPath + Constants.mapTargetRootDirectoryAndroid))
                Directory.CreateDirectory(Application.persistentDataPath + Constants.mapTargetRootDirectoryAndroid);

            return resultPath;
        }

        /// <summary>
        /// Helper method to check if a file exists in the APK
        /// Attention: loads the file to check existence
        /// Attention: blocking method
        /// </summary>
        /// <param name="filePath">file to check</param>
        /// <returns>If the target exists</returns>
        private static bool Exists(string filePath)
        {
            UnityWebRequest request = UnityWebRequest.Get(filePath);
            request.SendWebRequest();
            while (!request.isDone) { }
            return !(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError);
        }
    }

#endif

}
