using System;

[Serializable]
public class BadEndingTrigger
{
    public BadEndingId id;
    public StoryPhase phase;
    public int priority;       //이벤트 우선순위
    public string sourceNpcId; // 엔딩출처(NPC))

    public BadEndingTrigger(BadEndingId id, StoryPhase phase, int priority, string sourceNpcId)
    {
        this.id = id;
        this.phase = phase;
        this.priority = priority;
        this.sourceNpcId = sourceNpcId;
    }
}
