using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] 
    private float speed;
    
    [SerializeField] 
    [Tooltip("What portion of your velocityGap will you overcome this FixedUpdate? Should be between 0 and 1")]
    private float accelerationPortion;

    [SerializeField] 
    private float _raycastDist;
    
    [SerializeField] 
    [Tooltip("Rate at which player's downward velocity changes every FixedUpdate. Enter a positive float")]
    private float gravityAcceleration;
    #endregion
    
    
    #region Class Fields
    private Rigidbody2D _rb;
    private bool _isGrounded;
    private float _playerHeight;
    #endregion

    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerHeight = GetComponent<SpriteRenderer>().bounds.size.y;
    }
    
    private void Update()
    {
        // Puts the feet slightly (raycastDist) under the player since 2D raycasts will collide with a collider at their origin
        var feetPosition = new Vector2 (transform.position.x, transform.position.y - _playerHeight/2 - _raycastDist);
        _isGrounded = Physics2D.Raycast(feetPosition, Vector3.down, _raycastDist);
        Debug.DrawRay(feetPosition, Vector3.down * _raycastDist, Color.red);
    }

    private void FixedUpdate()
    {
        MovePlayer();
        ApplyGravity();
    }

    private void MovePlayer()
    {
        // Difference between desired and current velocity
        float velocityGap = (InputManager.Instance.InputMoveDirection * speed) - _rb.linearVelocity.x; 
        // When you are farther from your desired velocity, accelerate towards it quicker, allowing for smoother turns
        _rb.linearVelocity += new Vector2(velocityGap * accelerationPortion, 0);
    }

    private void ApplyGravity()
    {
        // Custom gravity
        _rb.linearVelocity += new Vector2(0, -gravityAcceleration * Time.deltaTime);
        
        // Kinematic rigidbodies don't collide, so we manually stop them just above the ground. 
        if (_isGrounded && _rb.linearVelocity.y < 0) _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0); 
    }
}
