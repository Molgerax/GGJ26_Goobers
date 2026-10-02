using System;
using GGJ.Utility;
using UnityEngine;

namespace GGJ.Rendering.Portals
{
    public class PortalObjectGhost : MonoBehaviour
    {
        private MeshFilter _filter;
        private Renderer _renderer;
        private Mesh _sharedMesh;
        private MaterialPropertyBlock _propertyBlock;

        private bool _drawProcedural;

        private Mesh _constructedMesh;

        private PortalObjectRenderer _original;
        
        public static PortalObjectGhost Create(PortalObjectRenderer original)
        {
            GameObject go = new GameObject(original.name + "_PortalGhost");
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.SetActive(false);
            var ghost = go.AddComponent<PortalObjectGhost>();
            ghost.Initialize(original);
            return ghost;
        }
        
        
        public void Initialize(PortalObjectRenderer original)
        {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<Renderer>();
            _propertyBlock = new();

            _constructedMesh = null;

            _sharedMesh = original.SharedMesh;
            _filter.sharedMesh = _sharedMesh;
            UpdateRenderer(original.Renderer);

            _drawProcedural = original.IsSkinned;
            
            if (original.IsSkinned && original.Renderer is SkinnedMeshRenderer smr)
            {
                _constructedMesh = Instantiate(_sharedMesh);
                _filter.sharedMesh = _constructedMesh;
                
                _constructedMesh.vertexBufferTarget |= GraphicsBuffer.Target.CopyDestination;
                smr.vertexBufferTarget |= GraphicsBuffer.Target.CopySource;
            }

            _original = original;
            SetActive(false);
        }

        private bool _isActive;
        
        public void SetActive(bool value)
        {
            if (_isActive == value)
                return;
            _isActive = value;
            
            gameObject.SetActive(value);
            if (value)
                transform.parent = _original.transform;
        }

        

        public void UpdateRenderer(Renderer render)
        {
            gameObject.layer = render.gameObject.layer;
            transform.parent = render.transform.parent;
            
            _renderer.renderingLayerMask = render.renderingLayerMask;
            _renderer.sharedMaterials = render.sharedMaterials;
            
            _propertyBlock.Clear();
            render.GetPropertyBlock(_propertyBlock);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
        
        public void SetSkinnedMesh(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            ConstructMesh(skinnedMeshRenderer);
        }

        private void ConstructMesh(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            _constructedMesh.vertexBufferTarget |= GraphicsBuffer.Target.CopyDestination;
            skinnedMeshRenderer.vertexBufferTarget |= GraphicsBuffer.Target.CopySource;
            
            var targetBuffer = _constructedMesh.GetVertexBuffer(0);
            var sourceBuffer = skinnedMeshRenderer.GetVertexBuffer();
            
            if ((sourceBuffer.target & GraphicsBuffer.Target.CopySource) > 0)
                Graphics.CopyBuffer(sourceBuffer, targetBuffer);
            
            targetBuffer.Dispose();
            sourceBuffer.Dispose();
        }

        public void SetTransform(ScaledPose pose)
        {
            transform.SetPositionAndRotation(pose.position, pose.rotation);
            transform.localScale = pose.scale;

            if (_drawProcedural)
            {
                Vector3 scale = transform.lossyScale;
                transform.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
                transform.localScale = Vector3.Scale(transform.localScale, pose.scale);
            }
        }
    }
}