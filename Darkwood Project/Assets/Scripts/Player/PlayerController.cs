using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private PlayerMovement movement;
    private PlayerCombat combat;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
    }


    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 moveInput = new Vector2(horizontal, vertical).normalized;

        Vector3 mouseScreenPos = Input.mousePosition;

        bool isAiming = Input.GetButton("Fire2");
        bool isShooting = Input.GetButtonDown("Fire1");
        bool isReloading = Input.GetKeyDown(KeyCode.R);

        movement.UpdateMovementData(moveInput, mouseScreenPos, isAiming);
        combat.HandleCombatLogic(isAiming, isShooting, isReloading);
        combat.UpdateVisuals(isAiming);
    }
}