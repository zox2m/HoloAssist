// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace VIRNECT
{
    /// <summary>
    /// Camera module for handling preview frame and setting virtual camera of Unity3D
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class TrackCamera : MonoBehaviour
    {

#if UNITY_STANDALONE || UNITY_EDITOR
        // Experimental feature - hidden for public release
        // [Tooltip("When enabled, the USB camera automatically adapts to changing lighting conditions for optimal tracking. This option is used for Windows platforms only. It can not be changed during
        // runtime.")]
        public bool autoAdjustCameraSettings { get; private set; } = true;
#endif

        /// <summary>
        /// Main camera that we are using for AR
        /// </summary>
        private Camera mCamera;

        /// <summary>
        /// Command buffer for rendering texture on camera
        /// </summary>
        private CommandBuffer mCommandBuffer = null;

        /// <summary>
        /// Background material for texture
        /// </summary>
        public Material mBackgroundMaterial { get; private set; }

        /// <summary>
        /// Preview aspect ratio
        /// </summary>
        private float mRatio = 0;

        /// <summary>
        /// Preview size
        /// </summary>
        private Rect mPreviewSize = Rect.zero;

#if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>
    /// Current screen orientation
    /// </summary>
    private ScreenOrientation currentOrientation = ScreenOrientation.AutoRotation;
#endif
        /// <summary>
        /// Set background color to black
        /// </summary>
        private void Awake()
        {
            mCamera = GetComponent<Camera>();
            mCamera.clearFlags = CameraClearFlags.SolidColor;
            mCamera.backgroundColor = Color.black;

            // init camera transform to identity
            mCamera.transform.position = Vector3.zero;
            mCamera.transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Initialize VirnectCamera class
        /// </summary>
        /// <returns></returns>
        public IEnumerator Initialize()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // For generating texture from native
            GL.IssuePluginEvent(LibraryInterface.GetRenderEventFunc(), 0);
            yield return new WaitUntil(() => LibraryInterface.GetTextureID() != null);
#endif
            InitializeCameraCalibration();
            CreateTextureAndPassToPlugin();
            EnableBackgroundRendering();

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            yield return null;
        }

        /// <summary>
        /// Deinitialize VirnectCamera class
        /// </summary>
        private void OnDestroy()
        {
            mCamera.ResetProjectionMatrix();
            DisableBackgroundRendering();
        }
#if UNITY_PIPELINE_URP
        /// <summary>
        /// OnLateUpdate cycle for URP, call GetRenderEventFunc in order to draw background texture in native plugin
        /// </summary>
        private void LateUpdate()
        {
            // Only render if the framework is initialized and running
            if (!(TrackManager.Instance == null || !TrackManager.Instance.IsInitialized || !TrackManager.Instance.IsRunning))
            {
                UpdateUVCoordinate();

#if UNITY_ANDROID && !UNITY_EDITOR
            if (Screen.orientation == currentOrientation)
                return;

            UpdateCameraCalibration();
#endif
            }
        }
#else
        /// <summary>
        /// OnPreRender cycle, call GetRenderEventFunc in order to draw background texture in native plugin
        /// </summary>
        IEnumerator OnPreRender()
        {
            yield return new WaitForEndOfFrame();

            // Only render if the framework is initialized and running
            if (!(TrackManager.Instance == null || !TrackManager.Instance.IsInitialized || !TrackManager.Instance.IsRunning))
            {
                UpdateUVCoordinate();
                GL.IssuePluginEvent(LibraryInterface.GetRenderEventFunc(), 1);

#if UNITY_ANDROID && !UNITY_EDITOR
            if (Screen.orientation == currentOrientation)
                yield return null;

            UpdateCameraCalibration();
#endif
            }
            yield return null;
        }
#endif
        /// <summary>
        /// Update UV Coordinate for various screen sizes
        /// </summary>
        private void UpdateUVCoordinate()
        {
            // Escape if mBackgroundMaterial is not defined
            if (!mBackgroundMaterial)
                return;

#if UNITY_EDITOR || UNITY_STANDALONE

            float screenRatio = (float)Screen.width / (float)Screen.height;

            // Clamp image by width or height depending on aspect ratio
            if (mRatio < screenRatio)
            {
                // Clamp horizontally
                float aspectUVMargin = mRatio / screenRatio;
                mBackgroundMaterial.SetVector("_UvTopLeftRight", new Vector4(0, 0.5f + 0.5f * aspectUVMargin, 1, 0.5f + 0.5f * aspectUVMargin));
                mBackgroundMaterial.SetVector("_UvBottomLeftRight", new Vector4(0, 0.5f - 0.5f * aspectUVMargin, 1, 0.5f - 0.5f * aspectUVMargin));
            }
            else
            {
                // Clamp vertically
                float aspectUVMargin = screenRatio / mRatio;
                mBackgroundMaterial.SetVector("_UvTopLeftRight", new Vector4(0.5f - 0.5f * aspectUVMargin, 1, 0.5f + 0.5f * aspectUVMargin, 1));
                mBackgroundMaterial.SetVector("_UvBottomLeftRight", new Vector4(0.5f - 0.5f * aspectUVMargin, 0, 0.5f + 0.5f * aspectUVMargin, 0));
            }
#elif UNITY_ANDROID
        float screenRatio = (float)Screen.width / (float)Screen.height;

        // if Android is Portrait mode
        if (Screen.orientation == ScreenOrientation.Portrait)
            screenRatio = (float)Screen.height / (float)Screen.width;

        if (mRatio < screenRatio)
        {
            float aspectUVMargin = 0.5f * (mRatio / screenRatio);

            if (Screen.orientation == ScreenOrientation.LandscapeLeft)
            {
                mBackgroundMaterial.SetVector("_UvTopLeftRight", new Vector4(0, 0.5f + aspectUVMargin, 1, 0.5f + aspectUVMargin));
                mBackgroundMaterial.SetVector("_UvBottomLeftRight", new Vector4(0, 0.5f - aspectUVMargin, 1, 0.5f - aspectUVMargin));
            }
            else if (Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                mBackgroundMaterial.SetVector("_UvTopLeftRight", new Vector4(1, 0.5f - aspectUVMargin, 0, 0.5f - aspectUVMargin));
                mBackgroundMaterial.SetVector("_UvBottomLeftRight", new Vector4(1, 0.5f + aspectUVMargin, 0, 0.5f + aspectUVMargin));
            }
            else if (Screen.orientation == ScreenOrientation.Portrait)
            {
                mBackgroundMaterial.SetVector("_UvTopLeftRight", new Vector4(1, 0.5f + aspectUVMargin, 1, 0.5f - aspectUVMargin));
                mBackgroundMaterial.SetVector("_UvBottomLeftRight", new Vector4(0, 0.5f + aspectUVMargin, 0, 0.5f - aspectUVMargin));
            }
        }
        else
        {
            float aspectUVMargin = 0.5f * (screenRatio / mRatio);

            if (Screen.orientation == ScreenOrientation.LandscapeLeft)
            {
                mBackgroundMaterial.SetVector("_UvTopLeftRight", new Vector4(0.5f - aspectUVMargin, 1, 0.5f + aspectUVMargin, 1));
                mBackgroundMaterial.SetVector("_UvBottomLeftRight", new Vector4(0.5f - aspectUVMargin, 0, 0.5f + aspectUVMargin, 0));
            }
            else if (Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                mBackgroundMaterial.SetVector("_UvTopLeftRight", new Vector4(0.5f + aspectUVMargin, 0, 0.5f - aspectUVMargin, 0));
                mBackgroundMaterial.SetVector("_UvBottomLeftRight", new Vector4(0.5f + aspectUVMargin, 1, 0.5f - aspectUVMargin, 1));
            }
            else if (Screen.orientation == ScreenOrientation.Portrait)
            {
                mBackgroundMaterial.SetVector("_UvTopLeftRight", new Vector4(0.5f + aspectUVMargin, 1, 0.5f + aspectUVMargin, 0));
                mBackgroundMaterial.SetVector("_UvBottomLeftRight", new Vector4(0.5f - aspectUVMargin, 1, 0.5f - aspectUVMargin, 0));
            }
        }
#endif
        }

        /// <summary>
        /// Create new texture with BackgroundShader and pass its ID to native plugin
        /// </summary>
        private void CreateTextureAndPassToPlugin()
        {
            int[] previewSize = LibraryInterface.GetPreviewSize();

            mPreviewSize.width = previewSize[0];
            mPreviewSize.height = previewSize[1];

            mRatio = (float)previewSize[0] / previewSize[1];
#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_IOS
            Texture2D backgroundTexture = new Texture2D(previewSize[0], previewSize[1], TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            // Upload to GPU side
            backgroundTexture.Apply();

            // Pass texture ID to native plugin
            LibraryInterface.SetTextureFromUnity(backgroundTexture.GetNativeTexturePtr());

            mBackgroundMaterial = new Material(Shader.Find("VIRNECT/BackgroundShader"));
#elif UNITY_ANDROID
        Texture2D backgroundTexture = Texture2D.CreateExternalTexture(previewSize[0], previewSize[1], TextureFormat.RGBA32, false, false, LibraryInterface.GetTextureID());

        mBackgroundMaterial = new Material(Shader.Find("VIRNECT/BackgroundYUVShader"));
#endif
            mBackgroundMaterial.SetTexture("_MainTex", backgroundTexture);
        }

        /// <summary>
        /// For background rendering on Camera, CommandBuffer and Background material that is set in CreateTextureAndPassToPlugin function are used.
        /// </summary>
        private void EnableBackgroundRendering()
        {
            if (mCamera == null)
            {
                return;
            }

            mCamera.clearFlags = CameraClearFlags.Depth;
#if UNITY_PIPELINE_BUILTIN
            mCommandBuffer = new CommandBuffer();
            mCommandBuffer.Blit(null, BuiltinRenderTextureType.CameraTarget, mBackgroundMaterial);
            mCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, mCommandBuffer);
            mCamera.AddCommandBuffer(CameraEvent.BeforeGBuffer, mCommandBuffer);
#endif
        }

        /// <summary>
        /// Remove all command buffer from camera
        /// </summary>
        private void DisableBackgroundRendering()
        {
#if UNITY_PIPELINE_BUILTIN
            if (mCommandBuffer == null || mCamera == null)
                return;

            mCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, mCommandBuffer);
            mCamera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, mCommandBuffer);
#endif
        }

        /// <summary>
        /// Initialize Camera calibration
        /// </summary>
        private void InitializeCameraCalibration()
        {
            mCamera.usePhysicalProperties = true;
            mCamera.gateFit = Camera.GateFitMode.Fill;
            mCamera.nearClipPlane = 0.01f;
            mCamera.farClipPlane = 1000f;

            UpdateCameraCalibration();
        }

        /// <summary>
        /// Update Unity3D Virtual camera intrinsic based on Physical camera intrinsic parameter
        /// </summary>
        private void UpdateCameraCalibration()
        {
            // Retrieve calibration from tracking framework
            CameraCalibration calibration = LibraryInterface.GetCameraCalibration();

            // Lens shift using principal points
            float shiftX = -((float)calibration.mPrincipalPoint[0] - calibration.mResolution[0] / 2.0f) / calibration.mResolution[0];
            float shiftY = ((float)calibration.mPrincipalPoint[1] - calibration.mResolution[1] / 2.0f) / calibration.mResolution[1];

#if UNITY_EDITOR || UNITY_STANDALONE
            mCamera.sensorSize = new Vector2(calibration.mResolution[0], calibration.mResolution[1]);
            mCamera.focalLength = (float)calibration.mFocalLength[0];
            mCamera.lensShift = new Vector2(shiftX, shiftY);

#elif UNITY_ANDROID
        if (Screen.orientation == ScreenOrientation.LandscapeLeft)
        {
            mCamera.sensorSize = new Vector2(calibration.mResolution[0], calibration.mResolution[1]);
            mCamera.focalLength = (float)calibration.mFocalLength[0];
            mCamera.lensShift = new Vector2(shiftX, shiftY);
        }
        else if (Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            mCamera.sensorSize = new Vector2(calibration.mResolution[0], calibration.mResolution[1]);
            mCamera.focalLength = (float)calibration.mFocalLength[0];
            mCamera.lensShift = new Vector2(-shiftX, -shiftY);
        }
        else
        {
            mCamera.sensorSize = new Vector2(calibration.mResolution[1], calibration.mResolution[0]);
            mCamera.focalLength = (float)calibration.mFocalLength[1];
            mCamera.lensShift = new Vector2(shiftY, -shiftX);
        }

        currentOrientation = Screen.orientation;
#endif
        }
    }
}