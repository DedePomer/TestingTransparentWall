using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridInput : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private InputActionReference mousePositionAction;
    [SerializeField] private InputActionReference mouseLeftButtonClickAction;
    [SerializeField] private InputActionReference mouseRightButtonClickAction;
    [SerializeField] private InputActionReference spaceButtonClickAction;

    public event Action OnMouseLeftButtonClicked;
    public event Action OnMouseRightButtonClicked;
    public event Action OnSpaceButtonClicked;

    public Vector2 MousePositionVector { get; private set; }

    private void OnEnable()
    {
        mousePositionAction?.action.Enable();
        mouseLeftButtonClickAction?.action.Enable();
        spaceButtonClickAction?.action.Enable();
        mouseRightButtonClickAction?.action.Enable();

        spaceButtonClickAction.action.performed += OnSpaceClick;
        mouseRightButtonClickAction.action.performed += OnMouseRightClick;
        mouseLeftButtonClickAction.action.performed += OnMouseLeftClick;
    }

    private void OnDisable()
    {
        mousePositionAction?.action.Disable();
        mouseLeftButtonClickAction?.action.Disable();
        spaceButtonClickAction?.action.Disable();
        mouseRightButtonClickAction?.action.Disable();

        spaceButtonClickAction.action.performed -= OnSpaceClick;
        mouseRightButtonClickAction.action.performed -= OnMouseRightClick;
        mouseLeftButtonClickAction.action.performed -= OnMouseLeftClick;
    }

    private void Update()
    {
        MousePositionVector = mousePositionAction.action.ReadValue<Vector2>();
    }

    private void OnMouseLeftClick(InputAction.CallbackContext context)
    {
        OnMouseLeftButtonClicked?.Invoke();
    }

    private void OnMouseRightClick(InputAction.CallbackContext context)
    {
        OnMouseRightButtonClicked?.Invoke();
    }

    private void OnSpaceClick(InputAction.CallbackContext context)
    {
        OnSpaceButtonClicked?.Invoke();
    }
}
