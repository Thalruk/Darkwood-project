using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private PlayerMovement movement;
    private PlayerCombat combat;

    [Header("Interaction Settings")]
    [SerializeField] LayerMask interactableMask;
    [Range(0.1f, 5f)]
    [SerializeField] float interactRange = 1f;
    [SerializeField] KeyCode interactKey = KeyCode.E;
    [SerializeField] float holdThreshold = 0.25f;

    private GameObject hoveredObject;
    private float holdTimer = 0f;
    private IInteractable[] detectedInteractables;
    private bool isCounting = false;
    private IInteractable activeDraggable;

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

        CheckHover();
        HandleInteractable();

        CheckPanicDrop();

        combat.UpdateVisuals(movement.IsAiming);
    }
    void CheckHover()
    {
        if (movement.IsDraggingObject())
        {
            hoveredObject = null;
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, movement.LookDir.normalized, interactRange, interactableMask);

        if (hit.collider != null) hoveredObject = hit.collider.gameObject;
        else hoveredObject = null;
    }

    void HandleInteractable()
    {
        if (Input.GetKeyDown(interactKey) && activeDraggable != null)
        {
            movement.StopDragging();
            activeDraggable.OnRelease(this);
            activeDraggable = null;
            isCounting = false;
            return;
        }

        if (Input.GetKeyDown(interactKey) && activeDraggable == null)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, movement.LookDir.normalized, interactRange, interactableMask);
            if (hit.collider != null)
            {
                detectedInteractables = hit.collider.GetComponents<IInteractable>();
                isCounting = true;
                holdTimer = 0f;
            }
        }

        if (isCounting && Input.GetKey(interactKey))
        {
            bool isLookingAway = true;
            if (hoveredObject != null && detectedInteractables != null && detectedInteractables.Length > 0)
            {
                if (hoveredObject == ((MonoBehaviour)detectedInteractables[0]).gameObject)
                {
                    isLookingAway = false;
                }
            }

            if (isLookingAway)
            {
                isCounting = false;
                detectedInteractables = null;
                holdTimer = 0f;
                return;
            }

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
                            Rigidbody2D itemRb = ((MonoBehaviour)activeDraggable).GetComponent<Rigidbody2D>();
                            movement.StartDragging(itemRb);
                        }
                    }
                }
                isCounting = false;
            }
        }

        if (isCounting && Input.GetKeyUp(interactKey))
        {
            if (holdTimer < holdThreshold && detectedInteractables != null)
            {
                foreach (var inter in detectedInteractables)
                {
                    inter.OnShortInteract();
                }
            }

            isCounting = false;
            detectedInteractables = null;
            holdTimer = 0f;
        }
    }

    private void CheckPanicDrop()
    {
        if (!IsDraggingObject() || activeDraggable == null) return;

        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            if (Vector2.Distance(transform.position, enemy.transform.position) < 1.5f)
            {
                Debug.Log("Wróg za blisko! Panika! Upuszczam barykadê!");

                movement.StopDragging();
                activeDraggable.OnRelease(this);
                activeDraggable = null;
                isCounting = false;
                break;
            }
        }
    }

    public void SetDragging(bool dragging)
    {
        if (!dragging) movement.StopDragging();
    }

    public float GetHoldProgress()
    {
        if (!isCounting || holdThreshold <= 0) return 0f;
        return Mathf.Clamp01(holdTimer / holdThreshold);
    }

    public string GetLookingAtObjectName()
    {
        if (hoveredObject != null) return hoveredObject.name;
        return null;
    }
    public GameObject GetHoveredObject()
    {
        return hoveredObject;
    }

    public IInteractable GetActiveDraggable()
    {
        return activeDraggable;
    }
    public bool IsDraggingObject() => movement.IsDraggingObject();

    private void OnDrawGizmos()
    {
        if (movement != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(movement.LookDir.normalized * interactRange));
        }
    }
}