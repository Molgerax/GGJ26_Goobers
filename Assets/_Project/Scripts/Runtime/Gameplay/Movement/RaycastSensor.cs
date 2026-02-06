using UnityEngine;

namespace GGJ.Gameplay.Movement
{
    public class RaycastSensor
    {
        public float CastLength = 1f;
        public LayerMask LayerMask = 255;
        
        private Vector3 _origin = Vector3.zero;
        private Transform _transform;

        private Vector3 _castDirection = Vector3.down;

        private RaycastHit _hitInfo;
        private bool _hasHitDetected;

        public RaycastSensor(Transform transform, Vector3 offset = default, Vector3 direction = default)
        {
            _transform = transform;
            _origin = offset;
            _castDirection = direction.magnitude > 0f ? direction : Vector3.down;
        }

        public void Cast()
        {
            Vector3 worldOrigin = _transform.TransformPoint(_origin);
            Vector3 worldDirection = _transform.TransformDirection(_castDirection);

            _hasHitDetected = Physics.Raycast(worldOrigin, worldDirection, out _hitInfo, CastLength, LayerMask,
                QueryTriggerInteraction.Ignore);
        }
        
        public void SetCastDirection(Vector3 direction) => _castDirection = direction;
        public void SetCastOrigin(Vector3 pos) => _origin = _transform.InverseTransformPoint(pos);
        
        public bool HasDetectedHit() => _hasHitDetected;
        public float GetDistance() => _hitInfo.distance;

        public bool TryGetHitInfo(out RaycastHit hitInfo)
        {
            hitInfo = _hitInfo;
            return _hasHitDetected;
        }

        public bool TryGetComponent<T>(out T component) where T : class
        {
            component = null;
            if (!_hasHitDetected)
                return false;

            return _hitInfo.collider.TryGetComponent(out component);
        }


        public void DrawDebug()
        {
            Debug.DrawRay(_transform.TransformPoint(_origin), _transform.TransformDirection(_castDirection) * CastLength, Color.blue, Time.deltaTime);
            
            if (!HasDetectedHit())
                return;
            
            Debug.DrawRay(_hitInfo.point, _hitInfo.normal, Color.green, Time.deltaTime);
            float markerSize = 0.2f;
            Debug.DrawLine(_hitInfo.point + Vector3.up * markerSize, _hitInfo.point - Vector3.up * markerSize, Color.red, Time.deltaTime);
            Debug.DrawLine(_hitInfo.point + Vector3.right * markerSize, _hitInfo.point - Vector3.right * markerSize, Color.red, Time.deltaTime);
            Debug.DrawLine(_hitInfo.point + Vector3.forward * markerSize, _hitInfo.point - Vector3.forward * markerSize, Color.red, Time.deltaTime);
        }
    }
}