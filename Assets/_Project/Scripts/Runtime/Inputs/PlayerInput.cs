using UnityEngine;
using UnityEngine.InputSystem;

namespace GGJ.Inputs
{
    [DefaultExecutionOrder(-100)]
    public class PlayerInput : MonoBehaviour
    {
        public static PlayerInput Instance;


        private static PlayerInputActions _input;
        
        public static PlayerInputActions Input
        {
            get
            {
                if (_input == null)
                    InitInputs();
                return _input;
            }
            private set
            {
                _input = value;
            }
        }



        #region Properties

        public static Vector2 Move => Input.Player.Move.ReadValue<Vector2>();
        public static Vector2 Look => Input.Player.Look.ReadValue<Vector2>();

        #endregion

        private static void InitInputs()
        {
            _input = new PlayerInputActions();
            _input.Enable();
        }

        public static void SetCursorLocked(bool value)
        {
            if (value)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.None;
        }

        public static void SetMoveInputs(bool value)
        {
            SetActionEnable(_input.Player.Crouch, value);
            SetActionEnable(_input.Player.Move, value);
            SetActionEnable(_input.Player.Look, value);
            SetActionEnable(_input.Player.Jump, value);
        }

        private static void SetActionEnable(InputAction action, bool value)
        {
            if (value)
                action.Enable();
            else 
                action.Disable();
        }
        

        private void Awake()
        {
            Instance = this;
            InitInputs();

            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}