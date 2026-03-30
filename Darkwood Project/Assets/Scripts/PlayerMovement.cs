using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float speed = 5f;
    [SerializeField] float aimSpeed = 2f;
    [SerializeField] float dragSpeedModifier = 0.5f;

    public bool IsAiming { get; private set; }
    public Vector2 LookDir { get; private set; }

    private Rigidbody2D rb;
    private Camera cam;
    private float vertical;
    private float horizontal;
    private Vector2 movementDirection;
    private Vector2 mousePosition;

    private bool isDragging = false;
    private Rigidbody2D draggedRb;
    private FixedJoint2D grabJoint;

    private float lockedAngle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    public void HandleInput()
    {
        IsAiming = Input.GetButton("Fire2");

        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
        movementDirection = new Vector2(horizontal, vertical).normalized;

        Vector3 mousePos = Input.mousePosition;
        if (mousePos.x >= 0 && mousePos.x <= Screen.width && mousePos.y >= 0 && mousePos.y <= Screen.height)
        {
            Vector3 mouseScreen = mousePos;
            mouseScreen.z = 10f;
            if (cam != null)
            {
                mousePosition = cam.ScreenToWorldPoint(mouseScreen);
            }
        }
    }

    private void FixedUpdate()
    {
        LookDir = mousePosition - rb.position;
        float targetAngle = Mathf.Atan2(LookDir.y, LookDir.x) * Mathf.Rad2Deg - 90f;

        if (!isDragging)
        {
            rb.rotation = targetAngle;
        }
        else
        {
            rb.rotation = lockedAngle;
        }

        rb.angularVelocity = 0f;

        float currentSpeed = IsAiming ? aimSpeed : speed;
        if (isDragging) currentSpeed *= dragSpeedModifier;

        Vector2 targetVelocity = movementDirection * currentSpeed;
        rb.velocity = targetVelocity;
    }

    public void StartDragging(Rigidbody2D itemRb)
    {
        isDragging = true;
        draggedRb = itemRb;

        lockedAngle = rb.rotation;

        grabJoint = gameObject.AddComponent<FixedJoint2D>();
        grabJoint.connectedBody = draggedRb;
    }

    public void StopDragging()
    {
        isDragging = false;

        if (draggedRb != null)
        {
            draggedRb.velocity = Vector2.zero;
            draggedRb.angularVelocity = 0f;
            draggedRb = null;
        }

        if (grabJoint != null)
        {
            Destroy(grabJoint);
        }
    }

    public bool IsDraggingObject() => isDragging;
}