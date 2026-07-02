using System;
using UnityEngine;

public class AirborneState : IState
{
    private Vector2 Velocity;
    private float _moveInput;
    private bool _runHeld;
    private MovementController _controller;
    private PlayerMovementStats _moveStats;

    public AirborneState(MovementController controller, PlayerMovementStats moveStats)
    {
        _controller = controller;
        _moveStats = moveStats;
    }

    public void OnEnter()
    {
        throw new System.NotImplementedException();
    }

    public void Execute(float timeStep)
    {
        _moveInput = InputManager.Instance.InputMoveDirection;
        _runHeld = InputManager.Instance.RunIsHeld;
        HandleHorizontalMovement(timeStep);

        _controller.Move(Velocity * timeStep);
    }

    private void HandleHorizontalMovement(float timeStep)
    {
        float targetVelocityX = 0f;

        float moveDirection = Mathf.Sign(_moveInput);
        targetVelocityX = _runHeld ? moveDirection * _moveStats.MaxRunSpeed : moveDirection * _moveStats.MaxWalkSpeed;

        float acceleration = _moveStats.AirAcceleration;
        float deceleration = _moveStats.AirDeceleration;

        if (_moveInput != 0)
        {
            Velocity.x = Mathf.Lerp(Velocity.x, targetVelocityX, acceleration * timeStep);
        }
        else
        {
            Velocity.x = Mathf.Lerp(Velocity.x, 0, deceleration * timeStep);
        }
    }

    public void OnExit()
    {
        throw new System.NotImplementedException();
    }
}
