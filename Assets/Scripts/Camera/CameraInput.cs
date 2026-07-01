using UnityEngine;
using UnityEngine.InputSystem;


namespace Scripts.Camera
{
    public class CameraInput : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference mousePositionAction;
        [SerializeField] private InputActionReference mouseLeftButtonClickAction;

        public Vector2 MoveVector { get; private set; }
        public Vector2 MousePositionVector { get; private set; }
        public bool IsMouseLeftButtonPressed { get; private set; }

        private void OnEnable()
        {
            moveAction?.action.Enable();
            mousePositionAction?.action.Enable();
            mouseLeftButtonClickAction?.action.Enable();
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
            mousePositionAction?.action.Disable();
            mouseLeftButtonClickAction?.action.Disable();
        }

        private void Update()
        {
            MoveVector = moveAction.action.ReadValue<Vector2>();
            MousePositionVector = mousePositionAction.action.ReadValue<Vector2>();
            IsMouseLeftButtonPressed = mouseLeftButtonClickAction.action.IsPressed();
        }
    }
}

