using GGJ.Utility.Extensions;
using UnityEngine;

namespace GGJ.Rendering.Portals
{
    public static class PortalUtility
    {
        public static bool Raycast(Ray ray, out RaycastHit hit, float maxDistance = float.PositiveInfinity, int layerMask = int.MaxValue, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            hit = new RaycastHit();
            
            if (Physics.Raycast(ray, out hit, maxDistance, layerMask, triggerInteraction))
            {
                if (!hit.collider.TryGetComponent(out Portal portal))
                {
                    return true;
                }

                if (!portal.OtherPortal)
                    return Raycast(new Ray(ray.GetPoint(hit.distance), ray.direction), out hit,
                        maxDistance - hit.distance, layerMask, triggerInteraction);
                
                var inWorldToLocal = portal.transform.worldToLocalMatrix;
                var outLocalToWorld = portal.OtherPortal.transform.localToWorldMatrix;
                
                Vector3 newDir = inWorldToLocal.MultiplyVector(ray.direction);
                newDir.z *= -1;
                if (!portal.OtherPortal.Mirror)
                    newDir.x *= -1;
                newDir = outLocalToWorld.MultiplyVector(newDir);

                Vector3 newPos = inWorldToLocal.MultiplyPoint(hit.point);
                newPos = outLocalToWorld.MultiplyPoint(newPos);

                Ray newRay = new(newPos, newDir);
                
                return Raycast(newRay, out hit, maxDistance - hit.distance, layerMask, triggerInteraction);
            }

            return false;
        }
    }
}