using UnityEngine;

public enum NpcIdentityType
{
    World1,
    OtherWorld
}

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private NpcProfileSO profile;
    [SerializeField] private string prompt = "대화하기";

    [Header("대화 완료 후 한 줄 멘트")]
    [TextArea(2, 6)]
    [SerializeField] private string afterFinishedLine = "해야할 일도 많을텐데 여기서 이래도 되?";

    [SerializeField] private bool dialogueFinished = false;

    private NpcIdentityType runtimeIdentity;
    [SerializeField] private NpcIdentityType debugRuntimeIdentity;

    public string Prompt => prompt;
    public bool CanInteract() => true;
    public NpcProfileSO Profile => profile;
    public NpcIdentityType RuntimeIdentity => runtimeIdentity;

    private void Awake()
    {
        AssignRandomIdentity();
    }

    private void AssignRandomIdentity()
    {
        runtimeIdentity = (Random.value < 0.5f)
            ? NpcIdentityType.World1
            : NpcIdentityType.OtherWorld;

        debugRuntimeIdentity = runtimeIdentity;

        Debug.Log($"{profile.DisplayName} 정체: {runtimeIdentity}");
    }

    public void Interact(GameObject interactor)
    {
        var ui = GameController.instance.DialogueUI;

        //대화 완료된 NPC: 종료
        if (dialogueFinished)
        {
            ui.OpenOneShot(null, afterFinishedLine);

            return;
        }

        ui.OpenChat(this);
    }

    public void MarkDialogueFinished()
    {
        dialogueFinished = true;

        if (profile != null)
        {
            GameController.instance
                .GetManager<InvestigationProgressManager>()
                .MarkTalked(profile.npcId);
        }
    }


    public void ResolveChoice(NpcChoice choice)
    {
        var ending = GameController.instance.GetManager<EndingSystemManager>();

        bool isOtherWorlder = (runtimeIdentity == NpcIdentityType.OtherWorld);

        if (isOtherWorlder)
        {
            if (choice == NpcChoice.Kill)
            {
                Debug.Log($"[{profile.displayName}] 이세계인 처치 성공");
                gameObject.SetActive(false);
                return;
            }
            else
            {
                Debug.Log($"[{profile.displayName}] 이세계인 놓침");
                var trigger = new BadEndingTrigger(
                    profile.badEndingId,
                    profile.badEndingPhase,
                    profile.priority,
                    profile.npcId
                );
                ending.ArmTrigger(trigger);
                return;
            }
        }
        else // 1세계인
        {
            if (choice == NpcChoice.Kill)
            {
                Debug.LogWarning($"[{profile.displayName}] 1세계인을 죽임");
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"[{profile.displayName}] 1세계인 살려둠");
            }
        }
    }
}
