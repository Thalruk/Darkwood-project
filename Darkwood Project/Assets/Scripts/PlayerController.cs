using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI; // Wymagane do obs³ugi Slidera

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 5f;
    [SerializeField] float aimSpeed = 2f;

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

    [Header("Interaction & Dragging")]
    [SerializeField] float interactionRange = 2f;
    [SerializeField] LayerMask draggableLayer;
    [SerializeField] float chargeRequired = 1.5f;
    [SerializeField] KeyCode interactKey = KeyCode.E;

    // --- Zmienne prywatne ---
    float vertical;
    float horizontal;
    Vector2 movementDirection;
    Vector2 mousePosition;
    Rigidbody2D rb;
    Camera cam;

    bool isAiming = false;
    bool isDragging = false;
    float currentCharge = 0f;
    GameObject currentTarget;
    Slider targetSlider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void Update()
    {
        // 1. Podstawowy Input
        HandleInput();

        // 2. Logika Celowania i Strza³u (Twoja oryginalna)
        HandleCombatLogic();

        // 3. System Interakcji (Nowy)
        if (isDragging)
        {
            HandleDraggingLogic();
        }
        else
        {
            HandleDetectionLogic();
        }

        // 4. Efekty wizualne (Latarka i B³ysk)
        UpdateVisuals();
    }

    void HandleInput()
    {
        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
        movementDirection = new Vector2(horizontal, vertical).normalized;
        isAiming = Input.GetButton("Fire2");

        Vector3 mousePos = Input.mousePosition;
        if (mousePos.x >= 0 && mousePos.x <= Screen.width && mousePos.y >= 0 && mousePos.y <= Screen.height)
        {
            Vector3 mouseScreen = mousePos;
            mouseScreen.z = 10f;
            mousePosition = cam.ScreenToWorldPoint(mouseScreen);
        }
    }

    void HandleDetectionLogic()
    {
        // Raycast puszczany w stronê, w któr¹ patrzy przód gracza (transform.up w 2D top-down)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, interactionRange, draggableLayer);

        if (hit.collider != null)
        {
            if (currentTarget != hit.collider.gameObject)
            {
                ResetOldTarget(); // Wy³¹cz UI poprzedniego, jeœli szybko zmieniliœmy cel
                currentTarget = hit.collider.gameObject;
                ToggleTargetUI(true);
            }

            // £adowanie interakcji
            if (Input.GetKey(interactKey))
            {
                currentCharge += Time.deltaTime;
                if (targetSlider) targetSlider.value = currentCharge / chargeRequired;

                if (currentCharge >= chargeRequired)
                {
                    StartDragging();
                }
            }
            else
            {
                currentCharge = 0f;
                if (targetSlider) targetSlider.value = 0f;
            }
        }
        else
        {
            ResetOldTarget();
        }
    }

    void StartDragging()
    {
        isDragging = true;
        currentCharge = 0f;

        // "Przyklejamy" obiekt do gracza
        currentTarget.transform.SetParent(this.transform);

        // Wy³¹czamy kolizje/fizykê obiektu, ¿eby nie blokowa³ gracza podczas ruchu
        Rigidbody2D targetRb = currentTarget.GetComponent<Rigidbody2D>();
        if (targetRb) targetRb.isKinematic = true;

        ToggleTargetUI(false); // Chowamy slider podczas ci¹gniêcia
    }

    void HandleDraggingLogic()
    {
        // Puœæ obiekt po naciœniêciu klawisza
        if (Input.GetKeyDown(interactKey))
        {
            isDragging = false;
            currentTarget.transform.SetParent(null);

            Rigidbody2D targetRb = currentTarget.GetComponent<Rigidbody2D>();
            if (targetRb) targetRb.isKinematic = false;

            currentTarget = null;
        }
    }

    void ResetOldTarget()
    {
        if (currentTarget != null && !isDragging)
        {
            ToggleTargetUI(false);
            currentTarget = null;
            currentCharge = 0f;
        }
    }

    void ToggleTargetUI(bool state)
    {
        if (currentTarget == null) return;

        // Szukamy Canvasu w dziecku obiektu
        Canvas canvas = currentTarget.GetComponentInChildren<Canvas>(true);
        if (canvas) canvas.gameObject.SetActive(state);

        if (state) targetSlider = currentTarget.GetComponentInChildren<Slider>();
    }

    // --- Reszta Twoich oryginalnych metod ---

    void HandleCombatLogic()
    {
        if (Input.GetButtonDown("Fire1") && isAiming)
        {
            if (flashlight != null && Mathf.Abs(flashlight.pointLightOuterAngle - aimAngle) < 1f)
            {
                Shoot();
            }
            else
            {
                Debug.Log("Jeszcze nie wycelowano!");
            }
        }
    }

    void UpdateVisuals()
    {
        if (flashlight != null)
        {
            float targetOuterAngle = isAiming ? aimAngle : normalAngle;
            float targetInnerAngle = Mathf.Max(0f, targetOuterAngle - 15f);

            flashlight.pointLightInnerAngle = Mathf.Lerp(flashlight.pointLightInnerAngle, targetInnerAngle, Time.deltaTime * lightLerpSpeed);
            flashlight.pointLightOuterAngle = Mathf.Lerp(flashlight.pointLightOuterAngle, targetOuterAngle, Time.deltaTime * lightLerpSpeed);
        }

        if (gunLight != null && gunLight.intensity > 0)
        {
            gunLight.intensity -= flashFadeSpeed * Time.deltaTime;
            if (gunLight.intensity < 0.01f) gunLight.intensity = 0f;
        }
    }

    private void FixedUpdate()
    {
        float currentSpeed = isAiming ? aimSpeed : speed;
        rb.MovePosition(rb.position + movementDirection * currentSpeed * Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        LookAtMouse();
    }

    void LookAtMouse()
    {
        Vector2 lookDir = mousePosition - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        rb.rotation = angle - 90f;
    }

    void Shoot()
    {
        if (gunLight != null)
        {
            gunLight.intensity = gunIntensity;
            gunLight.pointLightOuterRadius = 15f;
            gunLight.falloffIntensity = gunFalloff;
            gunLight.color = Color.white;
        }
    }
}