using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridInput : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private InputActionReference mousePositionAction;
    [SerializeField] private InputActionReference mouseLeftButtonClickAction;
    [SerializeField] private InputActionReference spaceButtonClickAction;

    public event Action OnMouseLeftButtonClicked;
    public event Action OnSpaceButtonClicked;

    public Vector2 MousePositionVector { get; private set; }

    private void OnEnable()
    {
        mousePositionAction?.action.Enable();
        mouseLeftButtonClickAction?.action.Enable();
        spaceButtonClickAction?.action.Enable();

        spaceButtonClickAction.action.performed += OnSpaceClick;
        mouseLeftButtonClickAction.action.performed += OnMouseClick;
    }

    private void OnDisable()
    {
        mousePositionAction?.action.Disable();
        mouseLeftButtonClickAction?.action.Disable();
        spaceButtonClickAction?.action.Disable();

        spaceButtonClickAction.action.performed -= OnSpaceClick;
        mouseLeftButtonClickAction.action.performed -= OnMouseClick;
    }

    private void Update()
    {
        MousePositionVector = mousePositionAction.action.ReadValue<Vector2>();
    }

    private void OnMouseClick(InputAction.CallbackContext context)
    {
        OnMouseLeftButtonClicked?.Invoke();
    }

    private void OnSpaceClick(InputAction.CallbackContext context)
    {
        OnSpaceButtonClicked?.Invoke();
    }
}
