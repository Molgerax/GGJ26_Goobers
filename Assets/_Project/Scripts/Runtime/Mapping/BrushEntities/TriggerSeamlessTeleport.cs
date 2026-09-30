using System.Collections.Generic;
using GGJ.Mapping.Tremble.Properties;
using GGJ.Rendering.Portals;
using TinyGoose.Tremble;
using UnityEngine;
using UnityEngine.Rendering;

namespace GGJ.Mapping.BrushEntities
{
    [BrushEntity("teleport_seamless", "trigger", BrushType.Trigger)]
    public class TriggerSeamlessTeleport : MonoBehaviour, IOnImportFromMapEntity
    {
        [SerializeField, Tremble("target")] private TriggerSeamlessTeleport destination;
        [SerializeField, Tremble("angle")] private QuakeAngle angle;

        [SerializeField, Tremble("passable"), SpawnFlags]
        private bool passable;
        [SerializeField, Tremble("mirror"), SpawnFlags]
        private bool mirror;
        
        [NoTremble] public Portal portal;
        
        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            Vector3 direction = angle;
            
            Vector3 positiveDirection = new(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));
            Vector3 right = Vector3.Cross(Vector3.up, direction);
            if (right.magnitude == 0)
                right = Vector3.right;
            right = right.normalized;
            Vector3 up = Vector3.Cross(direction, right).normalized;
            
            transform.rotation = Quaternion.LookRotation(direction, up);
            
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            List<Vector3> vertices = new();
            
            Bounds bounds = meshCollider.sharedMesh.bounds;
            meshCollider.sharedMesh.GetVertices(vertices);
            
            CoreUtils.Destroy(meshCollider);

            
            float distance = Vector3.Dot(positiveDirection, bounds.size);
            Vector2 size = Vector2.zero;
            size.x = Mathf.Abs(Vector3.Dot(right, bounds.size));
            size.y = Mathf.Abs(Vector3.Dot(up, bounds.size));

            distance = GetWidthAlongAxis(direction, vertices);
            size.x = GetWidthAlongAxis(right, vertices);
            size.y = GetWidthAlongAxis(up, vertices);
            
            transform.position += direction * distance * 0.5f;
            
            portal = gameObject.AddComponent<Portal>();
            portal.Setup(destination, size, distance, passable, mirror);

            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.center = Vector3.back * distance * 0.5f;
            boxCollider.size = new Vector3(size.x, size.y, distance);
        }

        private float GetWidthAlongAxis(Vector3 normal, List<Vector3> vertices)
        {
            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 vertex = vertices[i];
                min = Mathf.Min(min, Vector3.Dot(normal, vertex));
                max = Mathf.Max(max, Vector3.Dot(normal, vertex));
            }
            return max - min;
        }
    }
}
