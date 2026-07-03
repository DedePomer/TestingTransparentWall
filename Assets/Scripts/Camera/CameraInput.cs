using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Scripts.Camera
{
    public class CameraInput : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] private InputActionReference moveAction;

        public Vector2 MoveVector { get; private set; }

        private void OnEnable()
        {
            moveAction?.action.Enable();
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
        }

        private void Update()
        {
            MoveVector = moveAction.action.ReadValue<Vector2>();
        }
    }
}

