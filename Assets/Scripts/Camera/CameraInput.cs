using System;
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

        public event Action MouseLeftButtonClicked;

        public Vector2 MoveVector { get; private set; }
        public Vector2 MousePositionVector { get; private set; }

        private void OnEnable()
        {
            moveAction?.action.Enable();
            mousePositionAction?.action.Enable();
            mouseLeftButtonClickAction?.action.Enable();

            mouseLeftButtonClickAction.action.performed += OnMouseClick;
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
            mousePositionAction?.action.Disable();
            mouseLeftButtonClickAction?.action.Disable();

            mouseLeftButtonClickAction.action.performed -= OnMouseClick;
        }

        private void Update()
        {
            MoveVector = moveAction.action.ReadValue<Vector2>();
            MousePositionVector = mousePositionAction.action.ReadValue<Vector2>();
        }

        private void OnMouseClick(InputAction.CallbackContext context)
        {
            MouseLeftButtonClicked?.Invoke();
        }
    }
}

