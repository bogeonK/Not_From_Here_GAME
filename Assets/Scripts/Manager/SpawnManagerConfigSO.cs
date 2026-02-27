using UnityEngine;

[CreateAssetMenu(menuName = "Config/SpawnManagerConfig")]
public class SpawnManagerConfigSO : BaseScriptableObject
{
    [Header("PlayerPrefs Key")]
    public string spawnIdKey = "SpawnId";

    [Header("스폰 Id")]
    public string defaultSpawnId = "Default";

    [Header("Options")]
    public bool clearKeyAfterUse = true;

    public bool useControllerPlayerTransformFirst = true;
    public string playerTag = "Player";

    [Header("Debug")]
    public bool debugLog = false;


    [Header("인게임 진입 대사 설정")]
    [TextArea(2, 5)]
    public string[] enterGameLines;
}