using UnityEngine;

public class FightPortalTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        //npc대화완료 체크
        var progress = GameController.instance.GetManager<InvestigationProgressManager>();
        if (!progress.IsAllTalked())
        {
            GameController.instance.DialogueUI.OpenOneShot(null, progress.GetNotReadyLine());
            return;
        }

        //이장 엔딩 체크
        var ending = GameController.instance.GetManager<EndingSystemManager>();
        if (ending.TryTriggerPhase(StoryPhase.LeaveVillage))
            return;

        Debug.Log("전투맵 이동");
        // MapManager.Instance.MoveToBattle();
    }
}