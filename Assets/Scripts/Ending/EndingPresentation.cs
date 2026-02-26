using System;
using UnityEngine;

[Serializable]
public class EndingPresentation
{
    public EndingId id;
    public Sprite art;

    [TextArea(2, 6)] public string line1;
    [TextArea(2, 6)] public string line2;
}