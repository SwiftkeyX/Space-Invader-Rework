using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Single read point for player input. Exposes horizontal move intent and fire
/// intent via the new Input System (action-based, no legacy UnityEngine.Input).
/// Horizontal only — there is no vertical axis (D1/N1). See InputManager.md GDD.
///
/// Implementation note: actions are defined inline (code-built InputActions)
/// rather than an .inputactions asset. This keeps the fixed PC control scheme
/// self-contained; migrate to an Action Asset if a rebinding UI is added later.
/// </summary>
public class InputManager : MonoBehaviour
{
    private InputAction _moveAction;
    private InputAction _fireAction;

    /// <summary>Horizontal move intent, -1 (left) .. +1 (right).</summary>
    public float MoveAxis { get; private set; }

    /// <summary>True while a fire input is held.</summary>
    public bool FireHeld { get; private set; }

    /// <summary>Raised once on the frame fire is first pressed.</summary>
    public event Action OnFirePressed;

    /// <summary>When false, intent reads neutral (menus / game-over / pause).</summary>
    public bool InputEnabled { get; set; } = true;

    private void Awake()
    {
        _moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Axis");
        _moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");
        _moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/rightArrow");

        _fireAction = new InputAction("Fire", InputActionType.Button);
        _fireAction.AddBinding("<Keyboard>/space");
        _fireAction.AddBinding("<Mouse>/leftButton");
        _fireAction.performed += OnFirePerformed;
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _fireAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _fireAction.Disable();
    }

    private void OnDestroy()
    {
        _fireAction.performed -= OnFirePerformed;
        _moveAction?.Dispose();
        _fireAction?.Dispose();
    }

    private void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        if (InputEnabled) OnFirePressed?.Invoke();
    }

    private void Update()
    {
        if (!InputEnabled)
        {
            MoveAxis = 0f;
            FireHeld = false;
            return;
        }

        MoveAxis = _moveAction.ReadValue<float>();
        FireHeld = _fireAction.IsPressed();
    }
}
