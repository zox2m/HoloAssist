// Copyright (C) 2024 VIRNECT CO., LTD.
// All rights reserved.

#if URP_PRESENT

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VIRNECT
{
    /// <summary>
    /// A renderer feature for rendering the camera background for AR devices.
    /// </summary>
    public class TrackRendererFeature : ScriptableRendererFeature
    {
        static bool s_InitializedNearClipMesh;
        static Mesh s_NearClipMesh;

        /// <summary>
        /// A mesh that is placed near the near-clip plane
        /// </summary>
        internal static Mesh fullScreenNearClipMesh
        {
            get
            {
                if (!s_InitializedNearClipMesh)
                {
                    s_NearClipMesh = BuildFullscreenMesh(0.1f);
                    s_InitializedNearClipMesh = s_NearClipMesh != null;
                }

                return s_NearClipMesh;
            }
        }

        static Mesh BuildFullscreenMesh(float zVal)
        {
            const float bottomV = 0f;
            const float topV = 1f;
            var mesh = new Mesh
            {
                vertices = new Vector3[]
                {
                    new Vector3(0f, 0f, zVal),
                    new Vector3(0f, 1f, zVal),
                    new Vector3(1f, 1f, zVal),
                    new Vector3(1f, 0f, zVal),
                },
                uv = new Vector2[]
                {
                    new Vector2(0f, bottomV),
                    new Vector2(0f, topV),
                    new Vector2(1f, topV),
                    new Vector2(1f, bottomV),
                },
                triangles = new int[] { 0, 1, 2, 0, 2, 3 }
            };

            mesh.UploadMeshData(false);
            return mesh;
        }
        
        /// <summary>
        /// The scriptable render pass to be added to the renderer when the camera background is to be rendered.
        /// </summary>
        TrackBackgroundRenderPass trackBackgroundPass => m_trackBackgroundPass ??= new TrackBackgroundRenderPass();
        TrackBackgroundRenderPass m_trackBackgroundPass;


        /// <summary>
        /// Create the scriptable render pass.
        /// </summary>
        public override void Create() {

            if (TrackManager.Instance != null)
            {
                if (TrackManager.Instance.isFusionMode)
                {
                    SetActive(false);
                }
                else
                {
                    SetActive(true);
                }
            }

        }

        /// <summary>
        /// Add the background rendering pass when rendering a game camera with an enabled AR camera background component.
        /// </summary>
        /// <param name="renderer">The scriptable renderer in which to enqueue the render pass.</param>
        /// <param name="renderingData">Additional rendering data about the current state of rendering.</param>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var currentCamera = renderingData.cameraData.camera;
            if ((currentCamera != null) && (currentCamera.cameraType == CameraType.Game))
            {
                TrackCamera trackCamera = currentCamera.gameObject.GetComponent<TrackCamera>();
                if (trackCamera != null && trackCamera.mBackgroundMaterial != null)
                {
                    trackBackgroundPass.Setup(trackCamera);
                    renderer.EnqueuePass(trackBackgroundPass);
                }
            }
        }


        private class TrackBackgroundRenderPass : ScriptableRenderPass
        {
            /// <summary>
            /// The name for the custom render pass which will display in graphics debugging tools.
            /// </summary>
            const string k_CustomRenderPassName = "Track AR Background Pass (URP)";


            /// <summary>
            /// The projection matrix used to render the <see cref="mesh"/>.
            /// </summary>
            protected Matrix4x4 projectionMatrix { get; } = Matrix4x4.Ortho(0f, 1f, 0f, 1f, -0.1f, 9.9f);

            /// <summary>
            /// The <see cref="Mesh"/> used in this custom render pass.
            /// </summary>
            protected Mesh mesh { get; } = fullScreenNearClipMesh;

            /// <summary>
            /// Reference to the Track Camera instance
            /// </summary>
            TrackCamera trackCamera;


            /// <summary>
            /// Set up the background render pass.
            /// </summary>
            /// <param name="cameraBackground">The <see cref="ARCameraBackground"/> component that provides the <see cref="Material"/>
            /// and any additional rendering information required by the render pass.</param>
            /// <param name="invertCulling">Whether the culling mode should be inverted.</param>
            public void Setup(TrackCamera cam)
            {
                trackCamera = cam;
                ConfigureClear(ClearFlag.Depth, Color.clear);
                renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            }


            /// <summary>
            /// Execute the commands to render the camera background.
            /// </summary>
            /// <param name="context">The render context for executing the render commands.</param>
            /// <param name="renderingData">Additional rendering data about the current state of rendering.</param>
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cmd = CommandBufferPool.Get(k_CustomRenderPassName);
                cmd.BeginSample(k_CustomRenderPassName);

                cmd.IssuePluginEvent(LibraryInterface.GetRenderEventFunc(), 1);

                cmd.SetInvertCulling(false);
                cmd.SetViewProjectionMatrices(Matrix4x4.identity, projectionMatrix);
                cmd.DrawMesh(mesh, Matrix4x4.identity, trackCamera.mBackgroundMaterial, 0, 0);
                cmd.SetViewProjectionMatrices(renderingData.cameraData.camera.worldToCameraMatrix, renderingData.cameraData.camera.projectionMatrix);

                cmd.EndSample(k_CustomRenderPassName);

                context.ExecuteCommandBuffer(cmd);

                CommandBufferPool.Release(cmd);
            }
        }
    }
}
#endif // URP_PRESENT