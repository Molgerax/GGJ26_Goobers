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
    }
}
