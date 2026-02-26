using System;

[Serializable]
public class BadEndingTrigger
{
    public EndingId id;
    public StoryPhase phase;
    public int priority;      
    public string sourceNpcId; 

    public BadEndingTrigger(EndingId id, StoryPhase phase, int priority, string sourceNpcId)
    {
        this.id = id;
        this.phase = phase;
        this.priority = priority;
        this.sourceNpcId = sourceNpcId;
    }
}
