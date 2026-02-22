using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float vertical;
    [SerializeField] float horizontal;
    [SerializeField] float speed;

    Vector2 movementDirection;
    Vector2 mousePosition;
    Rigidbody2D rb;
    Camera cam;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void Update()
    {
        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
        movementDirection = new Vector2(horizontal, vertical).normalized;

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = 0;
        mousePosition = cam.ScreenToWorldPoint(mouseScreen);
    }

    private void FixedUpdate()
    {
        rb.velocity = movementDirection * speed;

        LookAtMouse();
    }

    void LookAtMouse()
    {
        Vector2 lookDir = mousePosition - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        rb.rotation = angle - 90f;
    }
}