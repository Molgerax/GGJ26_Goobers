using GGJ.Utility;
using GGJ.Utility.Extensions;
using UnityEngine;

namespace GGJ.Gameplay.Movement
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class CharacterMover : MonoBehaviour, IMovable
    {
        #region Fields
        [Header("Collider Settings")] 
        [Range(0f, 1f)] [SerializeField] private float stepHeightRatio = 0.1f;
        [SerializeField] private float colliderHeight = 2f;
        [SerializeField] private float colliderThickness = 1f;
        [SerializeField] private Vector3 colliderOffset = Vector3.zero;

        [SerializeField] private float skinWidth = 0.015f;
        [SerializeField] private float maxSlopeAngle = 55;

        private Rigidbody _rb;
        private Transform _transform;
        private CapsuleCollider _collider;
        private RaycastSensor _sensor;

        private bool _isGrounded;
        private float _baseSensorRange;
        private Vector3 _currentGroundAdjustmentVelocity; // velocity to adjust player position to maintain ground contact
        private int _currentLayer;


        [Header("Sensor Settings")] 
        [SerializeField] private bool isInDebugMode;
        [SerializeField] private bool isUsingExtendedSensorRange = true;
        
        #endregion


        struct DebugDrawInfo
        {
            public bool IsActive;
            public Vector3 Position;
            public Vector3 Normal;
        }

        private DebugDrawInfo[] _drawInfos = new DebugDrawInfo[6];
        
        
        public Rigidbody Rigidbody => _rb;
        
        private IMover _cachedMover;
        
        private void Awake()
        {
            Setup();
            RecalculateColliderDimensions();
        }

        private void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                RecalculateColliderDimensions();
            }
        }

        #region Setup
        
        private void Setup()
        {
            _transform = transform;
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();

            if (!_rb)
                return;
            
            _rb.freezeRotation = true;
            _rb.useGravity = false;
        }

        private void RecalculateSensorLayerMask()
        {
            int objectLayer = gameObject.layer;
            int layerMask = Physics.AllLayers;

            for (int i = 0; i < 32; i++)
            {
                if (Physics.GetIgnoreLayerCollision(objectLayer, i))
                    layerMask &= ~(1 << i);
            }

            int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            layerMask &= ~(1 << ignoreRaycastLayer);

            layerMask |= _collider.includeLayers;
            layerMask &= ~_collider.excludeLayers;

            _sensor.LayerMask = layerMask;
            _currentLayer = objectLayer;
        }

        
        private void RecalibrateSensor()
        {
            _sensor ??= new RaycastSensor(_transform);
            
            _sensor.SetCastOrigin(_collider.bounds.center);
            _sensor.SetCastDirection(Vector3.down);
            RecalculateSensorLayerMask();

            const float safetyDistanceFactor = 0.001f;

            float length = colliderHeight * (1f - stepHeightRatio) * 0.5f + colliderHeight * stepHeightRatio;
            _baseSensorRange = length * (1f + safetyDistanceFactor) * _transform.localScale.y;
            _sensor.CastLength = length * _transform.localScale.y;
        }
        
        private void RecalculateColliderDimensions()
        {
            if (_collider == null)
                Setup();

            _collider.height = colliderHeight * (1f - stepHeightRatio);
            _collider.radius = colliderThickness / 2f;
            _collider.center = colliderOffset.With(y: colliderOffset.y * colliderHeight) 
                               + new Vector3(0f, stepHeightRatio * colliderHeight / 2f, 0f);

            if (_collider.height / 2f < _collider.radius)
                _collider.radius = _collider.height / 2f;

            RecalibrateSensor();
        }
        #endregion
        
        public void CheckForGround(float deltaTime)
        {
            if (_currentLayer != gameObject.layer)
                RecalculateSensorLayerMask();
            
            _currentGroundAdjustmentVelocity = Vector3.zero;
            _sensor.CastLength = isUsingExtendedSensorRange
                ? _baseSensorRange + colliderHeight * _transform.localScale.y * stepHeightRatio
                : _baseSensorRange;
            _sensor.Cast();

            _isGrounded = _sensor.TryGetHitInfo(out RaycastHit hitInfo);

            GetMoverBelow();

            if (!_isGrounded)
                return;

            float distance = hitInfo.distance;
            float upperLimit = colliderHeight * _transform.localScale.y * (1f - stepHeightRatio) * 0.5f;
            float middle = upperLimit + colliderHeight * _transform.localScale.y * stepHeightRatio;
            float distanceToGo = middle - distance;

            //_currentGroundAdjustmentVelocity = _transform.up * (distanceToGo / deltaTime);
            
            //Debug.Log($"DistanceToGo: {distanceToGo}");
            transform.position += _transform.up * distanceToGo;
        }

        private void GetMoverBelow()
        {
            _sensor.TryGetComponent(out IMover mover);
                
            if (_cachedMover != mover)
            {
                _cachedMover?.RemoveMovable(this);
                _cachedMover = mover;
                _cachedMover?.AddMovable(this);
            }
        }

        public Vector3 Velocity;

        private Vector3 _velocity;
        
        public void SetVelocity(Vector3 velocity)
        {
            //_rb.linearVelocity = velocity + _currentGroundAdjustmentVelocity;
            Velocity = velocity;
            _velocity = Velocity + _currentGroundAdjustmentVelocity;
        }

        public void ComputeMovement(float deltaTime)
        {
            if (_velocity.magnitude == 0)
                return;
            
            int maxBounces = 5;

            float radius = _collider.radius;
            Vector3 up = Vector3.up * (_collider.height * 0.5f - _collider.radius);
            Vector3 worldPos = transform.position + _collider.center - up;

            Vector3 moveStep = CollideAndSlide(_velocity.With(y:0) * deltaTime, worldPos, up, radius, false, _velocity.With(y:0) * deltaTime, maxBounces);
            Vector3 gravity = CollideAndSlide(_velocity.With(x:0,z:0) * deltaTime, worldPos, up, radius, true, _velocity.With(x:0,z:0) * deltaTime, maxBounces);

            moveStep += gravity;
            
            _velocity = moveStep / deltaTime;

            Velocity = _velocity - _currentGroundAdjustmentVelocity;
            
            if (_velocity.magnitude > 0)
                transform.position += moveStep;
        }

        
        private Vector3 CollideAndSlide(Vector3 velocity, Vector3 pos, Vector3 up, float radius, bool gravityPass, Vector3 velocityInit, int depth)
        {
            if (depth < 0)
                return Vector3.zero;

            if (velocity.magnitude == 0)
                return Vector3.zero;
            
            float dist = velocity.magnitude + skinWidth;

            if (Physics.CapsuleCast(pos, pos + up * 2, radius, velocity.normalized, out var hit, dist, _sensor.LayerMask, QueryTriggerInteraction.Ignore))
            {
                _drawInfos[depth] = new DebugDrawInfo()
                {
                    IsActive = true,
                    Position = hit.point,
                    Normal = hit.normal
                };
                
                Vector3 snapToSurface = velocity.normalized * (hit.distance - skinWidth);
                Vector3 leftover = velocity - snapToSurface;
                float angle = Vector3.Angle(_transform.up, hit.normal);
                
                if (snapToSurface.magnitude <= skinWidth)
                    snapToSurface = Vector3.zero;

                // normal ground / slope
                if (angle <= maxSlopeAngle)
                {
                    if (gravityPass)
                        return snapToSurface;
                    leftover = VectorMath.ProjectToPlaneAndScale(leftover, hit.normal);
                }
                // wall or steep slope
                else
                {
                    float scale = 1 - Vector3.Dot(
                        hit.normal.With(y: 0).normalized,
                        -velocityInit.With(y: 0).normalized);
                    leftover = VectorMath.ProjectToPlaneAndScale(leftover, hit.normal) * scale;
                }
                
                return snapToSurface + CollideAndSlide(leftover, pos + snapToSurface, up, radius, gravityPass, velocityInit, depth - 1);
            }
            else
            {
                _drawInfos[depth] = new DebugDrawInfo()
                {
                    IsActive = false
                };
            }
            
            return velocity;
        }

        public bool IsGrounded() => _isGrounded;

        public Vector3 GetGroundNormal()
        {
            _sensor.TryGetHitInfo(out var hitInfo);
            return hitInfo.normal;
        }
        
        public void SetExtendedSensorRange(bool isExtended) => isUsingExtendedSensorRange = isExtended;

        private Vector3 _cachedDisplacement;
        
        
        public void Move(Vector3 displacement)
        {
            transform.position += displacement;
        }


        private void OnDrawGizmos()
        {
            for (int i = 0; i < _drawInfos.Length; i++)
            {
                DebugDrawInfo info = _drawInfos[i];
                if (!info.IsActive)
                    continue;

                Gizmos.color = Color.HSVToRGB((float) i / _drawInfos.Length, 1, 1);
                Gizmos.DrawWireSphere(info.Position, 0.1f);
                Gizmos.DrawRay(info.Position, info.Normal * 0.2f);
            }
        }
    }
}
