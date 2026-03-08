public interface IInteractable
{
    void OnShortInteract();
    void OnLongInteract(PlayerController player);
    void OnRelease(PlayerController player);
}