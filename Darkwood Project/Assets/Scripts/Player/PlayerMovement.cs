using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float speed = 5f;
    [SerializeField] float aimSpeed = 2f;
    public Vector2 LookDir { get; private set; }
    [SerializeField] float backwardsSpeedModifier = 0.5f;

    private Rigidbody2D rb;
    private Camera cam;
    private Vector2 currentMoveInput;
    private Vector2 currentMousePos;
    private bool currentIsAiming;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    public void UpdateMovementData(Vector2 moveInput, Vector3 mouseScreenPos, bool isAiming)
    {
        currentMoveInput = moveInput;
        currentIsAiming = isAiming;

        if (mouseScreenPos.x >= 0 && mouseScreenPos.x <= Screen.width &&
            mouseScreenPos.y >= 0 && mouseScreenPos.y <= Screen.height)
        {
            mouseScreenPos.z = 10f;
            if (cam != null)
            {
                currentMousePos = cam.ScreenToWorldPoint(mouseScreenPos);
            }
        }
    }

    private void FixedUpdate()
    {
        LookDir = currentMousePos - rb.position;
        Vector2 lookDirNormalized = LookDir.normalized;

        float targetAngle = Mathf.Atan2(LookDir.y, LookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = targetAngle;
        rb.angularVelocity = 0f;

        float currentSpeed = currentIsAiming ? aimSpeed : speed;

        if (currentMoveInput.magnitude > 0)
        {
            float dotProduct = Vector2.Dot(currentMoveInput, lookDirNormalized);

            if (dotProduct < 0)
            {
                float speedMultiplier = Mathf.Lerp(1f, backwardsSpeedModifier, Mathf.Abs(dotProduct));
                currentSpeed *= speedMultiplier;
            }
        }

        Vector2 targetVelocity = currentMoveInput * currentSpeed;
        rb.velocity = targetVelocity;
    }
}