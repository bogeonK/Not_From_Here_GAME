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
        Debug.Log("[SpawnManager] Init() CALLED");

        if (config == null)
        {
            Debug.LogError("[SpawnManager] Config is null!");
            return;
        }

        Debug.Log($"[SpawnManager] enterGameLines len = {(config.enterGameLines == null ? -1 : config.enterGameLines.Length)}");
        Debug.Log($"[SpawnManager] controller = {(controller == null ? "NULL" : controller.name)}");

        if (!IsPlayerPresentInScene(out string why))
        {
            Debug.LogWarning($"[SpawnManager] Player not present -> return. reason={why}");
            return;
        }

        string spawnId = PlayerPrefs.GetString(config.spawnIdKey, config.defaultSpawnId);
        Debug.Log($"[SpawnManager] spawnId = {spawnId}");

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
            Debug.LogError("[SpawnManager] Player not found. (controller.playerTransform and tag search failed)");
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
            Debug.LogWarning($"[SpawnManager] SpawnPoint not found. id={spawnId} (fallback={config.defaultSpawnId})");

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
            Debug.LogWarning("[SpawnManager] No SpawnPoint matched. Player position unchanged.");
            return;
        }

        player.position = target.transform.position;
        Debug.Log($"[SpawnManager] Spawned at {target.id} pos={target.transform.position}");

        // 카메라 스냅
        var camFollow = Object.FindFirstObjectByType<Cainos.PixelArtTopDown_Basic.CameraFollow>(FindObjectsInactive.Include);
        if (camFollow != null)
        {
            camFollow.target = player;
            camFollow.RecalculateOffset();
            camFollow.SnapNow();
        }

        // UI가 늦게 생기는 경우까지 대비: 최대 120프레임 기다렸다가 오픈
        if (controller != null)
        {
            controller.StartCoroutine(WaitAndOpenEnterDialogue());
        }
        else
        {
            Debug.LogWarning("[SpawnManager] controller is NULL -> cannot run coroutine. Trying immediate.");
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
                Debug.Log($"[SpawnManager] Enter dialogue opened after {i} frames.");
                yield break;
            }
            yield return null;
        }

        Debug.LogWarning("[SpawnManager] DialogueUI not found or enter lines empty within wait time.");
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
            Debug.LogWarning("[SpawnManager] enterGameLines is empty -> skip");
            return false;
        }

        var ui = Object.FindFirstObjectByType<DialogueUI>(FindObjectsInactive.Include);
        if (ui == null)
        {
            // 너무 스팸 방지로 여기선 Warning 안 찍어도 됨. (원하면 찍어도 됨)
            return false;
        }

        Debug.Log($"[SpawnManager] DialogueUI found: {ui.name}. Opening enter lines...");
        ui.SetNpcName("용사"); 
        ui.OpenEnterLines(config.enterGameLines);
        return true;
    }

    public override void Update() { }
    public override void Destory() { }
}