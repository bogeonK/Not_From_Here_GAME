using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    private IInteractable current;

    private void Update()
    {
        var ui = GameController.instance?.DialogueUI;
        if (ui != null)
        {
            if (ui.IsOpen) return;
            if (ui.LastClosedFrame == Time.frameCount) return; 
        }

        //전투 중이면 상호작용 막음
        var bm = GameController.instance?.GetManager<BattleManager>();
        if (bm != null && bm.State != BattleState.None) return;

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
