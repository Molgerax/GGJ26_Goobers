using System.Collections.Generic;
using GGJ.Utility;
using TinyGoose.Tremble;
using UnityEngine;
using UnityEngine.Rendering;

namespace GGJ.Mapping.BrushEntities
{
    [BrushEntity("probe_cluster", "light", BrushType.Invisible)]
    public class TrembleLightProbeCluster : MonoBehaviour, IOnImportFromMapEntity
    {
        [Tremble("resolution")] private float _resolution = 3f;
        
        
        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            MeshCollider mc = gameObject.GetComponent<MeshCollider>();
#if UNITY_EDITOR
            LightProbeGroup probeGroup = gameObject.AddComponent<LightProbeGroup>();

            List<Vector3> probePositions = new();

            Bounds bounds = mc.bounds;

            Vector3 resolution = _resolution * Vector3.one;

            Vector3 size = bounds.size;

            Vector3Int count = new Vector3Int(
                Mathf.CeilToInt(size.x / resolution.x),
                Mathf.CeilToInt(size.y / resolution.y), 
                Mathf.CeilToInt(size.z / resolution.z));

            
            if (count.x > 1) size.x -= 0.2f;
            if (count.y > 1) size.y -= 0.2f;
            if (count.z > 1) size.z -= 0.2f;
            
            resolution = new(
                size.x / (count.x - 1), 
                size.y / (count.y - 1), 
                size.z / (count.z - 1));
            //size = Vector3.Scale(count,  resolution);
            
            Vector3 startPos = bounds.center - size * 0.5f;
            
            for (int x = 0; x < count.x; x++)
            {
                for (int y = 0; y < count.y; y++)
                {
                    for (int z = 0; z < count.z; z++)
                    {
                        Vector3 pos = startPos + new Vector3(resolution.x * x, resolution.y * y, resolution.z * z);

                        if (IsInsideMeshCollider(mc, pos, 16))
                        {
                            probePositions.Add(pos - transform.position);
                        }
                    }
                }
            }

            probeGroup.probePositions = probePositions.ToArray();
#endif
            //CoreUtils.Destroy(mc);
        }

        public bool IsInsideMeshCollider(MeshCollider meshCollider, Vector3 point, int iterations = 2)
        {
            // Change "queriesHitBackfaces" to true
            bool previousBackfaceSetting = Physics.queriesHitBackfaces;
            Physics.queriesHitBackfaces = true;
            
            // Arbitrary direction to shoot in.

            int inside = 0;
            int outside = 0;

            for (int i = 0; i < iterations; i++)
            {
                Vector3 shootDirection = VectorMath.GetPointOnSphere(i, iterations);
                
                int meshHits = 0;

                Ray ray = new(point, shootDirection);

                // Stop raycasting if we didn't hit anything.
                // TODO: use collision layers to improve this.
                float distance = 10000;
                int maxIts = 10;
                while (meshCollider.Raycast(ray, out RaycastHit hit, distance) && distance > 0 && meshHits < maxIts)
                {
                    meshHits++;
                    ray.origin = ray.GetPoint(hit.distance + 0.001f);
                }

                if (meshHits >= maxIts)
                    continue;
                
                if (meshHits % 2 == 0)
                    outside++;
                else
                    inside++;
            }

            // Restore old "queriesHitBackfaces"
            Physics.queriesHitBackfaces = previousBackfaceSetting;

            // If the number of hits per direction is odd, the point is inside
            return inside > outside;
        }
    }
}