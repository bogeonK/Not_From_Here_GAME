using UnityEngine;

[CreateAssetMenu(menuName = "Game/Config/BattleManagerConfigSO")]
public class BattleManagerConfigSO : BaseScriptableObject
{
    [Header("Art")]
    public Sprite backgroundSprite;
    public Sprite bossSprite;

    [Header("Stats")]
    public int playerMaxHP = 100;
    public int enemyMaxHP = 80;

    [Header("Values")]
    public int playerAttackDamage = 20;
    public int enemyAttackDamage = 15;
    public int potionHealAmount = 30;

    [Header("Delays (seconds)")]
    public float introDelay = 0.8f;
    public float enemyDelay = 0.8f;
}