using System;
using GGJ.Utility;
using GGJ.Utility.Extensions;
using UnityEngine;
using UnityEngine.Rendering;

namespace GGJ.Rendering.Portals
{
    [RequireComponent(typeof(Renderer))]
    public class PortalObjectRenderer : MonoBehaviour
    {
        private SkinnedMeshRenderer _skinnedMeshRenderer;
        private MeshRenderer _meshRenderer;
        private Mesh _sharedMesh;

        
        private bool _isSkinned;
        public bool IsSkinned => _isSkinned;

        public Mesh SharedMesh => _sharedMesh;
        public Renderer Renderer => _isSkinned ? _skinnedMeshRenderer : _meshRenderer;
        
        private PortalObjectGhost _ghost;
        
        private void Awake()
        {
            var render = GetComponent<Renderer>();

            if (render is SkinnedMeshRenderer smr)
            {
                _skinnedMeshRenderer = smr;
                _sharedMesh = _skinnedMeshRenderer.sharedMesh;
                _isSkinned = true;
            }
            else if (render is MeshRenderer mr)
            {
                _meshRenderer = mr;
                var filter = GetComponent<MeshFilter>();
                _sharedMesh = filter.sharedMesh;
                _isSkinned = false;
            }
            else
            {
                Debug.LogError($"Renderer {render} is neither {nameof(MeshRenderer)} nor {nameof(SkinnedMeshRenderer)}!");
                return;
            }

            _ghost = PortalObjectGhost.Create(this);
        }

        private void OnEnable()
        {
            if (_ghost)
                _ghost.SetActive(true);
        }

        private void OnDisable()
        {
            if (_ghost)
                _ghost.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_ghost)
                CoreUtils.Destroy(_ghost.gameObject);
        }

        private void LateUpdate()
        {
            if (!_ghost)
                return;
            
            ScaledPose pose = transform.ToPose();
            
            pose.scale = transform.localScale;
            foreach (var portal in Portal.ActivePortals)
            {
                if (!portal.OtherPortal)
                    continue;

                if (portal.Bounds.Intersects(Renderer.bounds))
                {
                    _ghost.SetActive(true);
                    
                    _ghost.UpdateRenderer(Renderer);
                    
                    if (_isSkinned)
                    {
                        _ghost.SetSkinnedMesh(_skinnedMeshRenderer);
                        pose = _skinnedMeshRenderer.rootBone.ToPose();
                    }
                    
                    _ghost.SetTransform(Portal.GetCameraPose(portal, portal.OtherPortal, pose));
                    return;
                }
            }
            _ghost.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_meshRenderer)
                return;
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_meshRenderer.bounds.center, _meshRenderer.bounds.size);
        }
    }
}