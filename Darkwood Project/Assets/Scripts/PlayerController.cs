using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 5f;
    [SerializeField] float aimSpeed = 2f;
    [SerializeField] float dragRotationSpeed = 150f;
    [SerializeField] float dragSpeedModifier = 0.5f;

    [Header("Flashlight")]
    [SerializeField] Light2D flashlight;
    [SerializeField] float normalAngle = 70f;
    [SerializeField] float aimAngle = 30f;
    [SerializeField] float lightLerpSpeed = 8f;

    [Header("Gun")]
    [SerializeField] Light2D gunLight;
    [SerializeField] float flashFadeSpeed = 50f;
    [SerializeField] float gunFalloff = 0.75f;
    [SerializeField] float gunIntensity = 1;

    [Header("Interaction")]
    [SerializeField] LayerMask interactableMask;
    [Range(0.1f, 5f)]
    [SerializeField] float interactRange = 1f;
    [SerializeField] KeyCode interactKey = KeyCode.E;
    Vector2 dragOffset;
    Rigidbody2D draggedRb;
    float dragAngleOffset;

    float holdTimer = 0f;
    [SerializeField] float holdThreshold = 0.25f;
    IInteractable[] detectedInteractables;
    bool isCounting = false;

    float vertical;
    float horizontal;
    Vector2 movementDirection;
    Vector2 mousePosition;
    Rigidbody2D rb;
    Camera cam;

    Vector2 lookDir;
    bool isAiming = false;
    bool isDragging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }
    #region Update
    void Update()
    {
        HandleInput();

        HandleCombatLogic();

        HandleInteractable();

        UpdateVisuals();
    }

    void HandleInput()
    {
        isAiming = Input.GetButton("Fire2");

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
    void HandleCombatLogic()
    {
        if (Input.GetButtonDown("Fire1") && isAiming)
        {
            if (flashlight != null && Mathf.Abs(flashlight.pointLightOuterAngle - aimAngle) < 1f)
            {
                Shoot();
            }
        }
    }

    IInteractable activeDraggable;

    void HandleInteractable()
    {
        if (Input.GetKeyDown(interactKey) && activeDraggable != null)
        {
            if (draggedRb != null)
            {
                draggedRb.velocity = Vector2.zero;
                draggedRb.angularVelocity = 0f;
            }

            activeDraggable.OnRelease(this);
            activeDraggable = null;
            draggedRb = null;
            isCounting = false;
            return;
        }

        if (Input.GetKeyDown(interactKey) && activeDraggable == null)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, lookDir.normalized, interactRange, interactableMask);
            if (hit.collider != null)
            {
                detectedInteractables = hit.collider.GetComponents<IInteractable>();
                isCounting = true;
                holdTimer = 0f;
            }
        }

        if (isCounting && Input.GetKey(interactKey))
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdThreshold)
            {
                if (detectedInteractables != null)
                {
                    foreach (var inter in detectedInteractables)
                    {
                        inter.OnLongInteract(this);
                        if (inter is Draggable)
                        {
                            activeDraggable = inter;
                            draggedRb = ((MonoBehaviour)activeDraggable).GetComponent<Rigidbody2D>();

                            dragOffset = transform.InverseTransformPoint(draggedRb.position);
                            dragAngleOffset = Mathf.DeltaAngle(rb.rotation, draggedRb.rotation);
                        }
                    }
                }
                isCounting = false;
            }
        }

        if (Input.GetKeyUp(interactKey) && isCounting)
        {
            if (detectedInteractables != null)
            {
                foreach (var inter in detectedInteractables) inter.OnShortInteract();
            }
            isCounting = false;
            detectedInteractables = null;
        }
    }

    void Shoot()
    {
        if (gunLight != null)
        {
            gunLight.enabled = true;
            gunLight.intensity = gunIntensity;
            gunLight.pointLightOuterRadius = 15f;
            gunLight.falloffIntensity = gunFalloff;
        }
    }
    void UpdateVisuals()
    {
        if (flashlight != null)
        {
            float targetOuterAngle = isAiming ? aimAngle : normalAngle;
            float targetInnerAngle = Mathf.Max(0f, targetOuterAngle);

            flashlight.pointLightInnerAngle = Mathf.Lerp(flashlight.pointLightInnerAngle, targetInnerAngle, Time.deltaTime * lightLerpSpeed);
            flashlight.pointLightOuterAngle = Mathf.Lerp(flashlight.pointLightOuterAngle, targetOuterAngle, Time.deltaTime * lightLerpSpeed);
        }

        if (gunLight != null && gunLight.intensity > 0)
        {
            gunLight.intensity -= flashFadeSpeed * Time.deltaTime;
            if (gunLight.intensity < 0.01f)
            {
                gunLight.intensity = 0f;
                gunLight.enabled = false;
            }
        }
    }
    #endregion Update
    private void FixedUpdate()
    {
        lookDir = mousePosition - rb.position;
        float targetAngle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;

        if (isDragging)
        {
            float lerpedAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, dragRotationSpeed * Time.fixedDeltaTime);
            rb.rotation = lerpedAngle;
        }
        else
        {
            rb.rotation = targetAngle;
        }

        float currentSpeed = isAiming ? aimSpeed : speed;
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

                if (pullForce < 0)
                {
                    targetVelocity += moveDirNorm * pullForce;
                }
            }
            float targetObjAngle = rb.rotation + dragAngleOffset;
            float angleError = Mathf.DeltaAngle(draggedRb.rotation, targetObjAngle);
            draggedRb.angularVelocity = angleError * springStiffness;
        }
        rb.velocity = targetVelocity;
    }
    public void SetDragging(bool dragging)
    {
        isDragging = dragging;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(lookDir.normalized * interactRange));
    }
}