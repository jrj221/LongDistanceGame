using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
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
        // ClampSpeed();
    }

    private void MovePlayer()
    {
        float moveInput = InputManager.Instance.InputMoveDirection;
        bool pressingLeft = moveInput < 0;
        bool pressingRight = moveInput > 0;
        // if (pressingLeft) rb.AddForce(Vector2.left * speed);
        // if (pressingRight) rb.AddForce(Vector2.right * speed);
        float yVelo =  rb.linearVelocity.y;
        rb.linearVelocity = speed * moveInput * Vector3.right + new Vector3(0, yVelo, 0);
    }

    private void ClampSpeed()
    {
        if (rb.linearVelocity.magnitude > speed) rb.linearVelocity = speed * rb.linearVelocity.normalized;
    }

    private void ApplyGravity()
    {
        Vector2 currVelocity = rb.linearVelocity;
        rb.linearVelocity = currVelocity + new Vector2(0, -9.8f * Time.deltaTime);
        
        if (_isGrounded && rb.linearVelocity.y < 0) rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
        // rb.MovePosition(transform.position + new Vector3(0, -0.1f, 0));
    }
}
