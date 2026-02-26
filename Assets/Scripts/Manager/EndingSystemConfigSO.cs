using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/EndingSystemConfig")]
public class EndingSystemConfigSO : BaseScriptableObject
{
    public bool logPhase = true;
    public bool logBadEnding = true;

    public List<EndingPresentation> presentations = new();
}
