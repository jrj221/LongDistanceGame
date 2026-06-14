using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private InputActionReference _move;
    [SerializeField] private InputActionReference _jump;
    
    public static InputManager Instance { get; private set; }
    public float InputMoveDirection {get; private set;}

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        InputMoveDirection = _move.action.ReadValue<float>();
    }

    private void OnEnable()
    {
        _actions.Enable();
        _jump.action.started += PerformJump;
    }

    private void OnDisable()
    {
        _actions.Disable();
        _jump.action.started -= PerformJump;
    }

    private void PerformJump(InputAction.CallbackContext ctx)
    {
        EventBus.Trigger("PerformJump");
    }
}
