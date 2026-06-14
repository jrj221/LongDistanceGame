using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Rigidbody2D rb;
    
    private void Update()
    {
        MovePlayer();
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
}
