using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float accelerationPortion; // What portion of your velocityGap will you overcome this frame?
    [SerializeField] private Rigidbody2D rb;
    
    private bool _isGrounded;
    private readonly float RaycastDist = 0.01f;
    
    private void Update()
    {
        _isGrounded = Physics2D.Raycast(transform.position + Vector3.down * (0.5f + RaycastDist), Vector3.down, RaycastDist);
        Debug.DrawRay(transform.position + Vector3.down * (0.5f + RaycastDist), Vector3.down * RaycastDist, Color.red);
    }

    private void FixedUpdate()
    {
        MovePlayer();
        ApplyGravity();
    }

    private void MovePlayer()
    {
        // Difference between desired and current velocity
        float velocityGap = (InputManager.Instance.InputMoveDirection * speed) - rb.linearVelocity.x; 
        // When you are farther from your desired velocity, accelerate towards it quicker, allowing for smoother turns
        rb.linearVelocity += new Vector2(velocityGap * accelerationPortion, 0);
    }

    private void ApplyGravity()
    {
        // Custom gravity
        rb.linearVelocity += new Vector2(0, -9.8f * Time.deltaTime);
        
        // Kinematic rigidbodies don't collide, so we manually stop them just above the ground. 
        if (_isGrounded && rb.linearVelocity.y < 0) rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
    }
}
