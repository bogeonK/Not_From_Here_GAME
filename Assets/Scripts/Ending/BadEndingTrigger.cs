using System;

[Serializable]
public class BadEndingTrigger
{
    public BadEndingId id;
    public StoryPhase phase;
    public int priority;       // 같은 phase 안에서 충돌 시(낮을수록 먼저)
    public string sourceNpcId; // 어느 NPC에서 왔는지(디버그용)

    public BadEndingTrigger(BadEndingId id, StoryPhase phase, int priority, string sourceNpcId)
    {
        this.id = id;
        this.phase = phase;
        this.priority = priority;
        this.sourceNpcId = sourceNpcId;
    }
}
