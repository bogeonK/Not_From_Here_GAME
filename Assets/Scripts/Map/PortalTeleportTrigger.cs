using Cainos.PixelArtTopDown_Basic;
using UnityEngine;

public class PortalTeleportTrigger : MonoBehaviour
{
    [Header("Teleport")]
    [SerializeField] private Transform destination;
    [SerializeField] private float fadeDuration = 0.35f;

    [Header("Camera Follow")]
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Checks (optional)")]
    [SerializeField] private bool requireAllNpcTalked = true;
    [SerializeField] private bool checkEndingOnLeaveVillage = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        //npc 대화 완료 체크
        if (requireAllNpcTalked)
        {
            var progress = GameController.instance.GetManager<InvestigationProgressManager>();
            if (!progress.IsAllTalked())
            {
                GameController.instance.DialogueUI.OpenOneShot(null, progress.GetNotReadyLine());
                return;
            }
        }

        //이장 엔딩 체크
        if (checkEndingOnLeaveVillage)
        {
            var ending = GameController.instance.GetManager<EndingSystemManager>();
            if (ending.TryTriggerPhase(StoryPhase.LeaveVillage))
                return;
        }

        if (destination == null)
        {
            return;
        }

        if (cameraFollow == null)
            cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;


        //이동
        var playerTf = other.transform;

        ScreenFader.Instance.FadeOutIn(() =>
        {
            playerTf.position = destination.position;

            var rb = other.attachedRigidbody;
            if (rb != null) rb.linearVelocity = Vector2.zero;

            cameraFollow?.SnapNow();

            GameController.instance.GetManager<SoundManager>().SetAreaBgm(BgmId.Dungeon, 0.1f);

        }, fadeDuration);

    }
}