using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcChoicePopupUI_JRPG : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private RectTransform choiceBox;
    [SerializeField] private RectTransform highlight;
    [SerializeField] private Transform choicesRoot;         
    [SerializeField] private TMP_Text choiceTextPrefab;     
    [SerializeField] private TMP_Text promptText;           

    [Header("Input")]
    [SerializeField] private KeyCode upKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode downKey = KeyCode.DownArrow;
    [SerializeField] private KeyCode confirmKey = KeyCode.Return; 
    [SerializeField] private KeyCode confirmKey2 = KeyCode.F;     
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

    private readonly List<TMP_Text> spawnedTexts = new();
    private int index = 0;
    private Action<int> onConfirm;
    private Action onCancel;

    private bool isOpen;

    void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyDown(upKey)) Move(-1);
        if (Input.GetKeyDown(downKey)) Move(+1);

        if (Input.GetKeyDown(confirmKey) || Input.GetKeyDown(confirmKey2))
            Confirm();

        if (Input.GetKeyDown(cancelKey))
            Cancel();
    }

    public void Open(List<string> choices, Action<int> onConfirm, Action onCancel = null, string prompt = "어떻게 할까?")
    {
        gameObject.SetActive(true);
        isOpen = true;

        this.onConfirm = onConfirm;
        this.onCancel = onCancel;

        if (promptText != null) promptText.text = prompt;

        for (int i = 0; i < spawnedTexts.Count; i++)
            Destroy(spawnedTexts[i].gameObject);
        spawnedTexts.Clear();

        for (int i = 0; i < choices.Count; i++)
        {
            var t = Instantiate(choiceTextPrefab, choicesRoot);
            t.text = choices[i];
            spawnedTexts.Add(t);
        }

        index = 0;

        Canvas.ForceUpdateCanvases();
        UpdateHighlight();
    }

    public void Close()
    {
        isOpen = false;
        gameObject.SetActive(false);
    }

    private void Move(int dir)
    {
        if (spawnedTexts.Count == 0) return;

        index += dir;
        if (index < 0) index = spawnedTexts.Count - 1;
        if (index >= spawnedTexts.Count) index = 0;

        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        if (spawnedTexts.Count == 0 || highlight == null) return;

        var target = spawnedTexts[index].rectTransform;

        highlight.SetAsFirstSibling();

        highlight.position = target.position;


        var size = highlight.sizeDelta;
        size.y = target.sizeDelta.y + 10f;   
        size.x = choiceBox.rect.width - 24f; 
        highlight.sizeDelta = size;
    }

    private void Confirm()
    {
        onConfirm?.Invoke(index);
        Close();
    }

    private void Cancel()
    {
        onCancel?.Invoke();
        Close();
    }
}