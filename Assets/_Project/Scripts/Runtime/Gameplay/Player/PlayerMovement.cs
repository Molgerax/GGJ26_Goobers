using QuakeLR;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput = GGJ.Inputs.PlayerInput;

namespace GGJ.Gameplay.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float lookSpeed = 5;

        [SerializeField] private Vector2 maxAngles = new(-80, 80);
        
        [SerializeField] private Transform cameraChild;

        [SerializeField] private bool snapToGround = false;
        [SerializeField, Min(0)] private float snapDistance = 0.1f;
        
        private QuakeCharacterController _quakeController;
        
        private void Awake()
        {
            _quakeController = GetComponent<QuakeCharacterController>();
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
            _quakeController.TryJump();
        }

        private void Update()
        {
            HandleCameraRotation();
            HandleMovement();
            
            if (snapToGround)
                SnapToGround();
        }


        private void HandleMovement()
        {
            Vector2 moveInput = PlayerInput.Move;
            Vector3 moveDir = transform.rotation * new Vector3(moveInput.x, 0, moveInput.y);
            
            _quakeController.Move(moveDir);
            _quakeController.ControllerThink(Time.deltaTime);
        }


        private void HandleCameraRotation()
        {
            Vector2 lookInput = PlayerInput.Look;
            
            transform.Rotate(Vector3.up, lookInput.x * lookSpeed * Time.deltaTime);

            float pitch = cameraChild.localEulerAngles.x;
            pitch -= lookInput.y * lookSpeed * Time.deltaTime;

            if (pitch > 180)
                pitch -= 360;
            
            pitch = Mathf.Clamp(pitch, maxAngles.x, maxAngles.y);

            cameraChild.localEulerAngles = new(pitch, 0, 0);
        }

        private void SnapToGround()
        {
            if (_quakeController.OnGround || _quakeController.Velocity.y > 0.1f)
                return;

            Vector3 pos = transform.position + _quakeController.CharacterController.center;
            pos += Vector3.down * (_quakeController.CharacterController.height - snapDistance);
            Ray ray = new Ray(pos, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, snapDistance * 2, _quakeController.GroundLayers, QueryTriggerInteraction.Ignore))
            {
                transform.position = hitInfo.point;
            }
        }
    }
}
