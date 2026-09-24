using System.Collections.Generic;
using GGJ.Gameplay;
using GGJ.Gameplay.Movement;
using GGJ.Mapping.PointEntities;
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

        [NoTremble] public Portal portal;
        
        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            Vector3 direction = angle;

            
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            Bounds bounds = meshCollider.bounds;
            CoreUtils.Destroy(meshCollider);
            
            Vector3 positiveDirection = new(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));
            float distance = Vector3.Dot(positiveDirection, bounds.size);
            Vector3 right = Vector3.Cross(Vector3.up, direction);
            if (right.magnitude == 0)
                right = Vector3.right;
            right = right.normalized;
            Vector3 up = Vector3.Cross(direction, right).normalized;

            Vector2 size = Vector2.zero;
            size.x = Mathf.Abs(Vector3.Dot(right, bounds.size));
            size.y = Mathf.Abs(Vector3.Dot(up, bounds.size));
            
            transform.rotation = Quaternion.LookRotation(direction, up);
            transform.position += direction * distance * 0.5f;
            
            portal = gameObject.AddComponent<Portal>();
            portal.Setup(destination, size);

            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(size.x, size.y, Portal.PortalDepth * 2);
        }
    }
}
