using System.Collections;
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

    [Header("Boss Art")]
    [SerializeField] private Image bossArtImage;          
    [SerializeField] private RectTransform bossArtRect;   
    [SerializeField] private float bossShakeDuration = 0.25f;
    [SerializeField] private float bossShakeStrength = 10f;   
    [SerializeField] private int bossShakeVibrato = 12;

    [Header("Background")]
    [SerializeField] private Image backgroundImage;

    [Header("Highlight")]
    public string prefixSelected = "> ";
    public string prefixNormal = "  ";

    private BattleManager manager;

    private int selectedIndex = 0;
    private bool inputEnabled = false;

    private string[] optionRaw;

    private Vector2 bossBaseAnchoredPos;
    private Coroutine bossShakeRoutine;

    private void Awake()
    {
        SetBattleVisualsActive(false);
    }

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

        if (bossArtRect == null && bossArtImage != null)
            bossArtRect = bossArtImage.rectTransform;

        if (bossArtRect != null)
            bossBaseAnchoredPos = bossArtRect.anchoredPosition;

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
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space))
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

    //보스 연출
    public void SetBossArt(Sprite sprite)
    {
        if (bossArtImage == null) return;
        bossArtImage.sprite = sprite;
        bossArtImage.enabled = (sprite != null);
    }

    public void PlayBossHitShake()
    {
        if (bossArtRect == null) return;

        if (bossShakeRoutine != null) StopCoroutine(bossShakeRoutine);
        bossShakeRoutine = StartCoroutine(CoShakeBoss());
    }

    private IEnumerator CoShakeBoss()
    {
        float t = 0f;
        while (t < bossShakeDuration)
        {
            t += Time.deltaTime;

          
            float x = Random.Range(-bossShakeStrength, bossShakeStrength);
            float y = Random.Range(-bossShakeStrength, bossShakeStrength);

            bossArtRect.anchoredPosition = bossBaseAnchoredPos + new Vector2(x, y);
            yield return null;
        }

        bossArtRect.anchoredPosition = bossBaseAnchoredPos;
        bossShakeRoutine = null;
    }

    public void Show()
    {
        if (root != null) root.SetActive(true);
        else gameObject.SetActive(true);

        SetBattleVisualsActive(true);
    }

    public void Hide()
    {
        SetBattleVisualsActive(false);
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

    //전투배경
    public void SetBackground(Sprite sprite)
    {
        if (backgroundImage == null) return;
        backgroundImage.sprite = sprite;
        backgroundImage.enabled = (sprite != null);
    }

    public void SetBattleVisualsActive(bool active)
    {
        if (bossArtImage != null)
            bossArtImage.enabled = active && bossArtImage.sprite != null;

        if (backgroundImage != null)
            backgroundImage.enabled = active && backgroundImage.sprite != null;
    }
}