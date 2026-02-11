public interface IInteractable
{
    string Prompt { get; }
    bool CanInteract();
    void Interact(UnityEngine.GameObject interactor);
}