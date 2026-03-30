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
        player.SetDragging(true);
    }

    public void OnRelease(PlayerController player)
    {
        player.SetDragging(false);
    }
}