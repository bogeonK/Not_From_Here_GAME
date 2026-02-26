using System.Collections.Generic;
using UnityEngine;

public class InvestigationProgressManager : baseManager
{
    private readonly InvestigationProgressConfigSO _config;

    //대화 완료 npcId 저장
    private readonly HashSet<string> _talked = new();

    public InvestigationProgressManager(InvestigationProgressConfigSO config)
    {
        _config = config;
    }

    public override void GetController(GameController controller)
    {
        this.controller = controller;
    }

    public override void Init()
    {
        _talked.Clear();
        if (_config != null && _config.log)
            Debug.Log("[InvestigationProgressManager] Init");
    }

    public override void Update() { }

    public override void ActiveOff() { }

    public override void Destory()
    {
        _talked.Clear();
    }

    public void MarkTalked(string npcId)
    {
        if (string.IsNullOrEmpty(npcId)) return;

        bool added = _talked.Add(npcId);
        if (added && _config != null && _config.log)
            Debug.Log($"[Investigation] talked: {npcId} ({_talked.Count}/{RequiredCount})");
    }

    public int RequiredCount => (_config != null && _config.requiredNpcIds != null)
        ? _config.requiredNpcIds.Count
        : 0;

    public bool IsAllTalked()
    {
        if (_config == null || _config.requiredNpcIds == null || _config.requiredNpcIds.Count == 0)
            return true; 

        for (int i = 0; i < _config.requiredNpcIds.Count; i++)
        {
            var id = _config.requiredNpcIds[i];
            if (string.IsNullOrEmpty(id)) continue;

            if (!_talked.Contains(id))
                return false;
        }

        return true;
    }

    public string GetNotReadyLine()
    {
        return _config != null ? _config.notReadyLine : "아직 준비할 게 남아있어..";
    }

    //디버그용
    public void DebugPrintTalked()
    {
        Debug.Log("대화한 npc");
        foreach (var id in _talked)
            Debug.Log(id);
    }
}