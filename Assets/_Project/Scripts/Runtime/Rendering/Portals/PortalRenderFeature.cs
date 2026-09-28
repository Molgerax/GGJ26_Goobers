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

        private static List<Portal> _portals = new();
        
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
        
        public override void OnCameraPreCull(ScriptableRenderer renderer, in CameraData cameraData)
        {
            foreach (var lightEnforcer in LightEnforcer.ActiveLightEnforcers)
            {
                lightEnforcer.ForceVisible = false;
                lightEnforcer.SetForcedVisible(false);
            }
            
            _portals.Clear();
            foreach (Portal p in Portal.ActivePortals)
            {
                if (!p.transform.IsInFrontOf(cameraData.camera.transform.position))
                    continue;

                if (!CameraUtility.IsVisibleFromCamera(p.Bounds, cameraData.camera))
                    continue;
                    
                if (!p.OtherPortal)
                    continue;
                    
                p.UpdateDistanceToCamera(cameraData.camera);
                _portals.Add(p);
            }


            if (_portals.Count == 0)
                return;
                
            _portals.Sort();


            foreach (Portal portal in _portals)
            {
                ScaledPose cameraPose = Portal.GetCameraPose(portal, portal.OtherPortal,
                    cameraData.camera.transform.ToPose());
                ScaledPose portalOutPose = portal.OtherPortal.Transform;
                
                cameraData.camera.TryGetCullingParameters(out var cullParams);

                for (int i = 0; i < 6; i++)
                {
                    Plane plane = cullParams.GetCullingPlane(i);
                    Vector4 p = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);

                    p = cameraData.camera.transform.worldToLocalMatrix.inverse.transpose * p;
                    p = cameraPose.ToMatrix().inverse.transpose * p;

                    plane = new Plane(p, p.w);

                    if (i == 4)
                        plane = new Plane(portalOutPose.forward, portalOutPose.position);
                    ClippingPlanes[i] = plane;
                }

                foreach (var lightEnforcer in LightEnforcer.ActiveLightEnforcers)
                {
                    bool visible = true;
                    for (int i = 0; i < 6; i++)
                    {
                        Plane p = ClippingPlanes[i];
                        Vector3 pos = lightEnforcer.transform.position;
                        pos += p.normal * lightEnforcer.Light.dilatedRange;
                        
                        if (!p.GetSide(pos))
                            visible = false;
                    }

                    if (visible)
                        lightEnforcer.ForceVisible = true;
                }
            }
            
            foreach (var lightEnforcer in LightEnforcer.ActiveLightEnforcers)
            {
                if (lightEnforcer.ForceVisible)
                    lightEnforcer.SetForcedVisible(true);
            }
        }

        // Use this class to pass around settings from the feature to the pass
        [Serializable]
        public class PortalRenderFeatureSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPrePasses;
            [Range(0, 5)] public int maxIterations = 5;
            
            public Material materialFirst;
            public Material materialSecond;
            public Mesh mesh;
        }
        
        public static Plane[] ClippingPlanes = new Plane[6];

        class PortalRenderFeaturePass : ScriptableRenderPass
        {
            readonly PortalRenderFeatureSettings _settings;
            
            private ShaderTagId _forwardTag = new ShaderTagId("UniversalForward");
            private ShaderTagId _shadowTag = new ShaderTagId("ShadowCaster");

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
                public RendererListHandle SkyboxList;

                public ScaledPose PortalPose;
                public ScaledPose PortalOutPose;
                public ScaledPose CameraPose;
                public ScaledPose CameraInitPose;
                
                public Vector2 PortalSize;
                public float PortalDepth;
                public bool PortalMirror;
                
                public Material MaterialFirst;
                public Material MaterialSecond;
                public Mesh Mesh;
                public int RecursionLevel;
            }

            private struct PortalData
            {
                public UniversalCameraData cameraData;
                public UniversalRenderingData renderingData;
                public UniversalLightData lightData;
                public UniversalResourceData resourceData;
                public CullContextData cullContextData;
            }
            
            private void InitRendererLists(ShaderTagId tagId, UniversalRenderingData renderingData, UniversalLightData lightData, CullContextData cullData,
                ref PassData passData, RenderGraph renderGraph, bool mirror)
            {
                passData.CameraData.camera.TryGetCullingParameters(out var cullParams);
                cullParams.origin = passData.CameraPose.position;

                for (int i = 0; i < 6; i++)
                {
                    Plane plane = cullParams.GetCullingPlane(i);
                    Vector4 p = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
                    
                    p = passData.CameraData.camera.transform.worldToLocalMatrix.inverse.transpose * p;
                    p = passData.CameraPose.ToMatrix().inverse.transpose * p;

                    plane = new Plane(p, p.w);
                    
                    if (i == 4)
                        plane = new Plane(passData.PortalOutPose.forward, passData.PortalOutPose.position);
                    
                    cullParams.SetCullingPlane(i, plane);
                    
                    if (passData.CameraData.cameraType == CameraType.Game)
                        ClippingPlanes[i] = plane;
                }


                cullParams.cullingOptions |= CullingOptions.NeedsLighting;
                var cullResults = cullData.Cull(ref cullParams);

                
                //cullResults = renderingData.cullResults;
                
                //string debug = "\nOld: ";
                //var lightMap = renderingData.cullResults.GetLightIndexMap(Allocator.Temp);
                //foreach (var l in lightMap)
                //{
                //    debug += $"{l}, ";
                //}
                //lightMap = cullResults.GetLightIndexMap(Allocator.Temp);
                //debug += "\nNew: ";
                //foreach (var l in lightMap)
                //{
                //    debug += $"{l}, ";
                //}

                //debug +=
                //    $"\n{passData.CameraData.cameraType}, Old: {renderingData.cullResults.lightIndexCount}, New: {cullResults.lightIndexCount}";
                //
                //Debug.Log(debug);
                
                
                SortingCriteria sortingCriteria = passData.CameraData.defaultOpaqueSortFlags;
                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(tagId, renderingData, passData.CameraData, lightData, sortingCriteria);
                
                FilteringSettings filteringSettings = FilteringSettings.defaultValue;
                filteringSettings.layerMask = Int32.MaxValue;
                filteringSettings.renderQueueRange = RenderQueueRange.all;
                
                RendererListParams listParams = new RendererListParams(cullResults, drawingSettings,
                    filteringSettings);
                
                
                listParams.tagName = tagId;
                var tags = new NativeArray<ShaderTagId>(1, Allocator.Temp);
                tags[0] = ShaderTagId.none;
                
                var blocks = new NativeArray<RenderStateBlock>(1, Allocator.Temp);
                
                RenderStateBlock stencilBlock = new RenderStateBlock(RenderStateMask.Stencil);
                stencilBlock.stencilReference = passData.RecursionLevel + 1;
                stencilBlock.stencilState = new StencilState(true, 255, 255, CompareFunction.Equal, StencilOp.Keep);
                
                stencilBlock.mask |= RenderStateMask.Raster;

                CullMode cull = CullMode.Back;
                if (mirror)
                    cull = CullMode.Front;
                stencilBlock.rasterState = new RasterState(cull, 1, 1);
                
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
                Vector3 offset = data.PortalPose.rotation * new Vector3(0, 0, -0.5f * data.PortalDepth);

                Matrix4x4 projectionMatrix = data.CameraData.GetProjectionMatrix();
                Matrix4x4 viewMatrix = data.CameraInitPose.ToViewMatrix();
                context.cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                

                viewMatrix = data.CameraPose.ToViewMatrix();
                
                Plane plane = new Plane(data.PortalOutPose.forward, data.PortalOutPose.position);
                context.cmd.SetGlobalVector("_ClippingPlane", new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance));
                
                context.cmd.SetGlobalVector("_WorldSpaceCameraPos", data.CameraPose.position);
                
                context.cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                context.cmd.DrawRendererList(data.RendererListHdl);
                context.cmd.DrawRendererList(data.SkyboxList);
                
                
                context.cmd.SetGlobalVector("_ClippingPlane", new Vector4(0, 1, 0, 100000));
                
                context.cmd.SetViewProjectionMatrices(data.CameraInitPose.ToViewMatrix(), data.CameraData.GetProjectionMatrix());
                Matrix4x4 portalMatrix =
                    Matrix4x4.TRS(data.PortalPose.position + offset, data.PortalPose.rotation, new Vector3(data.PortalSize.x, data.PortalSize.y, data.PortalDepth));
                
                context.cmd.DrawMesh(data.Mesh, portalMatrix, data.MaterialSecond, 0, data.RecursionLevel + 1);
            }
            
            static void ExecutePortalQuadPass(PassData data, RasterGraphContext context)
            {
                Vector3 offset = data.PortalPose.rotation * new Vector3(0, 0, -0.5f * data.PortalDepth);
                Matrix4x4 viewMatrix = data.CameraInitPose.ToViewMatrix();
                Matrix4x4 projectionMatrix = data.CameraData.GetProjectionMatrix();

                //if (data.PortalMirror)
                //    viewMatrix = Matrix4x4.Scale(new Vector3(-1, 1, 1)) * viewMatrix;
                context.cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                
                Matrix4x4 portalMatrix =
                    Matrix4x4.TRS(data.PortalPose.position + offset, data.PortalPose.rotation, new Vector3(data.PortalSize.x, data.PortalSize.y, data.PortalDepth));
                
                //data.Material.SetInt("_StencilReference", data.RecursionLevel);
                
                context.cmd.DrawMesh(data.Mesh, portalMatrix, data.MaterialFirst, 0, data.RecursionLevel);
            }
            
            // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
            // FrameData is a context container through which URP resources can be accessed and managed.
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                const string passName = "Render Portal Pass";
                
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
                UniversalLightData lightData = frameData.Get<UniversalLightData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                CullContextData cullContextData = frameData.Get<CullContextData>();

                PortalData portalData = new PortalData()
                {
                    cameraData = cameraData,
                    renderingData = renderingData,
                    resourceData = resourceData,
                    lightData = lightData,
                    cullContextData = cullContextData,
                };
                
                if (_portals.Count == 0)
                    return;
                
                if (!_settings.mesh || !_settings.materialFirst || !_settings.materialSecond)
                    return;

                // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
                foreach (Portal portal in _portals)
                {
                    ScaledPose camPose = cameraData.camera.transform.ToPose();

                    if(!CameraUtility.TryGetScreenRectFromBounds(portal.Bounds, cameraData.camera, out var visibleBounds))
                       continue;
                    
                    DrawRecursivePortals(portalData, renderGraph, portal, camPose, visibleBounds, _settings.maxIterations, 0);
                    
                    continue;
                    
                    using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                    {
                        // Use this scope to set the required inputs and outputs of the pass and to
                        // setup the passData with the required properties needed at pass execution time.

                        // Make use of frameData to access resources and camera data through the dedicated containers.
                        // Eg:

                        ScaledPose cameraPose = Portal.GetCameraPose(portal, portal.OtherPortal,
                            cameraData.camera.transform.ToPose());

                        var skyboxRendererList = renderGraph.CreateSkyboxRendererList(cameraData.camera);
                        passData.SkyboxList = skyboxRendererList;
                        passData.CameraPose = cameraPose;
                        passData.CameraInitPose = cameraData.camera.transform.ToPose();
                        passData.PortalOutPose = portal.OtherPortal.transform.ToPose();
                        passData.PortalPose = portal.transform.ToPose();
                        passData.PortalSize = portal.Size;
                        passData.PortalDepth = portal.PortalDepth;
                        passData.PortalMirror = portal.OtherPortal.Mirror;
                        passData.MaterialFirst = _settings.materialFirst;
                        passData.MaterialSecond = _settings.materialSecond;
                        passData.Mesh = _settings.mesh;
                        passData.RecursionLevel = 0;

                        passData.CameraData = cameraData;

                        InitRendererLists(_forwardTag, renderingData, lightData, cullContextData, ref passData, renderGraph, passData.PortalMirror);

                        // Setup pass inputs and outputs through the builder interface.
                        // Eg:
                        // builder.UseTexture(sourceTexture);
                        // TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraData.cameraTargetDescriptor, "Destination Texture", false);

                        //builder.AllowPassCulling(false);
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

            private void DrawRecursivePortals(PortalData portalData, RenderGraph renderGraph, Portal portal,
                ScaledPose cameraPose, Bounds visibleBounds,
                int maxRecursionLevel, int recursionLevel)
            {
                const string passName = "Render Portal Quad";
                const string passName2 = "Render Portal Pass Inside";
                
                var newCameraPose = Portal.GetCameraPose(portal, portal.OtherPortal,
                    cameraPose);


                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName + $"_{recursionLevel}", out var passData))
                {
                    passData.CameraPose = newCameraPose;
                    passData.CameraInitPose = cameraPose;
                    passData.PortalPose = portal.transform.ToPose();
                    passData.PortalSize = portal.Size;
                    passData.PortalDepth = portal.PortalDepth * _settings.mesh.bounds.size.z;
                    passData.PortalMirror = portal.OtherPortal.Mirror;
                    if (cameraPose.scale.x < 0)
                        passData.PortalMirror = !passData.PortalMirror;
                    passData.MaterialFirst = _settings.materialFirst;
                    passData.MaterialSecond = _settings.materialSecond;
                    passData.Mesh = _settings.mesh;
                    passData.RecursionLevel = recursionLevel;

                    passData.CameraData = portalData.cameraData;

                    builder.AllowGlobalStateModification(true);

                    // This sets the render target of the pass to the active color texture. Change it to your own render target as needed.
                    builder.SetRenderAttachment(portalData.resourceData.activeColorTexture, 0);
                    builder.SetRenderAttachmentDepth(portalData.resourceData.activeDepthTexture);

                    // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                        ExecutePortalQuadPass(data, context));
                }
                
                if (recursionLevel < maxRecursionLevel)
                {
                    foreach (Portal activePortal in Portal.ActivePortals)
                    {
                        if (activePortal == portal.OtherPortal)
                            continue;
                        
                        if (!activePortal.OtherPortal)
                            continue;
                        if (!activePortal.transform.IsInFrontOf(newCameraPose.position))
                            continue;
                        if (!CameraUtility.IsVisibleFromCameraAdjusted(activePortal.Bounds,
                                portalData.cameraData.camera, newCameraPose))
                            continue;

                        if (!CameraUtility.TryGetScreenRectFromBounds(activePortal.Bounds,
                                ScaledPose.identity, newCameraPose.ToViewMatrix(),
                                portalData.cameraData.GetProjectionMatrix(), out var newScreenBounds))
                            continue;
                        
                        if (!CameraUtility.ScreenBoundsOverlap(out Bounds summedBounds, visibleBounds, newScreenBounds))
                            continue;
                        
                        DrawRecursivePortals(portalData, renderGraph, activePortal, newCameraPose, summedBounds,
                            maxRecursionLevel, recursionLevel + 1);
                    }
                }

                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName2 + $"_{recursionLevel}", out var passData))
                {
                    var skyboxRendererList = renderGraph.CreateSkyboxRendererList(portalData.cameraData.camera);
                    passData.SkyboxList = skyboxRendererList;
                    passData.CameraPose = newCameraPose;
                    passData.CameraInitPose = cameraPose;
                    passData.PortalOutPose = portal.OtherPortal.transform.ToPose();
                    passData.PortalPose = portal.transform.ToPose();
                    passData.PortalSize = portal.Size;
                    passData.PortalDepth = portal.PortalDepth * _settings.mesh.bounds.size.z;
                    passData.PortalMirror = portal.OtherPortal.Mirror;
                    if (cameraPose.scale.x < 0)
                        passData.PortalMirror = !passData.PortalMirror;
                    passData.MaterialFirst = _settings.materialFirst;
                    passData.MaterialSecond = _settings.materialSecond;
                    passData.Mesh = _settings.mesh;
                    passData.RecursionLevel = recursionLevel;

                    passData.CameraData = portalData.cameraData;

                    InitRendererLists(_forwardTag, portalData.renderingData, portalData.lightData,
                        portalData.cullContextData, ref passData, renderGraph,
                        passData.PortalMirror);

                    
                    builder.AllowGlobalStateModification(true);
                    builder.UseRendererList(passData.RendererListHdl);
                    builder.UseRendererList(passData.SkyboxList);

                    // This sets the render target of the pass to the active color texture. Change it to your own render target as needed.
                    builder.SetRenderAttachment(portalData.resourceData.activeColorTexture, 0);
                    builder.SetRenderAttachmentDepth(portalData.resourceData.activeDepthTexture);

                    // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                        ExecutePass(data, context));
                }
            }
        }
    }
}
