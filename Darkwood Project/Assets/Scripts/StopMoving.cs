using UnityEngine;

public class StopMoving : MonoBehaviour
{
    Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        if (rb.velocity.magnitude < 0.1f)
        {
            rb.velocity = Vector2.zero;
        }
    }
}
