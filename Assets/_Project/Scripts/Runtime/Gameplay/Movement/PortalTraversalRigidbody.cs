using System;
using GGJ.Rendering.Portals;
using GGJ.Utility;
using GGJ.Utility.Extensions;
using UnityEngine;

namespace GGJ.Gameplay.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class PortalTraversalRigidbody : MonoBehaviour, ITeleportable
    {
        [SerializeField, Range(1, 16)] private int maxColliderChecks = 8;
        private Rigidbody _rb;
        private Vector3 _previousPosition;

        private RaycastHit[] _hits;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _hits = new RaycastHit[maxColliderChecks];
            
            _previousPosition = _rb.position;
        }

        private void FixedUpdate()
        {
            Vector3 pos = _rb.position;
            Vector3 previousPosition = _previousPosition;
            
            _previousPosition = _rb.position;
            
            DetectPortalStepThrough(pos, previousPosition);
        }

        private void DetectPortalStepThrough(Vector3 pos, Vector3 prevPos)
        {
            Vector3 moveStep = pos - prevPos;
            Ray ray = new(prevPos, moveStep);

            int count = Physics.RaycastNonAlloc(ray, _hits, moveStep.magnitude, int.MaxValue,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                if (_hits[i].collider.TryGetComponent(out Portal portal) && portal.Passable && portal.OtherPortal)
                {
                    if (portal.transform.IsInFrontOf(ray.origin) && portal.transform.IsBehind(transform.position))
                    {
                        this.Teleport(portal.OtherPortal, portal.GetTeleportData());
                    }
                }
            }
        }

        public void Teleport(ITeleportDestination destination, TeleportData data)
        {
            Transform t = transform;
            Vector3 localVelocity = t.InverseTransformVector(_rb.linearVelocity);
            Vector3 localAngular = t.InverseTransformVector(_rb.angularVelocity);
            
            if (destination.UseRelativeRotation)
                localVelocity = Quaternion.Inverse(data.RelativeRotation) * _rb.linearVelocity;
            if (destination.UseRelativeRotation)
                localAngular = Quaternion.Inverse(data.RelativeRotation) * _rb.angularVelocity;

            
            
            _rb.Sleep();
            
            ScaledPose destinationTransform = destination.Transform;
            _rb.linearVelocity = destinationTransform.TransformVector(localVelocity);
            _rb.angularVelocity = destinationTransform.TransformVector(localAngular);
            
            Quaternion inverse = Quaternion.Inverse(data.RelativeRotation) * t.rotation;

            Vector3 localPosition = t.position - data.RelativePosition;
            localPosition = Quaternion.Inverse(data.RelativeRotation) * localPosition;
            
            if (destination.UseRelativePosition)
                _rb.position = destinationTransform.position + destinationTransform.rotation * localPosition;
            else
                _rb.position = destinationTransform.position;

            if (destination.UseRelativeRotation)
                _rb.rotation = destinationTransform.rotation * inverse;
            else
                _rb.rotation = destinationTransform.rotation;
            
            _rb.WakeUp();
        }
    }
}
