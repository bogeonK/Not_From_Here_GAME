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

        if (_config.logPhase) Debug.Log("EndingSystemManager Init");
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
            Debug.Log($"EndingSystemManager : {trigger.id} / phase={trigger.phase}");
    }

    private void OnStoryPhase(object payload)
    {
        if (payload is not StoryPhase phase) return;

        if (_config.logPhase) Debug.Log($"EndingSystemManager: {phase}");

        TryTriggerPhase(phase);
    }

    public void DebugPrintArmedTriggers()
    {
        Debug.Log("베드엔딩 활성화 목록");

        if (_armed.Count == 0)
        {
            Debug.Log("없음");
            return;
        }

        for (int i = 0; i < _armed.Count; i++)
        {
            var t = _armed[i];
            Debug.Log(
                $"[{i}] ID: {t.id} | Phase: {t.phase} | Priority: {t.priority} | Source: {t.sourceNpcId}"
            );
        }
    }

    public bool TryTriggerPhase(StoryPhase phase)
    {
        if (_ended) return true;

        var trigger = _armed
            .Where(t => t.phase == phase)
            .OrderBy(t => t.priority)
            .FirstOrDefault();

        if (trigger == null) return false;

        _ended = true;
        Debug.Log($"베드엔딩: {trigger.id} (src={trigger.sourceNpcId})");

        var p = FindPresentation(trigger.id);

        if (GameController.instance != null && GameController.instance.DialogueUI != null)
        {
            if (p != null)
            {
                GameController.instance.DialogueUI.OpenBadEnding(p.art, p.line1, p.line2);
            }
            else
            {

                GameController.instance.DialogueUI.OpenBadEnding(
                    null,
                    "당신은 베드엔딩을 맞이했다.",
                    $"{trigger.id} - Bad Ending"
                );
            }
        }

        return true;
    }

    private EndingPresentation FindPresentation(EndingId id)
    {
        if (_config == null || _config.presentations == null) return null;

        for (int i = 0; i < _config.presentations.Count; i++)
        {
            if (_config.presentations[i].id == id)
                return _config.presentations[i];
        }
        return null;
    }

    public bool ResolveAfterBoss()
    {
        if (_ended) return true;

        if (TryTriggerPhase(StoryPhase.AfterBoss))
            return true;

        TriggerHappyEnding();
        return true;
    }

    public void TriggerHappyEnding()
    {
        if (_ended) return;

        _ended = true;
        Debug.Log("해피엔딩");

        var p = FindPresentation(EndingId.AfterBossHappyEnding);

        if (GameController.instance != null && GameController.instance.DialogueUI != null)
        {
            if (p != null)
                GameController.instance.DialogueUI.OpenBadEnding(p.art, p.line1, p.line2);
            else
                GameController.instance.DialogueUI.OpenBadEnding(
                    null,
                    "모든 이세계인을 가려내고 마왕을 처치했다!",
                    "해피엔딩"
                );
        }
    }
}
