using System;
using UnityEngine;

namespace GGJ.Gameplay.Faces
{
    public class Face : MonoBehaviour
    {
        [SerializeField] private FaceTexture faceTexture;
        [SerializeField] private MeshRenderer faceMeshRenderer;

        private MaterialPropertyBlock _propertyBlock;

        private float _removeRadius;
        private Vector2 _removeUv;

        public FaceTexture FaceTexture => faceTexture;

        public void SetFace(FaceTexture newFace)
        {
            faceTexture = newFace;
            SetFace();
        }
        
        private void Awake()
        {
            SetFace();
        }

        public void SetFace()
        {
            _propertyBlock ??= new();

            faceMeshRenderer.GetPropertyBlock(_propertyBlock);
            
            _propertyBlock.SetTexture(PropertyIDs.MainTex, faceTexture.Texture);
            _propertyBlock.SetFloat(PropertyIDs.RemoveRadius, _removeRadius);
            _propertyBlock.SetVector(PropertyIDs.RemoveUv, _removeUv);
            
            faceMeshRenderer.SetPropertyBlock(_propertyBlock);
        }
        
        public void RemoveFromFace(Vector2 uv, float radius)
        {
            _removeRadius = radius;
            _removeUv = uv;
            
            SetFace();
        }

        

        private static class PropertyIDs
        {
            public static readonly int MainTex = Shader.PropertyToID("_MainTex");
            
            public static readonly int RemoveUv = Shader.PropertyToID("_RemoveUv");
            public static readonly int RemoveRadius = Shader.PropertyToID("_RemoveRadius");
        }
    }
}
