using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject root;

    [Header("Texts")]
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI enemyHPText;

    [Header("Options (Top -> Bottom)")]
    public TextMeshProUGUI[] optionTexts; // size 3: 공격/물약/도망

    [Header("Highlight")]
    public string prefixSelected = "▶ ";
    public string prefixNormal = "  ";

    private BattleManager manager;

    private int selectedIndex = 0;
    private bool inputEnabled = false;

    private string[] optionRaw;

    public void Bind(BattleManager mgr)
    {
        manager = mgr;
        string[] defaults = { "1.공격한다", "2.물약 마시기", "3.도망치기" };

        optionRaw = new string[optionTexts.Length];

        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (i < defaults.Length)
            {
                optionTexts[i].text = defaults[i];
                optionRaw[i] = defaults[i];
            }
            else
            {
                optionRaw[i] = optionTexts[i].text;
            }
        }

        selectedIndex = 0;
        RefreshHighlight();
    }

    private void Update()
    {
        if (!inputEnabled) return;

        // 좌/우 이동
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedIndex = (selectedIndex - 1 + optionTexts.Length) % optionTexts.Length;
            RefreshHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedIndex = (selectedIndex + 1) % optionTexts.Length;
            RefreshHighlight();
        }

        // 결정
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelection();
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
        {
            manager?.Escape();
        }
    }

    private void ConfirmSelection()
    {
        if (manager == null) return;

        switch (selectedIndex)
        {
            case 0: manager.PlayerAttack(); break;
            case 1: manager.UsePotion(); break;
            case 2: manager.Escape(); break;
        }
    }

    private void RefreshHighlight()
    {
        if (optionTexts == null || optionTexts.Length == 0) return;

        for (int i = 0; i < optionTexts.Length; i++)
        {
            string prefix = (i == selectedIndex) ? prefixSelected : prefixNormal;
            optionTexts[i].text = prefix + optionRaw[i];
        }
    }

    public void Show()
    {
        if (root != null) root.SetActive(true);
        else gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
        else gameObject.SetActive(false);
    }

    public void SetMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;
    }

    public void SetHP(int pHP, int pMax, int eHP, int eMax)
    {
        if (playerHPText != null) playerHPText.text = $"PLAYER {pHP}/{pMax}";
        if (enemyHPText != null) enemyHPText.text = $"BOSS {eHP}/{eMax}";
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (enabled)
        {
            selectedIndex = 0;
            RefreshHighlight();
        }
    }

    public void SetOptionsVisible(bool visible)
    {
        if (optionTexts == null) return;

        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (optionTexts[i] != null)
                optionTexts[i].gameObject.SetActive(visible);
        }
    }
}