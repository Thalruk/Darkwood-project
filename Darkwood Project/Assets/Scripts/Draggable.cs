using UnityEngine;

public class Draggable : MonoBehaviour, IInteractable
{
    private Rigidbody2D rb;
    private Collider2D myCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
    }

    public void OnShortInteract()
    {
        Debug.Log("To jest zbyt ciê¿kie, by tak po prostu tego u¿yæ.");
    }

    public void OnLongInteract(PlayerController player)
    {
        // Zero wy³¹czania kolizji. Tylko czysty sygna³ do kontrolera.
        player.SetDragging(true);
    }

    public void OnRelease(PlayerController player)
    {
        // Czysty sygna³ o puszczeniu mebla.
        player.SetDragging(false);
    }
}