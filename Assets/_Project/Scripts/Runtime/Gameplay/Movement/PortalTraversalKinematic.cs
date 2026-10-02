using GGJ.Rendering.Portals;
using GGJ.Utility.Extensions;
using UnityEngine;

namespace GGJ.Gameplay.Movement
{
    [DefaultExecutionOrder(100)]
    public class PortalTraversalKinematic : MonoBehaviour
    {
        [SerializeField, Range(1, 16)] private int maxColliderChecks = 8;
        
        private ITeleportable _teleportable;

        private Vector3 _position;
        private Vector3 _previousPosition;

        private RaycastHit[] _hits;
        
        private void Awake()
        {
            _teleportable = GetComponent<ITeleportable>();

            _hits = new RaycastHit[maxColliderChecks];
            
            _previousPosition = transform.position;
            _position = transform.position;
        }


        private void Update()
        {
            if(_teleportable == null)
                return;
            
            _previousPosition = _position;
            _position = transform.position;
            
            DetectPortalStepThrough();
        }

        private void DetectPortalStepThrough()
        {
            Vector3 moveStep = _position - _previousPosition;
            Ray ray = new(_previousPosition, moveStep);

            int count = Physics.RaycastNonAlloc(ray, _hits, moveStep.magnitude, int.MaxValue,
                QueryTriggerInteraction.Collide);
            
            for (int i = 0; i < count; i++)
            {
                if (_hits[i].collider.TryGetComponent(out Portal portal) && portal.Passable && portal.OtherPortal)
                {
                    if (portal.transform.IsInFrontOf(_previousPosition) && !portal.transform.IsInFrontOf(_position))
                    {
                        _teleportable.Teleport(portal.OtherPortal, portal.GetTeleportData());
                    }
                }
            }
        }
    }
}