using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BadEndingRegistry : MonoBehaviour
{
    public static BadEndingRegistry Instance { get; private set; }

    private readonly List<BadEndingTrigger> armed = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Arm(BadEndingTrigger trigger)
    {
        // 같은 NPC 같은 엔딩 중복 등록 방지
        bool exists = armed.Any(t => t.id == trigger.id && t.sourceNpcId == trigger.sourceNpcId);
        if (exists) return;

        armed.Add(trigger);
        Debug.Log($"BadTrigger= {trigger.id} (phase={trigger.phase}, prio={trigger.priority}, src={trigger.sourceNpcId})");

        EventManager.TriggerEvent(EventType.BAD_TRIGGER_ARMED, trigger);
    }

    public BadEndingTrigger GetFirstForPhase(StoryPhase phase)
    {
        return armed
            .Where(t => t.phase == phase)
            .OrderBy(t => t.priority)
            .FirstOrDefault();
    }

    public void ClearAll() => armed.Clear();
}
