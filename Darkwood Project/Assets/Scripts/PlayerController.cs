using UnityEngine;
using UnityEngine.Rendering.Universal;

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

    float vertical;
    float horizontal;
    Vector2 movementDirection;
    Vector2 mousePosition;
    Rigidbody2D rb;
    Camera cam;

    bool isAiming = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void Update()
    {
        // 1. Oœ ruchu
        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
        movementDirection = new Vector2(horizontal, vertical).normalized;

        // 2. Nas³uchiwanie prawego przycisku myszy (celowanie)
        isAiming = Input.GetButton("Fire2");

        // 3. Nas³uchiwanie strza³u (lewy przycisk) - dzia³a tylko przy celowaniu
        // 3. Nas³uchiwanie strza³u (lewy przycisk)
        if (Input.GetButtonDown("Fire1") && isAiming)
        {
            // Zabezpieczenie: strzelamy tylko, jeœli latarka istnieje i jej k¹t prawie osi¹gn¹³ minimum
            if (flashlight != null && Mathf.Abs(flashlight.pointLightOuterAngle - aimAngle) < 1f)
            {
                Shoot();
            }
            else
            {
                // Opcjonalnie: dŸwiêk pustego klikniêcia, jeœli gracz nacisn¹³ za wczeœnie
                Debug.Log("Jeszcze nie wycelowano!");
            }
        }

        // 4. Kalkulacja pozycji myszy
        Vector3 mousePos = Input.mousePosition;
        if (mousePos.x >= 0 && mousePos.x <= Screen.width && mousePos.y >= 0 && mousePos.y <= Screen.height)
        {
            Vector3 mouseScreen = mousePos;
            mouseScreen.z = 10f;
            mousePosition = cam.ScreenToWorldPoint(mouseScreen);
        }

        // 5. P³ynne zwê¿anie i rozszerzanie latarki
        if (flashlight != null)
        {
            float targetOuterAngle = isAiming ? aimAngle : normalAngle;

            // Wewnêtrzny k¹t niech bêdzie zawsze o 15 stopni mniejszy od zewnêtrznego (i nie mniejszy ni¿ 0)
            float targetInnerAngle = Mathf.Max(0f, targetOuterAngle - 15f);

            // Zmieniamy oba k¹ty p³ynnie
            flashlight.pointLightInnerAngle = Mathf.Lerp(flashlight.pointLightInnerAngle, targetInnerAngle, Time.deltaTime * lightLerpSpeed);
            flashlight.pointLightOuterAngle = Mathf.Lerp(flashlight.pointLightOuterAngle, targetOuterAngle, Time.deltaTime * lightLerpSpeed);
        }
    }

    private void FixedUpdate()
    {
        // Ruch z uwzglêdnieniem kary do prêdkoœci przy celowaniu
        float currentSpeed = isAiming ? aimSpeed : speed;
        rb.velocity = movementDirection * currentSpeed;

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
        // Miejsce na instancjonowanie kuli i odrzut
        Debug.Log("Pif paf! Strza³ oddany.");
    }
}