using System;
using System.Collections.Generic;
using GGJ.Utility;
using GGJ.Utility.Extensions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace GGJ.Rendering.Portals
{
    public class EraseDepthRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] EraseDepthRenderFeatureSettings settings;
        EraseDepthRenderFeaturePass _scriptablePass;

        
        /// <inheritdoc/>
        public override void Create()
        {
            _scriptablePass = new EraseDepthRenderFeaturePass(settings);

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
        public class EraseDepthRenderFeatureSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPrePasses;
            public Material material;
            public Mesh mesh;
        }
        
        public static Plane[] ClippingPlanes = new Plane[6];

        class EraseDepthRenderFeaturePass : ScriptableRenderPass
        {
            readonly EraseDepthRenderFeatureSettings _settings;
            
            private ShaderTagId _forwardTag = new ShaderTagId("ClearDepth");
            private ShaderTagId _shadowTag = new ShaderTagId("ShadowCaster");

            public EraseDepthRenderFeaturePass(EraseDepthRenderFeatureSettings settings)
            {
                this._settings = settings;
            }

            // This class stores the data needed by the RenderGraph pass.
            // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
            private class PassData
            {
                public UniversalCameraData CameraData;
                public RendererListHandle RendererListHdl;
                public RendererListHandle SkyboxList;
                public Material Material;
                public Mesh Mesh;
            }

            
            private void InitRendererLists(ShaderTagId tagId, UniversalRenderingData renderingData, UniversalLightData lightData, CullContextData cullData,
                ref PassData passData, RenderGraph renderGraph)
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
                
                RenderStateBlock stencilBlock = new RenderStateBlock(RenderStateMask.Nothing);

                //stencilBlock.mask |= RenderStateMask.Stencil;
                
                stencilBlock.stencilReference = 128;
                stencilBlock.stencilState = new StencilState(true, 128, 128, CompareFunction.Equal, StencilOp.Keep);

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
                context.cmd.DrawRendererList(data.RendererListHdl);
                
                context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 6);
                
                context.cmd.DrawRendererList(data.SkyboxList);
            }
            
            
            // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
            // FrameData is a context container through which URP resources can be accessed and managed.
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                const string passName = "Erase Depth Pass";
                
                if (!_settings.material || !_settings.mesh)
                    return;
                
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
                UniversalLightData lightData = frameData.Get<UniversalLightData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                CullContextData cullContextData = frameData.Get<CullContextData>();

                
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                {
                    var skyboxRendererList = renderGraph.CreateSkyboxRendererList(cameraData.camera);
                    passData.SkyboxList = skyboxRendererList;
                    passData.CameraData = cameraData;
                    passData.Material = _settings.material;
                    passData.Mesh = _settings.mesh;

                    InitRendererLists(_forwardTag, renderingData, lightData, cullContextData, ref passData, renderGraph);
                    
                    builder.AllowGlobalStateModification(true);
                    builder.UseRendererList(passData.RendererListHdl);
                    builder.UseRendererList(passData.SkyboxList);

                    // This sets the render target of the pass to the active color texture. Change it to your own render target as needed.
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                    builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);

                    // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                        ExecutePass(data, context));
                }
            }
        }
    }
}
