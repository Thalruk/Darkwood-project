using UnityEngine;

public class Draggable : MonoBehaviour, IInteractable
{
    private FixedJoint2D joint;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnShortInteract()
    {
        Debug.Log("To jest zbyt ciê¿kie, by tak po prostu tego u¿yæ.");
    }

    public void OnLongInteract(PlayerController player)
    {
        if (joint == null)
        {
            joint = gameObject.AddComponent<FixedJoint2D>();
            joint.connectedBody = player.GetComponent<Rigidbody2D>();

            joint.dampingRatio = 1f;
            joint.frequency = 0f;

            player.SetDragging(true);
        }
    }

    public void OnRelease(PlayerController player)
    {
        if (joint != null)
        {
            Destroy(joint);
            player.SetDragging(false);
        }

    }
}