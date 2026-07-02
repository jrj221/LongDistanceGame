using UnityEngine;

[RequireComponent(typeof(MovementController))]
public class StateMachine : MonoBehaviour
{
    private MovementController _controller;
    [SerializeField] private PlayerMovementStats _moveStats;

    private IState _lastState;
    private IState _currentState;
    private MoveState _moveState;
    private JumpState _jumpState;
    private AirborneState _airborneState;
    private IdleState _idleState;

    private float _moveInput;
    private bool _jumpPressed;
    private bool _jumpReleased;

    private void Awake()
    {
        _controller = GetComponent<MovementController>();

        _moveState = new(_controller, _moveStats);
        _airborneState = new();
        _jumpState = new();
    }

    private void Update()
    {
        _moveInput = InputManager.Instance.InputMoveDirection;
        if (InputManager.Instance.JumpWasPressed) _jumpPressed = true;
        if (InputManager.Instance.JumpWasReleased) _jumpReleased = true;

        _lastState = _currentState;
        _currentState = ChooseNextState();
    }

    private void FixedUpdate()
    {
        if (_currentState != _lastState)
        {
            _lastState.OnExit();
            _currentState.OnEnter();
        }
        _currentState.Execute(Time.fixedDeltaTime);
    }

    private IState ChooseNextState()
    {
        if (!_controller.IsGrounded())
        {
            return _airborneState;
        }
        if (_moveInput != 0)
        {
            return _moveState;
        }
        else
        {
            return _idleState;
        }
    }
}
