using System;
using UnityEngine;
using UnityEngine.UI;

public class NpcChoicePopupUI : MonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Button killButton;
    [SerializeField] private Button spareButton;

    public void Open(string npcName, Action onKill, Action onSpare)
    {
        titleText.text = npcName;

        killButton.onClick.RemoveAllListeners();
        spareButton.onClick.RemoveAllListeners();

        killButton.onClick.AddListener(() => onKill?.Invoke());
        spareButton.onClick.AddListener(() => onSpare?.Invoke());
    }
}
