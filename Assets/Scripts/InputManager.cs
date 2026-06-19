using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private InputActionReference _move;
    [SerializeField] private InputActionReference _jump;
    [SerializeField] private InputActionReference _run;

    public static InputManager Instance { get; private set; }
    public bool RunIsHeld { get; private set; }
    public bool JumpWasPressed { get; private set; }
    public bool JumpIsHeld { get; private set; }
    public bool JumpWasReleased { get; private set; }
    public float InputMoveDirection { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        InputMoveDirection = _move.action.ReadValue<float>();

        JumpWasPressed = _jump.action.WasPressedThisFrame();
        JumpIsHeld = _jump.action.IsPressed();
        JumpWasReleased = _jump.action.WasReleasedThisFrame();

        RunIsHeld = _run.action.IsPressed();
    }

    private void OnEnable()
    {
        _actions.Enable();
    }

    private void OnDisable()
    {
        _actions.Disable();
    }


}
