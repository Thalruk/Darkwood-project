using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float speed = 5f;
    [SerializeField] float aimSpeed = 2f;
    [SerializeField] float dragRotationSpeed = 35f;
    [SerializeField] float dragSpeedModifier = 0.5f;

    public bool IsAiming { get; private set; }
    public Vector2 LookDir { get; private set; }

    private Rigidbody2D rb;
    private Camera cam;
    private float vertical;
    private float horizontal;
    private Vector2 movementDirection;
    private Vector2 mousePosition;

    // --- Stan Dragowania ---
    private bool isDragging = false;
    private Rigidbody2D draggedRb;
    private Vector2 dragOffset;
    private float dragAngleOffset;

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
            mousePosition = cam.ScreenToWorldPoint(mouseScreen);
        }
    }

    private void FixedUpdate()
    {
        LookDir = mousePosition - rb.position;
        float targetAngle = Mathf.Atan2(LookDir.y, LookDir.x) * Mathf.Rad2Deg - 90f;

        if (isDragging)
        {
            float lerpedAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, dragRotationSpeed * Time.fixedDeltaTime);
            rb.rotation = lerpedAngle;
        }
        else
        {
            rb.rotation = targetAngle;
        }

        float currentSpeed = IsAiming ? aimSpeed : speed;
        if (isDragging) currentSpeed *= dragSpeedModifier;

        Vector2 targetVelocity = movementDirection * currentSpeed;
        float springStiffness = 15f;

        if (isDragging && draggedRb != null)
        {
            Vector2 targetItemPos = transform.TransformPoint(dragOffset);
            Vector2 errorVector = targetItemPos - draggedRb.position;
            draggedRb.velocity = errorVector * springStiffness;

            float expectedDist = dragOffset.magnitude;
            float currentDist = Vector2.Distance(rb.position, draggedRb.position);
            float distError = currentDist - expectedDist;

            if (Mathf.Abs(distError) > 0.05f && movementDirection.sqrMagnitude > 0.01f)
            {
                Vector2 dirToItem = (draggedRb.position - rb.position).normalized;
                Vector2 playerCorrection = dirToItem * distError * springStiffness;
                Vector2 moveDirNorm = movementDirection.normalized;

                float pullForce = Vector2.Dot(playerCorrection, moveDirNorm);
                if (pullForce < 0) targetVelocity += moveDirNorm * pullForce;
            }

            float targetObjAngle = rb.rotation + dragAngleOffset;
            float angleError = Mathf.DeltaAngle(draggedRb.rotation, targetObjAngle);
            draggedRb.angularVelocity = angleError * springStiffness;
        }

        rb.velocity = targetVelocity;
    }

    public void StartDragging(Rigidbody2D itemRb)
    {
        isDragging = true;
        draggedRb = itemRb;
        dragOffset = transform.InverseTransformPoint(draggedRb.position);
        dragAngleOffset = Mathf.DeltaAngle(rb.rotation, draggedRb.rotation);
    }

    public void StopDragging()
    {
        if (draggedRb != null)
        {
            draggedRb.velocity = Vector2.zero;
            draggedRb.angularVelocity = 0f;
        }
        isDragging = false;
        draggedRb = null;
    }

    public bool IsDraggingObject() => isDragging;
}