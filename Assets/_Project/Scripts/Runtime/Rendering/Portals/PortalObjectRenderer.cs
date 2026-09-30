using GGJ.Utility;
using GGJ.Utility.Extensions;
using UnityEngine;

namespace GGJ.Rendering.Portals
{
    [RequireComponent(typeof(MeshRenderer))]
    public class PortalObjectRenderer : MonoBehaviour
    {
        private MeshRenderer _renderer;
        private MeshFilter _filter;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _filter = GetComponent<MeshFilter>();
        }

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
                    PortalObjectGhost mirror = PortalObjectGhost.RendererPool.Get();
                    mirror.SetMeshAndMaterials(_filter, _renderer);
                    mirror.SetTransform(Portal.GetCameraPose(portal, portal.OtherPortal, pose));
                }
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