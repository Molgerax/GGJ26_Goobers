using System;
using GGJ.Gameplay.Movement;
using GGJ.Mapping.PointEntities;
using GGJ.Utility;
using GGJ.Utility.Extensions;
using QuakeLR;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput = GGJ.Inputs.PlayerInput;

namespace GGJ.Gameplay.Player
{
    public class PlayerMovementController : MonoBehaviour, ITeleportable
    {
        #region Fields
        [SerializeField] private float lookSpeed = 5;
        [SerializeField] private Vector2 maxAngles = new(-80, 80);
        [SerializeField] private Transform cameraChild;
        [SerializeField] private Animator animator;

        [Header("Movement")] 
        [SerializeField] private float accelerationSpeed = 10;
        [SerializeField] private float stopSpeed = 5;
        
        [SerializeField] private float movementSpeed = 7f;
        [SerializeField] private float airControlSpeed = 2f;
        [SerializeField] private float jumpSpeed = 7f;
        [SerializeField] private float groundFriction = 7f;
        [SerializeField] private float airFriction = 7f;
        [SerializeField] private float gravity = 7f;
        [SerializeField] private bool useLocalMomentum;
        
        
        #endregion

        private CharacterMover _mover;
        private Transform _transform;

        private Vector3 _momentum, _savedVelocity, _savedMovementVelocity;

        private bool _isGrounded;
        private bool IsGrounded() => _isGrounded;

        private bool _isJumpQueued;

        private void Awake()
        {
            _transform = transform;
            _mover = GetComponent<CharacterMover>();
        }

        private void OnEnable()
        {
            PlayerInput.Input.Player.Jump.performed += OnJump;
        }

        private void OnDisable()
        {
            PlayerInput.Input.Player.Jump.performed -= OnJump;
        }
        
        private void OnJump(InputAction.CallbackContext context)
        {
            _isJumpQueued = true;
        }

        public Vector3 GetMomentum() => useLocalMomentum ? _transform.localToWorldMatrix * _momentum : _momentum;
        

        private void Update()
        {
            _mover.CheckForGround(Time.deltaTime);
            _isGrounded = _mover.IsGrounded();

            HandleMomentum(Time.deltaTime);

            _mover.SetExtendedSensorRange(IsGrounded());
            _mover.SetVelocity(_momentum);
            _mover.ComputeMovement(Time.deltaTime);

            _momentum = _mover.Velocity;
            
            _savedVelocity = _momentum;
            _savedMovementVelocity = CalculateMovementVelocity();

            HandleCameraRotation(Time.deltaTime);
        }


        private Vector3 CalculateMovementDirection()
        {
            Vector2 moveInput = PlayerInput.Move;
            Vector3 direction = _transform.right * moveInput.x + _transform.forward * moveInput.y;
            return direction.magnitude > 1f ? direction.normalized : direction;
        }
        
        private Vector3 CalculateMovementVelocity() => CalculateMovementDirection() * movementSpeed;

        private void GroundAccelerate(float deltaTime)
        {
            float alignment = Vector3.Dot(_momentum, CalculateMovementDirection().normalized);
            float addSpeed = movementSpeed - alignment;
            if (addSpeed <= 0)
                return;

            float accelSpeed = Mathf.Min(accelerationSpeed * deltaTime * movementSpeed, addSpeed);
            _momentum += accelSpeed * CalculateMovementDirection().normalized.With(y:0);
        }
        
        private void AirAccelerate(float deltaTime)
        {
            float wishSpeed = Mathf.Min(CalculateMovementVelocity().magnitude, 1.07f); //1.07f is 30.0f in Quake Units
            float alignment = Vector3.Dot(_momentum, CalculateMovementDirection().normalized);

            float addSpeed = wishSpeed - alignment;
            if (addSpeed <= 0.0f)
                return;

            float accelSpeed = Mathf.Min(accelerationSpeed * wishSpeed * deltaTime, addSpeed);
            _momentum += CalculateMovementVelocity() * accelSpeed;
        }
        
        private void UserFriction(float deltaTime)
        {
            float speed = _momentum.With(y: 0).magnitude;
            if (speed < 0.01f)
            {
                _momentum = _momentum.With(x:0, z:0);
                return;
            }

            float drop = 0.0f;
            if (IsGrounded() && !_isJumpQueued)
            {
                float control = speed < stopSpeed ? stopSpeed : speed;
                drop += control * groundFriction * deltaTime;
            }

            float newSpeed = Mathf.Max(speed - drop, 0.0f) / speed;
            _momentum *= newSpeed;
        }
        
        private void HandleMomentum(float deltaTime)
        {
            if (useLocalMomentum)
                _momentum = _transform.localToWorldMatrix * _momentum;
            
            HandleJumping(deltaTime);

            UserFriction(deltaTime);
            
            if (IsGrounded())
                GroundAccelerate(deltaTime);
            else
                AirAccelerate(deltaTime);
            

            if (useLocalMomentum)
                _momentum = _transform.worldToLocalMatrix * _momentum;
            
            if (animator)
                animator.SetBool("Walk", CalculateMovementDirection().magnitude > 0.1f);
        }


        private void HandleJumping(float deltaTime)
        {
            if (IsGrounded())
            {
                if (_isJumpQueued)
                {
                    _momentum = VectorMath.RemoveDotVector(_momentum, _transform.up);
                    _momentum += _transform.up * jumpSpeed;
                    _isGrounded = false;
                }
                else
                {
                    _momentum = VectorMath.RemoveDotVector(_momentum, _transform.up);
                }
            }
            else
            {
                _momentum -= _transform.up * gravity * deltaTime;
            }

            _isJumpQueued = false;
        }
        

        private void AdjustHorizontalMomentum(ref Vector3 horizontalMomentum, Vector3 movementVelocity, float deltaTime)
        {
            if (horizontalMomentum.magnitude > movementSpeed)
            {
                if (VectorMath.GetDotProduct(movementVelocity, horizontalMomentum) > 0f)
                    movementVelocity = VectorMath.RemoveDotVector(movementVelocity, horizontalMomentum);
                horizontalMomentum += movementVelocity * (deltaTime * airControlSpeed);
            }
            else
            {
                horizontalMomentum += movementVelocity * (deltaTime * airControlSpeed);
                horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, movementSpeed);
            }
        }
        
        
        private void HandleCameraRotation(float deltaTime)
        {
            Vector2 lookInput = PlayerInput.Look;
            
            transform.Rotate(Vector3.up, lookInput.x * lookSpeed * deltaTime);

            float pitch = cameraChild.localEulerAngles.x;
            pitch -= lookInput.y * lookSpeed * deltaTime;

            if (pitch > 180)
                pitch -= 360;
            
            pitch = Mathf.Clamp(pitch, maxAngles.x, maxAngles.y);

            cameraChild.localEulerAngles = new(pitch, 0, 0);
        }
        
        public void ToggleMovementForDialogue(bool isPaused)
        {
            if (isPaused)
            {
                // disable movement
            }
            else
            {
                // enable movement
            }
        }

        public void Teleport(InfoTeleportDestination destination, TeleportData data)
        {
            Transform t = transform;
            Vector3 localVelocity = t.InverseTransformVector(_momentum);

            Vector3 fwd = t.forward;
            
            if (destination.UseRelativeRotation)
                localVelocity = Quaternion.Inverse(data.RelativeRotation) * _momentum;
            
            Transform destinationTransform = destination.transform;
            _momentum = destinationTransform.TransformVector(localVelocity);

            Quaternion inverse = Quaternion.Inverse(data.RelativeRotation) * t.rotation;

            Vector3 localPosition = t.position - data.RelativePosition;
            localPosition = Quaternion.Inverse(data.RelativeRotation) * localPosition;
            
            // Disable
            //_mover.Rigidbody.Sleep();
            
            if (destination.UseRelativePosition)
                t.position = destinationTransform.position + destinationTransform.rotation * localPosition;
            else
                t.position = destinationTransform.position;

            if (destination.UseRelativeRotation)
                t.rotation = destinationTransform.rotation * inverse;
            else
                t.rotation = destinationTransform.rotation;

            Vector3 newForward = t.forward;
            if (newForward == Vector3.up)
                newForward = fwd;
            
            t.rotation = Quaternion.LookRotation(newForward);
            
            // Enable
            //_mover.Rigidbody.WakeUp();
        }
    }
}
