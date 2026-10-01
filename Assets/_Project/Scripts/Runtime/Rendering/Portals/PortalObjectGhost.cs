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
        
        public void Initialize()
        {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<Renderer>();
            _propertyBlock = new();

            _constructedMesh = new Mesh();
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }

        

        public void SetMeshAndMaterials(MeshFilter filter, MeshRenderer meshRenderer)
        {
            gameObject.layer = filter.gameObject.layer;
            
            _filter.sharedMesh = filter.sharedMesh;
            _renderer.sharedMaterials = meshRenderer.sharedMaterials;

            transform.parent = filter.transform.parent;
            
            _propertyBlock.Clear();
            meshRenderer.GetPropertyBlock(_propertyBlock);
            _renderer.SetPropertyBlock(_propertyBlock);

            _drawProcedural = false;
        }
        
        public void SetSkinnedMesh(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            _renderer.sharedMaterials = skinnedMeshRenderer.sharedMaterials;
            transform.parent = skinnedMeshRenderer.transform.parent;
            
            _propertyBlock.Clear();
            skinnedMeshRenderer.GetPropertyBlock(_propertyBlock);
            _renderer.SetPropertyBlock(_propertyBlock);

            _sharedMesh = skinnedMeshRenderer.sharedMesh;
            
            _drawProcedural = true;
            
            ConstructMesh(skinnedMeshRenderer);
            _filter.sharedMesh = _constructedMesh;
        }

        private void ConstructMesh(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            if (!(_constructedMesh.vertexCount == _sharedMesh.vertexCount &&
                _constructedMesh.GetIndexCount(0) == _sharedMesh.GetIndexCount(0)))
            {
                _constructedMesh = Instantiate(_sharedMesh);
            }

            skinnedMeshRenderer.vertexBufferTarget |= GraphicsBuffer.Target.Vertex; 

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