using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EndingSystemManager : baseManager
{
    private readonly EndingSystemConfigSO _config;
    private readonly List<BadEndingTrigger> _armed = new();

    private bool _ended;

    public EndingSystemManager(EndingSystemConfigSO config)
    {
        _config = config;
    }

    public override void GetController(GameController controller)
    {
    }

    public override void Init()
    {
        _ended = false;
        _armed.Clear();

        EventManager.RegisterEvent(EventType.STORY_PHASE, OnStoryPhase);

        if (_config.logPhase) Debug.Log("[EndingSystemManager] Init");
    }

    public override void ActiveOff()
    {
      
    }

    public override void Update()
    {
       
    }

    public override void Destory()
    {
        EventManager.UnRegisterEvent(EventType.STORY_PHASE, OnStoryPhase);
        _armed.Clear();
    }

    public void ArmTrigger(BadEndingTrigger trigger)
    {
        bool exists = _armed.Any(t => t.id == trigger.id && t.sourceNpcId == trigger.sourceNpcId);
        if (exists) return;

        _armed.Add(trigger);

        if (_config.logBadEnding)
            Debug.Log($"[EndingSystemManager] ARM -> {trigger.id} (phase={trigger.phase})");
    }

    private void OnStoryPhase(object payload)
    {
        if (_ended) return;
        if (payload is not StoryPhase phase) return;

        if (_config.logPhase) Debug.Log($"[EndingSystemManager] Phase arrived: {phase}");

        var trigger = _armed
            .Where(t => t.phase == phase)
            .OrderBy(t => t.priority)
            .FirstOrDefault();

        if (trigger == null) return;

        _ended = true;
        Debug.Log($"[BAD ENDING] {trigger.id} (src={trigger.sourceNpcId})");
    }
}
