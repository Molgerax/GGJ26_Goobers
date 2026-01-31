using UnityEngine;

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