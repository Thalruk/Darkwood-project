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
        movement.HandleInput();
        combat.HandleCombatLogic(movement.IsAiming);
        combat.UpdateVisuals(movement.IsAiming);
    }
}