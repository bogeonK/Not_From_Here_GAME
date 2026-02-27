using UnityEngine;

[CreateAssetMenu(menuName = "Game/Config/BattleManagerConfigSO")]
public class BattleManagerConfigSO : BaseScriptableObject
{
    [Header("스프라이트")]
    public Sprite backgroundSprite;
    public Sprite bossSprite;

    [Header("HP")]
    public int playerMaxHP = 100;
    public int enemyMaxHP = 80;

    [Header("값")]
    public int playerAttackDamage = 20;
    public int enemyAttackDamage = 15;
    public int potionHealAmount = 30;

    [Header("Delay")]
    public float introDelay = 0.8f;
    public float enemyDelay = 0.8f;
}