using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Collider2D _collider;

    private Rigidbody2D _rb;

    // Movement vars
    public bool IsFacingRight { get; private set; }
    public MovementController Controller;
    [HideInInspector] public Vector2 Velocity;

    //input
    private float _moveInput;
    private bool _runHeld;
    private bool _jumpPressed;
    private bool _jumpReleased;

    // Jump vars
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    // Apex vars
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    // Jump buffer vars
    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;

    // Coyote time vars
    private float _coyoteTimer;

    private void Awake()
    {
        IsFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
        Controller = GetComponent<MovementController>();
    }

    private void Update()
    {
        _moveInput = InputManager.Instance.InputMoveDirection;
        _runHeld = InputManager.Instance.RunIsHeld;
        if (InputManager.Instance.JumpWasPressed) _jumpPressed = true;
        if (InputManager.Instance.JumpWasReleased) _jumpReleased = true;
    }

    private void FixedUpdate()
    {
        CountTimers(Time.fixedDeltaTime);
        JumpChecks();
        HandleHorizontalMovement(Time.fixedDeltaTime);

        Jump(Time.fixedDeltaTime);

        ClampVelocity();

        Controller.Move(Velocity * Time.fixedDeltaTime);

        // Reset inputs
        _jumpPressed = false;
        _jumpReleased = false;
    }

    private void ClampVelocity()
    {
        // Clamp Fall Speed
        Velocity.y = Mathf.Clamp(Velocity.y, -MoveStats.MaxFallSpeed, 50f);
    }

    #region Movement

    private void HandleHorizontalMovement(float timeStep)
    {
        TurnCheck(_moveInput);
        float targetVelocityX = 0f;

        float moveDirection = Mathf.Sign(_moveInput);
        targetVelocityX = _runHeld ? moveDirection * MoveStats.MaxRunSpeed : moveDirection * MoveStats.MaxWalkSpeed;

        float acceleration = Controller.IsGrounded() ? MoveStats.GroundAcceleration : MoveStats.AirAcceleration;
        float deceleration = Controller.IsGrounded() ? MoveStats.GroundDeceleration : MoveStats.AirDeceleration;

        if (_moveInput != 0)
        {
            Velocity.x = Mathf.Lerp(Velocity.x, targetVelocityX, acceleration * timeStep);
        }
        else
        {
            Velocity.x = Mathf.Lerp(Velocity.x, 0, deceleration * timeStep);
        }
    }

    private void TurnCheck(float moveInput)
    {
        if (IsFacingRight && moveInput < 0)
        {
            Turn(false);
        }

        else if (!IsFacingRight && moveInput > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            IsFacingRight = true;
            // transform.Rotate(0f, 100f, 0f);
        }
        else
        {
            IsFacingRight = false;
            // transform.Rotate(0f, -100f, 0f);
        }
    }

    #endregion

    #region Jump

    private void JumpChecks()
    {
        // When we press the button
        if (_jumpPressed)
        {
            _jumpBufferTimer = MoveStats.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }

        // When we release jump
        if (InputManager.Instance.JumpWasReleased)
        {
            if (_jumpBufferTimer > 0f)
            {
                _jumpReleasedDuringBuffer = true;
            }

            if (_isJumping && Velocity.y > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MoveStats.TimeForUpwardsCancel;
                    Velocity.y = 0f; // To prevent feeling floaty
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = Velocity.y;
                }
            }
        }

        // intiiate jump wit buffering and coyoto
        if (_jumpBufferTimer > 0f && !_isJumping && (Controller.IsGrounded() || _coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (_jumpReleasedDuringBuffer) // So they just do a bunny hop when hitting the ground if they tapped the jump key
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = Velocity.y;
            }
        }

        // double jump
        else if (_jumpBufferTimer > 0f && _isJumping && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }

        // air jump after coyotoe time lapsed 
        else if (_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1) // AIR jump
        {
            InitiateJump(2);
            _isFastFalling = false;
        }

        // landed 
        if ((_isJumping || _isFalling) && Controller.IsGrounded() && Velocity.y <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;

            Velocity.y = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int numberOfJumpsUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        Velocity.y = MoveStats.InitialJumpVelocity;
    }

    private void Jump(float timeStep)
    {
        // apply gravity while jumping
        if (_isJumping)
        {
            // check fo rhead bump
            if (Controller.BumpedHead())
            {
                _isFastFalling = true;
            }

            // gravity on ascending
            if (Velocity.y >= 0f)
            {
                // handle apex controlls
                _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, Velocity.y);

                if (_apexPoint > MoveStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += timeStep;
                        if (_timePastApexThreshold < MoveStats.ApexHangTime)
                        {
                            Velocity.y = 0f;
                        }
                        else
                        {
                            Velocity.y = -0.01f;
                        }
                    }
                }
                // gravity on ascending but not past apex threshold
                else
                {
                    Velocity.y += MoveStats.Gravity * timeStep;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }

            // gravity on descending
            else if (!_isFastFalling)
            {
                Velocity.y += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * timeStep;
            }

            else if (Velocity.y < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }

        }

        // jump cut
        if (_isFastFalling)
        {
            if (_fastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                Velocity.y += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * timeStep;
            }
            else if (_fastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                Velocity.y = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / MoveStats.TimeForUpwardsCancel));
            }

            _fastFallTime += timeStep;
        }

        // normal gravity while falling
        if (!Controller.IsGrounded() && !_isJumping)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }

            Velocity.y += MoveStats.Gravity * timeStep;
        }
    }

    #endregion

    #region Timers

    private void CountTimers(float timeStep)
    {
        _jumpBufferTimer -= timeStep;

        if (!Controller.IsGrounded())
        {
            _coyoteTimer -= timeStep;
        }
        else { _coyoteTimer = MoveStats.JumpCoyoteTime; }
    }

    #endregion
}
