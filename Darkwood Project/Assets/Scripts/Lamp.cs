using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Lamp : MonoBehaviour, IInteractable
{
    Light2D lampLight;
    private void Awake()
    {
        lampLight = GetComponentInChildren<Light2D>();
    }
    public void OnShortInteract()
    {
        lampLight.enabled = !lampLight.enabled;
    }
    public void OnLongInteract(PlayerController player)
    {
        print("OnLongInteract " + name);
    }

    public void OnRelease(PlayerController player)
    {
        print("Released");
    }
}
