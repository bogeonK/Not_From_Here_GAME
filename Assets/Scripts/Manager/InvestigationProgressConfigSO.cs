using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/InvestigationProgressConfig")]
public class InvestigationProgressConfigSO : BaseScriptableObject
{
    [Header("대화완료가 필요한 NPC 목록")]
    public List<string> requiredNpcIds = new();

    [Header("로그")]
    public bool log = true;

    [TextArea]
    public string notReadyLine = "아직 준비할 게 남아있어..";
}