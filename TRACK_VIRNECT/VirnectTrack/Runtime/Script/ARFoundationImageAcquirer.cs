using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

#if ARFOUNDATION_PRESENT
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace VIRNECT
{
    /// <summary>
    /// Provides access to the ARFoundation video image buffer.
    /// </summary>
    public class ARFoundationImageAcquirer : MonoBehaviour
    {
#if ARFOUNDATION_PRESENT
        /// <summary>
        /// FrameDataEvent event argument class
        /// </summary>
        public class FrameDataEventArgs : EventArgs
        {
            /// <summary>
            /// Byte type of frame buffer
            /// </summary>
            public NativeArray<byte> Buffer { get; private set; }

            /// <summary>
            /// Frame width
            /// </summary>
            public int Width { get; private set; }

            /// <summary>
            /// Frame height
            /// </summary>
            public int Height { get; private set; }

            /// <summary>
            /// Frame format
            /// </summary>
            public int Format { get; private set; }

            /// <summary>
            /// Constructor of FrameDataEventArgs
            /// </summary>
            /// <param name="buffer"> Native array type frame buffer</param>
            /// <param name="width"> Frame width </param>
            /// <param name="height"> Frame height </param>
            /// <param name="format"> Frame format </param>
            public FrameDataEventArgs(NativeArray<byte> buffer, int width, int height, int format)
            {
                Buffer = buffer;
                Width = width;
                Height = height;
                Format = format;
            }

        }

        /// <summary>
        /// Callback handler if new frame is available by AR Foundation
        /// </summary>
        public event EventHandler<FrameDataEventArgs> OnNewFrameDataAvailable = delegate { };

        /// <summary>
        /// Intrinsics event arguments class
        /// </summary>
        public class IntrinsicsEventArgs : EventArgs
        {
            /// <summary>
            /// ARFoundation's XRCameraIntrinsics
            /// </summary>
            public XRCameraIntrinsics Intrinsics { get; private set; }

            /// <summary>
            /// Constructor of IntrinsicsEventArgs
            /// </summary>
            /// <param name="intrinsics"></param>
            public IntrinsicsEventArgs(XRCameraIntrinsics intrinsics)
            {
                Intrinsics = intrinsics;
            }

        }

        /// <summary>
        /// Callback handler if new intrinsics are available by AR Foundation
        /// </summary>
        public event EventHandler<IntrinsicsEventArgs> OnIntrinsicsAvailable = delegate { };

        [SerializeField]
        [Tooltip("The ARCameraManager which will produce frame events.")]
        private ARCameraManager cameraManager;
        
        /// <summary>
        /// Frame buffer in this application.
        /// This buffer will be allocated in enable time and disposed in disable time
        /// </summary>
        private NativeArray<byte> frameBuffer;

        /// <summary>
        /// Set camera manager and enroll callback
        /// </summary>
        void OnEnable()
        {
            if (cameraManager == null) cameraManager = FindObjectOfType<ARCameraManager>();
            if (cameraManager != null) cameraManager.frameReceived += OnCameraFrameReceived;
        }

        /// <summary>
        /// Disable callback and dispose buffer
        /// </summary>
        void OnDisable()
        {
            if (cameraManager != null) cameraManager.frameReceived -= OnCameraFrameReceived;
            if (frameBuffer.IsCreated) frameBuffer.Dispose();
        }

        /// <summary>
        /// Called if new frame is available
        /// </summary>
        /// <param name="eventArgs"> Event arguments </param>
        private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            UpdateIntrinsics();
            
            if (ARSession.state != ARSessionState.SessionTracking) return;
            UpdateCameraImage();
        }

        /// <summary>
        /// Call intrinsics callback
        /// </summary>
        private void UpdateIntrinsics()
        {
            if (cameraManager.TryGetIntrinsics(out XRCameraIntrinsics intrinsics))
            {
                OnIntrinsicsAvailable(this, new IntrinsicsEventArgs(intrinsics));
            }
        }

        /// <summary>
        /// Get frame buffer from ARFoundation
        /// Allocate buffer using image size information and copy from XRCpuImage class
        /// Call OnNewFrameDataAvailable after copy flow
        /// </summary>
        void UpdateCameraImage()
        {
            // Attempt to get the latest camera image. If this method succeeds,
            // it acquires a native resource that must be disposed (see below).
            if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) return;

            if (!frameBuffer.IsCreated)
                // Create a buffer to accommodate YUV image data
                frameBuffer = new NativeArray<byte>((image.width * image.height * 2), Allocator.Persistent);

            int offset = 0;
            for (int i = 0; i < image.planeCount; i++)
            {
                XRCpuImage.Plane plane = image.GetPlane(i);
           
                unsafe
                {
                    // Faster than Native buffer copy function
                    UnsafeUtility.MemCpy(
                        (byte*)frameBuffer.GetUnsafePtr() + offset,
                        plane.data.GetUnsafeReadOnlyPtr(), 
                        plane.data.Count()
                    );
                }
                offset += plane.data.Count();
            }
         
            YUVFormat yuvFormat = image.GetPlane(1).pixelStride == 2 ? YUVFormat.YUV420SP : YUVFormat.YUV420P;

            OnNewFrameDataAvailable(this, new FrameDataEventArgs(frameBuffer, image.width, image.height, (int)yuvFormat));

            image.Dispose();
        }
#endif
    }
}
