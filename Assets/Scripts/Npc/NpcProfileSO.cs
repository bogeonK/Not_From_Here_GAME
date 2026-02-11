using UnityEngine;

[CreateAssetMenu(menuName = "NPC/NpcProfile")]
public class NpcProfileSO : BaseScriptableObject
{
    [Header("Identity")]
    public string npcId;
    public string displayName;

    public string DisplayName => displayName;

    [Header("Truth")]
    public bool isOtherWorlder; // 이세계인?

    [Header("If NOT killed (Spare) and isOtherWorlder => arm bad ending trigger")]
    public BadEndingId badEndingId;
    public StoryPhase badEndingPhase;
    public int priority = 0;
}
