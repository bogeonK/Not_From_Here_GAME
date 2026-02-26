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

    [Header("Prompt")]
    [TextArea(3, 10)] public string systemPromptWorld1;
    [TextArea(3, 10)] public string systemPromptOtherWorld;

    public string GetSystemPrompt(NpcIdentityType identity)
    {
        return identity == NpcIdentityType.OtherWorld
            ? systemPromptOtherWorld
            : systemPromptWorld1;
    }

    [Header("죽인다 or 안죽인다 선택지")]
    public EndingId badEndingId;
    public StoryPhase badEndingPhase;
    public int priority = 0;
}
