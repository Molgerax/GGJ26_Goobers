using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace GGJ.Rendering.Portals
{
    public class SkyboxRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        SkyboxRenderFeaturePass _scriptablePass;

        /// <inheritdoc/>
        public override void Create()
        {
            _scriptablePass = new SkyboxRenderFeaturePass();

            // Configures where the render pass should be injected.
            _scriptablePass.renderPassEvent = renderPassEvent;

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
       
        
        class SkyboxRenderFeaturePass : ScriptableRenderPass
        {
            public SkyboxRenderFeaturePass()
            {
            }

            // This class stores the data needed by the RenderGraph pass.
            // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
            private class PassData
            {
                public UniversalCameraData CameraData;
                public RendererListHandle SkyboxList;
            }
            

            
            // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
            // It is used to execute draw commands.
            static void ExecutePass(PassData data, RasterGraphContext context)
            {
                context.cmd.DrawRendererList(data.SkyboxList);
            }
            
            // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
            // FrameData is a context container through which URP resources can be accessed and managed.
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                const string passName = "Skybox Extra Pass";
                
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                {
                    var skyboxRendererList = renderGraph.CreateSkyboxRendererList(cameraData.camera);
                    passData.SkyboxList = skyboxRendererList;
                    passData.CameraData = cameraData;
                    
                    // This sets the render target of the pass to the active color texture. Change it to your own render target as needed.
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                    builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);
                    
                    builder.UseRendererList(skyboxRendererList);
                    
                    // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                        ExecutePass(data, context));
                }
            }
        }
    }
}
