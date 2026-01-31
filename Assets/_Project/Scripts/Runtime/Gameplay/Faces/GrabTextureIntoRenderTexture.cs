using GGJ.Utility;
using UnityEngine;

namespace GGJ.Gameplay.Faces
{
    public class GrabTextureIntoRenderTexture
    {
        public static void TransferTexture(ComputeShader cs, Vector2 uv, int radius, Texture read, RenderTexture write)
        {
            cs.SetTexture(0, PropertyIDs.ReadTexture, read);
            cs.SetTexture(0, PropertyIDs.WriteTexture, write);
            cs.SetInt(PropertyIDs.RemoveRadius, radius);
            cs.SetVector(PropertyIDs.RemoveUv, uv);
            
            cs.SetInt(PropertyIDs.ReadResolution, read.height);
            cs.SetInt(PropertyIDs.WriteResolution, write.height);
            
            cs.DispatchExact(0, write.width, write.height);
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
