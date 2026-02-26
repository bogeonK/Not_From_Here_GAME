using UnityEngine;

public enum BattleState
{
    None,
    Intro,
    PlayerTurn,
    EnemyTurn,
    Victory,
    Defeat,
    Escape
}

public class BattleManager : baseManager
{
    private readonly BattleManagerConfigSO config;

    private int playerHP;
    private int enemyHP;

    private BattleState state = BattleState.None;
    private float waitTimer = 0f;
    private bool enemyAttackShown = false;

    private BattleUI battleUI;

    public BattleState State => state;

    public BattleManager(BattleManagerConfigSO config)
    {
        this.config = config;
    }

    public override void Init()
    {
        if (config == null)
        {
            return;
        }

        ResetStats();
        state = BattleState.None;
        waitTimer = 0f;

        EnsureUI();
        battleUI?.Hide();
    }

    public override void ActiveOff()
    {
        EnsureUI();
        battleUI?.SetInputEnabled(false);
        battleUI?.Hide();
    }

    public override void Update()
    {
        if (state == BattleState.None) return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer > 0f) return;
        }

        if (state == BattleState.Intro)
        {
            EnterPlayerTurn();
        }
        else if (state == BattleState.EnemyTurn)
        {
            if (!enemyAttackShown)
            {
                // 1단계: 마왕 공격 실행 + 메시지 보여주기
                ResolveEnemyAttack();
                enemyAttackShown = true;

                // 마왕 공격 메시지를 보여줄 시간
                waitTimer = config.enemyDelay;
            }
            else
            {
                // 2단계: 메시지 보여준 뒤 플레이어 턴으로 복귀
                enemyAttackShown = false;
                EnterPlayerTurn();
            }
        }
    }

    public override void Destory()
    {
    }

    private void ResetStats()
    {
        playerHP = config.playerMaxHP;
        enemyHP = config.enemyMaxHP;
    }

    public void StartBattle()
    {
        if (config == null)
        {
            return;
        }

        EnsureUI();
        ResetStats();

        state = BattleState.Intro;
        waitTimer = config.introDelay;

        if (battleUI != null)
        {
            battleUI.Show();
            battleUI.SetInputEnabled(false);
            battleUI.SetOptionsVisible(false);
            battleUI.SetMessage("마왕이 나타난다...!");
            battleUI.SetHP(playerHP, config.playerMaxHP, enemyHP, config.enemyMaxHP);
        }
    }

    //플레이어턴
    private void EnterPlayerTurn()
    {
        state = BattleState.PlayerTurn;

        if (battleUI != null)
        {
            battleUI.SetOptionsVisible(true);
            battleUI.SetMessage("무엇을 할 것인가?");
            battleUI.SetHP(playerHP, config.playerMaxHP, enemyHP, config.enemyMaxHP);
            battleUI.SetInputEnabled(true);
        }
    }

    public void PlayerAttack()
    {
        //무기상인 베드엔딩 체크
        var ending = GameController.instance.GetManager<EndingSystemManager>();

        if (ending.TryTriggerPhase(StoryPhase.WeaponBroken))
        {
            if (battleUI != null)
            {
                battleUI.SetInputEnabled(false);
                battleUI.Hide();
            }
            return;
        }

        if (state != BattleState.PlayerTurn) return;

        battleUI?.SetInputEnabled(false);
        battleUI?.SetOptionsVisible(false);

        enemyHP -= config.playerAttackDamage;
        if (enemyHP < 0) enemyHP = 0;

        if (battleUI != null)
        {
            battleUI.SetHP(playerHP, config.playerMaxHP, enemyHP, config.enemyMaxHP);
            battleUI.SetMessage($"당신의 공격! {config.playerAttackDamage} 데미지를 입혔다!");
        }


        if (enemyHP <= 0)
        {
            // 이세계인 있으면 베드엔딩, 없으면 해피엔딩
            if (ending != null && ending.ResolveAfterBoss())
            {
                battleUI?.SetInputEnabled(false);
                battleUI?.Hide();
                return;
            }

            EndBattle(true);
            return;
        }

        state = BattleState.EnemyTurn;
        enemyAttackShown = false;
        waitTimer = config.enemyDelay;
    }

    public void UsePotion()
    {
        //if (state != BattleState.PlayerTurn) return;

        //battleUI?.SetInputEnabled(false);

        //playerHP = Mathf.Min(config.playerMaxHP, playerHP + config.potionHealAmount);

        //state = BattleState.EnemyTurn;
        //waitTimer = config.enemyDelay;

        //if (battleUI != null)
        //{
        //    battleUI.SetHP(playerHP, config.playerMaxHP, enemyHP, config.enemyMaxHP);
        //    battleUI.SetMessage("마왕의 차례...");
        //}
        if (state != BattleState.PlayerTurn) return;

        battleUI?.SetInputEnabled(false);
        battleUI?.SetOptionsVisible(false);

        var ending = GameController.instance.GetManager<EndingSystemManager>();

        // 물약 베드엔딩 체크
        if (ending.TryTriggerPhase(StoryPhase.OnPotionUse))
        {
            if (battleUI != null)
            {
                battleUI.SetInputEnabled(false);
                battleUI.Hide(); 
            }
            return;
        }


        //정상 회복
        playerHP = Mathf.Min(config.playerMaxHP, playerHP + config.potionHealAmount);

        if (battleUI != null)
        {
            battleUI.SetHP(playerHP, config.playerMaxHP, enemyHP, config.enemyMaxHP);
            battleUI.SetMessage($"포션을 마셨다! HP가 {config.potionHealAmount} 회복됐다.");
        }

        state = BattleState.EnemyTurn;
        waitTimer = config.enemyDelay;
    }

    public void Escape()
    {
        if (state != BattleState.PlayerTurn) return;

        battleUI?.SetInputEnabled(false);

        state = BattleState.Escape;
        EndBattle(null);
    }

    //마왕턴
    private void ResolveEnemyAttack()
    {
        battleUI?.SetOptionsVisible(false);
        playerHP -= config.enemyAttackDamage;
        if (playerHP < 0) playerHP = 0;

        if (battleUI != null)
        {
            battleUI.SetHP(playerHP, config.playerMaxHP, enemyHP, config.enemyMaxHP);
            battleUI.SetMessage($"마왕의 공격! {config.enemyAttackDamage} 데미지를 입혔다!");
        }

        //방어구상인 베드엔딩 체크
        var ending = GameController.instance.GetManager<EndingSystemManager>();
        if (ending != null && ending.TryTriggerPhase(StoryPhase.ArmorHit))
        {
            battleUI?.SetInputEnabled(false);
            battleUI?.Hide();
            return;
        }

        if (playerHP <= 0)
        {
            EndBattle(false);
            return;
        }

        //EnterPlayerTurn();
    }

    private void EndBattle(bool? victory)
    {
        battleUI?.SetInputEnabled(false);

        if (victory == true)
        {
            state = BattleState.Victory;
            battleUI?.SetMessage("승리!");
        }
        else if (victory == false)
        {
            state = BattleState.Defeat;
            battleUI?.SetMessage("패배...");
        }
        else
        {
            battleUI?.SetMessage("도망쳤다!");
        }

        battleUI?.Hide();
        state = BattleState.None;
        waitTimer = 0f;
    }

    private void EnsureUI()
    {
        if (battleUI != null) return;

        if (GameController.instance != null)
            battleUI = GameController.instance.GetComponentInChildren<BattleUI>(true);

        if (battleUI == null)
            battleUI = Object.FindFirstObjectByType<BattleUI>(FindObjectsInactive.Include);

        if (battleUI == null)
        {
            return;
        }

        battleUI.Bind(this);
    }
}