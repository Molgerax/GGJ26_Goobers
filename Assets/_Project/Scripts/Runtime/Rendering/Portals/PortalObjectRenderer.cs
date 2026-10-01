using System;
using GGJ.Utility;
using GGJ.Utility.Extensions;
using UnityEngine;

namespace GGJ.Rendering.Portals
{
    [RequireComponent(typeof(Renderer))]
    public class PortalObjectRenderer : MonoBehaviour
    {
        private Renderer _renderer;
        private MeshFilter _filter;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _filter = GetComponent<MeshFilter>();
        }

        private void OnDisable()
        {
            if (_ghost)
            {
                PortalObjectGhostManager.Pool.Release(_ghost);
                _ghost = null;
            }
        }

        private PortalObjectGhost _ghost;
        
        private void LateUpdate()
        {
            ScaledPose pose = transform.ToPose();
            
            pose.scale = transform.localScale;
            foreach (var portal in Portal.ActivePortals)
            {
                if (!portal.OtherPortal)
                    continue;

                if (portal.Bounds.Intersects(_renderer.bounds))
                {
                    if (!_ghost) 
                        _ghost = PortalObjectGhostManager.Pool.Get();
                    
                    if (_renderer is MeshRenderer ms)
                        _ghost.SetMeshAndMaterials(_filter, ms);
                    if (_renderer is SkinnedMeshRenderer sms)
                    {
                        _ghost.SetSkinnedMesh(sms);
                        pose = sms.rootBone.ToPose();
                    }
                    
                    _ghost.SetTransform(Portal.GetCameraPose(portal, portal.OtherPortal, pose));
                    return;
                }
            }
            if (_ghost)
            {
                PortalObjectGhostManager.Pool.Release(_ghost);
                _ghost = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_renderer)
                return;
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_renderer.bounds.center, _renderer.bounds.size);
        }
    }
}