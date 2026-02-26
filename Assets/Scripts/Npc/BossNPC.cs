using System.Collections;
using UnityEngine;

public class BossNPC : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    [SerializeField] private string prompt = "도전하기";

    [Header("Intro Lines (고정 대사)")]
    [TextArea(2, 6)]
    [SerializeField]
    private string[] introLines =
    {
        "...결국 여기까지 왔군.",
        "이 몸이 바로 마왕이다."
    };

    [Header("Timing")]
    [SerializeField] private float lineDelay = 1.4f;

    private bool isBusy = false;

    public string Prompt => prompt;

    public bool CanInteract()
    {
        // 전투 중/연출 중 재상호작용 방지
        if (isBusy) return false;

        var battle = GameController.instance.GetManager<BattleManager>();
        if (battle != null && battle.State != BattleState.None) return false;

        return true;
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract()) return;
        StartCoroutine(CoBossSequence());
    }

    private IEnumerator CoBossSequence()
    {
        isBusy = true;

        var ui = GameController.instance.DialogueUI;

        if (ui == null)
        {
            isBusy = false;
            yield break;
        }
        if (introLines != null)
        {
            for (int i = 0; i < introLines.Length; i++)
            {
                var line = introLines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                ui.OpenOneShot(null, line);
                if (lineDelay > 0f)
                    yield return new WaitForSeconds(lineDelay);

                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
            }
        }

        ui.gameObject.SendMessage("Close", SendMessageOptions.DontRequireReceiver);

        GameController.instance.GetManager<BattleManager>().StartBattle();

        isBusy = false;
    }
}