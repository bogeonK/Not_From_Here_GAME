using UnityEngine;

[CreateAssetMenu(menuName = "Config/SpawnManagerConfig")]
public class SpawnManagerConfigSO : BaseScriptableObject
{
    [Header("PlayerPrefs Key")]
    public string spawnIdKey = "SpawnId";

    [Header("½ºÆù Id")]
    public string defaultSpawnId = "Default";

    [Header("Options")]
    public bool clearKeyAfterUse = true;

    [Header("How to find Player")]
    public bool useControllerPlayerTransformFirst = true;
    public string playerTag = "Player";

    [Header("Debug")]
    public bool debugLog = false;
}