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
            Debug.LogError("[SpawnManager] Config is null!");
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
            Debug.LogWarning($"[SpawnManager] SpawnPoint not found. id={spawnId} (fallback to default id={config.defaultSpawnId})");

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

        var camFollow = Object.FindFirstObjectByType<Cainos.PixelArtTopDown_Basic.CameraFollow>();
        if (camFollow != null)
        {
            camFollow.target = player;
            camFollow.RecalculateOffset();
            camFollow.SnapNow();
        }


        if (config.debugLog)
            Debug.Log($"[SpawnManager] Spawned at id={target.id} pos={target.transform.position}");

        if (config.clearKeyAfterUse)
            PlayerPrefs.DeleteKey(config.spawnIdKey);
    }

    public override void Update()
    {

    }

    public override void Destory()
    {
 
    }
}