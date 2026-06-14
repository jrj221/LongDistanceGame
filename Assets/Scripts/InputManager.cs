using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private InputActionReference _move;
    
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
    }

    private void OnDisable()
    {
        _actions.Disable();
    }
}
