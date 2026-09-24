using System;
using System.Collections.Generic;
using GGJ.Utility;
using GGJ.Utility.Extensions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace GGJ.Rendering.Portals
{
    public class PortalRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] PortalRenderFeatureSettings settings;
        PortalRenderFeaturePass _scriptablePass;
        
        /// <inheritdoc/>
        public override void Create()
        {
            _scriptablePass = new PortalRenderFeaturePass(settings);

            // Configures where the render pass should be injected.
            _scriptablePass.renderPassEvent = settings.renderPassEvent;

            // You can request URP color texture and depth buffer as inputs by uncommenting the line below,
            // URP will ensure copies of these resources are available for sampling before executing the render pass.
            // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
            //m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);

            // You can request URP to render to an intermediate texture by uncommenting the line below.
            // Use this option for passes that do not support rendering directly to the backbuffer.
            // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
            //m_ScriptablePass.requiresIntermediateTexture = true;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_scriptablePass);
        }

        // Use this class to pass around settings from the feature to the pass
        [Serializable]
        public class PortalRenderFeatureSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPrePasses;
            [Range(1, 20)] public int maxIterations = 5;
            [Range(0, 1)] public int stencilReference = 1;
            
            public Material material;
            public Mesh mesh;
        }

        class PortalRenderFeaturePass : ScriptableRenderPass
        {
            readonly PortalRenderFeatureSettings _settings;
            
            private ShaderTagId _forwardTag = new ShaderTagId("UniversalForward");
            private ShaderTagId _stencilTag = new ShaderTagId("UniversalForwardStencil");
            private ShaderTagId _srpUnlit = new ShaderTagId("SRPDefaultUnlit");

            public PortalRenderFeaturePass(PortalRenderFeatureSettings settings)
            {
                this._settings = settings;
            }

            // This class stores the data needed by the RenderGraph pass.
            // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
            private class PassData
            {
                public UniversalCameraData CameraData;
                public RendererListHandle RendererListHdl;

                public Pose PortalPose;
                public Pose PortalOutPose;
                public Pose CameraPose;

                public Vector2 PortalSize;

                public Material Material;
                public Mesh Mesh;
            }
            
            private void InitRendererLists(ShaderTagId tagId, UniversalRenderingData renderingData, UniversalLightData lightData,
                ref PassData passData, ScriptableRenderContext context, RenderGraph renderGraph)
            {
                SortingCriteria sortingCriteria = passData.CameraData.defaultOpaqueSortFlags;
                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(tagId, renderingData, passData.CameraData, lightData, sortingCriteria);

                FilteringSettings filteringSettings = FilteringSettings.defaultValue;
                filteringSettings.layerMask = Int32.MaxValue;
                filteringSettings.renderQueueRange = RenderQueueRange.all;
                
                RendererListParams listParams = new RendererListParams(renderingData.cullResults, drawingSettings,
                    filteringSettings);
                
                
                listParams.tagName = tagId;
                var tags = new NativeArray<ShaderTagId>(1, Allocator.Temp);
                tags[0] = ShaderTagId.none;
                
                var blocks = new NativeArray<RenderStateBlock>(1, Allocator.Temp);
                
                RenderStateBlock stencilBlock = new RenderStateBlock(RenderStateMask.Stencil);
                stencilBlock.stencilReference = _settings.stencilReference;
                stencilBlock.stencilState = new StencilState(true, 255, 255, CompareFunction.LessEqual, StencilOp.Keep);
                blocks[0] = stencilBlock;
                
                listParams.stateBlocks = blocks;
                listParams.tagValues = tags;

                listParams.isPassTagName = false;
                
                
                passData.RendererListHdl = renderGraph.CreateRendererList(listParams);
            }

            // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
            // It is used to execute draw commands.
            static void ExecutePass(PassData data, RasterGraphContext context)
            {
                Matrix4x4 obliqueProjectionMatrix =
                    Portal.GetProjectionMatrix(data.PortalOutPose, data.CameraPose,
                        data.CameraData.camera);

                Vector3 offset = data.PortalPose.rotation * new Vector3(0, 0, -0.5f * Portal.PortalDepth);
                
                Matrix4x4 portalMatrix =
                    Matrix4x4.TRS(data.PortalPose.position + offset, data.PortalPose.rotation, new Vector3(data.PortalSize.x, data.PortalSize.y, Portal.PortalDepth));
                
                context.cmd.DrawMesh(data.Mesh, portalMatrix, data.Material, 0, 0);
                
                context.cmd.SetViewProjectionMatrices(data.CameraPose.ToViewMatrix(), obliqueProjectionMatrix);
                context.cmd.DrawRendererList(data.RendererListHdl);
                
                context.cmd.SetViewProjectionMatrices(data.CameraData.GetViewMatrix(), data.CameraData.GetProjectionMatrix());
                
                
                context.cmd.DrawMesh(data.Mesh, portalMatrix, data.Material, 0, 1);
            }

            private List<Portal> _portals = new();
            
            // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
            // FrameData is a context container through which URP resources can be accessed and managed.
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                const string passName = "Render Portal Pass";
                
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
                UniversalLightData lightData = frameData.Get<UniversalLightData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                _portals.Clear();
                foreach (Portal p in Portal.ActivePortals)
                {
                    if (!p.transform.IsInFrontOf(cameraData.camera.transform.position))
                        continue;

                    if (!CameraUtility.IsVisibleFromCamera(p.Bounds, cameraData.camera))
                        continue;
                    
                    p.UpdateDistanceToCamera(cameraData.camera);
                    _portals.Add(p);
                }


                if (_portals.Count == 0)
                    return;
                
                _portals.Sort();
                
                if (!_settings.mesh || !_settings.material)
                    return;

                // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
                foreach (Portal portal in _portals)
                {
                    using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                    {
                        // Use this scope to set the required inputs and outputs of the pass and to
                        // setup the passData with the required properties needed at pass execution time.

                        // Make use of frameData to access resources and camera data through the dedicated containers.
                        // Eg:

                        Pose cameraPose = Portal.GetCameraPose(portal, portal.OtherPortal,
                            cameraData.camera.transform.ToPose());

                        passData.CameraPose = cameraPose;
                        passData.PortalOutPose = portal.OtherPortal.transform.ToPose();
                        passData.PortalPose = portal.transform.ToPose();
                        passData.PortalSize = portal.Size;
                        passData.Material = _settings.material;
                        passData.Mesh = _settings.mesh;

                        passData.CameraData = cameraData;
                        InitRendererLists(_forwardTag, renderingData, lightData, ref passData, default, renderGraph);

                        // Setup pass inputs and outputs through the builder interface.
                        // Eg:
                        // builder.UseTexture(sourceTexture);
                        // TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraData.cameraTargetDescriptor, "Destination Texture", false);

                        //builder.AllowPassCulling(false);
                        //builder.AllowGlobalStateModification(true);
                        builder.UseRendererList(passData.RendererListHdl);

                        // This sets the render target of the pass to the active color texture. Change it to your own render target as needed.
                        builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                        builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);

                        // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
                        builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                            ExecutePass(data, context));
                    }
                }
            }

            private void DrawRecursivePortals(Matrix4x4 viewMat, Matrix4x4 projMat, int maxRecursionLevel,
                int recursionLevel)
            {
                
            }
        }
    }
}
