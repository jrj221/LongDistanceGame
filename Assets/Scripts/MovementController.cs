using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public const float CollisionPadding = 0.015f;

    [Range(2, 100)] public int NumHorizontalRays = 4;
    [Range(2, 100)] public int NumVerticalRays = 4;

    private float _horizontalRaySpace;
    private float _verticalRaySpace;

    private BoxCollider2D _collider;

    private PlayerMovementStats _moveStats;

    public bool IsCollidingAbove { get; private set; }
    public bool IsCollidingBelow { get; private set; }
    public bool IsCollidingLeft { get; private set; }
    public bool IsCollidingRight { get; private set; }

    private PlayerMovement _playerMovement;
    public RaycastCorners RayCastCorners;
    private Rigidbody2D _rb;

    public struct RaycastCorners
    {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
    }

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _rb = GetComponent<Rigidbody2D>();
        _playerMovement = GetComponent<PlayerMovement>();
        _moveStats = _playerMovement.MoveStats;
    }

    private void Start()
    {
        CalculateRaySpacing();
    }

    public void Move(Vector2 velocity)
    {
        UpdateRaycastCorners();
        ResetCollisionStates();

        ResolveHorizontalMovement(ref velocity);
        ResolveVerticalMovement(ref velocity);

        _rb.MovePosition(_rb.position + velocity);
    }

    private void ResetCollisionStates()
    {
        IsCollidingAbove = false;
        IsCollidingBelow = false;
        IsCollidingLeft = false;
        IsCollidingRight = false;
    }

    private void ResolveHorizontalMovement(ref Vector2 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + CollisionPadding;

        for (int i = 0; i < NumHorizontalRays; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? RayCastCorners.bottomLeft : RayCastCorners.bottomRight;
            rayOrigin += Vector2.up * (_horizontalRaySpace * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, _moveStats.GroundLayer);

            if (hit)
            {
                velocity.x = (hit.distance - CollisionPadding) * directionX;
                rayLength = hit.distance;

                if (directionX == -1)
                {
                    IsCollidingLeft = true;
                }

                else if (directionX == 1)
                {
                    IsCollidingRight = true;
                }
            }

            #region Debug Visualization

            if (_moveStats.DebugShowWallHit)
            {
                Color rayColor = hit ? Color.cyan : Color.red;
                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, rayColor);
            }

            #endregion
        }
    }

    private void ResolveVerticalMovement(ref Vector2 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + CollisionPadding;

        for (int i = 0; i < NumVerticalRays; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? RayCastCorners.bottomLeft : RayCastCorners.topLeft;
            rayOrigin += Vector2.right * (_verticalRaySpace * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, _moveStats.GroundLayer);

            if (hit)
            {
                velocity.y = (hit.distance - CollisionPadding) * directionY;
                rayLength = hit.distance;

                if (directionY == -1)
                {
                    IsCollidingBelow = true;
                }

                else if (directionY == 1)
                {
                    IsCollidingAbove = true;
                }
            }

            #region Debug Visualization

            if (_moveStats.DebugShowIsGrounded)
            {
                Color rayColor = hit ? Color.cyan : Color.red;
                Debug.DrawRay(rayOrigin, Vector2.down * directionY * rayLength, rayColor);
            }

            if (_moveStats.DebugShowHeadRays)
            {
                Color rayColor = hit ? Color.cyan : Color.red;
                if (i == 0 || i == NumVerticalRays - 1)
                {
                    rayColor = hit ? Color.green : Color.magenta;
                }
                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, rayColor);
            }

            #endregion
        }
    }

    private void UpdateRaycastCorners()
    {
        Bounds bounds = _collider.bounds;
        bounds.Expand(CollisionPadding * -2);

        RayCastCorners.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        RayCastCorners.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        RayCastCorners.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        RayCastCorners.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    private void CalculateRaySpacing()
    {
        Bounds bounds = _collider.bounds;
        bounds.Expand(CollisionPadding * -2);

        _horizontalRaySpace = bounds.size.y / (NumHorizontalRays - 1);
        _verticalRaySpace = bounds.size.x / (NumVerticalRays - 1);
    }

    public bool IsGrounded() => IsCollidingBelow;
    public bool BumpedHead() => IsCollidingAbove;
    public bool IsTouchingWall(bool isFacingRight) => (isFacingRight && IsCollidingRight) || (!isFacingRight && IsCollidingLeft);
    public int GetWallDirection()
    {
        if (IsCollidingLeft) return -1;
        if (IsCollidingRight) return 1;
        return 0;
    }
}
