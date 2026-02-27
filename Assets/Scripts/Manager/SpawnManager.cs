using System.Collections;
using UnityEngine;

public class SpawnManager : baseManager
{
    private readonly SpawnManagerConfigSO config;

    public SpawnManager(SpawnManagerConfigSO config)
    {
        this.config = config;
    }

    public override void Init()
    {

        if (config == null)
        {
            return;
        }


        if (!IsPlayerPresentInScene(out string why))
        {
            return;
        }

        string spawnId = PlayerPrefs.GetString(config.spawnIdKey, config.defaultSpawnId);

        Transform player = null;

        if (config.useControllerPlayerTransformFirst && controller != null && controller.playerTransform != null)
            player = controller.playerTransform;

        if (player == null)
        {
            var pObj = GameObject.FindGameObjectWithTag(config.playerTag);
            if (pObj != null) player = pObj.transform;
        }

        if (player == null)
        {
            return;
        }

        var points = Object.FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        SpawnPoint target = null;

        foreach (var sp in points)
        {
            if (sp != null && sp.id == spawnId)
            {
                target = sp;
                break;
            }
        }

        if (target == null)
        {

            if (spawnId != config.defaultSpawnId)
            {
                foreach (var sp in points)
                {
                    if (sp != null && sp.id == config.defaultSpawnId)
                    {
                        target = sp;
                        break;
                    }
                }
            }
        }

        if (target == null)
        {
            return;
        }

        player.position = target.transform.position;

        // 카메라 스냅
        var camFollow = Object.FindFirstObjectByType<Cainos.PixelArtTopDown_Basic.CameraFollow>(FindObjectsInactive.Include);
        if (camFollow != null)
        {
            camFollow.target = player;
            camFollow.RecalculateOffset();
            camFollow.SnapNow();
        }

        if (controller != null)
        {
            controller.StartCoroutine(WaitAndOpenEnterDialogue());
        }
        else
        {
            TryPlayEnterDialogueOnce();
        }

        if (config.clearKeyAfterUse)
            PlayerPrefs.DeleteKey(config.spawnIdKey);
    }

    private IEnumerator WaitAndOpenEnterDialogue()
    {
        const int maxFrames = 120;

        for (int i = 0; i < maxFrames; i++)
        {
            if (TryPlayEnterDialogueOnce())
            {
                yield break;
            }
            yield return null;
        }

    }

    private bool IsPlayerPresentInScene(out string reason)
    {
        if (controller != null && controller.playerTransform != null)
        {
            reason = "controller.playerTransform";
            return true;
        }

        var pObj = GameObject.FindGameObjectWithTag(config.playerTag);
        if (pObj != null)
        {
            reason = "tag search";
            return true;
        }

        reason = $"no playerTransform & no tag({config.playerTag})";
        return false;
    }

    private bool TryPlayEnterDialogueOnce()
    {
        if (config == null) return false;
        if (config.enterGameLines == null || config.enterGameLines.Length == 0)
        {
            return false;
        }

        var ui = Object.FindFirstObjectByType<DialogueUI>(FindObjectsInactive.Include);
        if (ui == null)
        {

            return false;
        }

        ui.SetNpcName("용사"); 
        ui.OpenEnterLines(config.enterGameLines);
        return true;
    }

    public override void Update() { }
    public override void Destory() { }
}