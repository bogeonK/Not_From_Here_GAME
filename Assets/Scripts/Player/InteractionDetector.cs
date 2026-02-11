using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    private IInteractable current;

    private void Update()
    {
        if (GameController.instance.DialogueUI.IsOpen) return;

        var ui = GameController.instance.DialogueUI;

        if (ui != null && ui.IsOpen)
            return;
        if (ui != null && ui.LastClosedFrame == Time.frameCount)
            return;

        if (!Input.GetKeyDown(interactKey)) return;
        if (current == null) return;
        if (!current.CanInteract()) return;

        current.Interact(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable == null) return;

        current = interactable;
        Debug.Log($"[Interact] Enter -> {current.Prompt}");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable == null) return;

        if (current == interactable)
        {
            current = null;
            Debug.Log("[Interact] Exit");
        }
    }
}
