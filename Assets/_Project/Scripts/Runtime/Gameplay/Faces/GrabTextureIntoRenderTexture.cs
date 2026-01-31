using GGJ.Utility;
using UnityEngine;

namespace GGJ.Gameplay.Faces
{
    public class GrabTextureIntoRenderTexture
    {
        public static void TransferTexture(ComputeShader cs, Vector2 uv, int radius, Texture read, RenderTexture write)
        {
            int kernel = cs.FindKernel("GrabFaceTexture");
            cs.SetTexture(kernel, PropertyIDs.ReadTexture, read);
            cs.SetTexture(kernel, PropertyIDs.WriteTexture, write);
            cs.SetInt(PropertyIDs.RemoveRadius, radius);
            cs.SetVector(PropertyIDs.RemoveUv, uv);
            
            cs.SetInt(PropertyIDs.ReadResolution, read.height);
            cs.SetInt(PropertyIDs.WriteResolution, write.height);
            
            cs.DispatchExact(kernel, write.width, write.height);
        }
        
        public static void StackTextureToFace(ComputeShader cs, Vector2 uv, GrabbedFacePart facePart, Texture read, RenderTexture write)
        {
            int kernel = cs.FindKernel("StackFaceTexture");
            cs.SetTexture(kernel, PropertyIDs.ReadTexture, read);
            cs.SetTexture(kernel, PropertyIDs.WriteTexture, write);
            cs.SetInt(PropertyIDs.RemoveRadius, facePart.Radius);
            cs.SetVector(PropertyIDs.RemoveUv, uv);
            
            cs.SetInt(PropertyIDs.ReadResolution, read.height);
            cs.SetInt(PropertyIDs.WriteResolution, write.height);
            
            cs.DispatchExact(kernel, write.width, write.height);
        }
        
        public static void ClearTexture(ComputeShader cs, RenderTexture write)
        {
            int kernel = cs.FindKernel("Clear");
            cs.SetTexture(kernel, PropertyIDs.WriteTexture, write);
            cs.SetInt(PropertyIDs.WriteResolution, write.height);
            
            cs.DispatchExact(kernel, write.width, write.height);
        }
        
        
        private static class PropertyIDs
        {
            public static readonly int ReadTexture = Shader.PropertyToID("_ReadTexture");
            public static readonly int WriteTexture = Shader.PropertyToID("_WriteTexture");
            
            public static readonly int ReadResolution = Shader.PropertyToID("_ReadResolution");
            public static readonly int WriteResolution = Shader.PropertyToID("_WriteResolution");
            
            public static readonly int RemoveUv = Shader.PropertyToID("_RemoveUv");
            public static readonly int RemoveRadius = Shader.PropertyToID("_RemoveRadius");
        }
    }
}
