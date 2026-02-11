public class PlayerIdle : IPlayerState
{
    private readonly Player player;

    public PlayerIdle(Player player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.Stop();
    }

    public void Update()
    {
        // 입력이 생기면 Move로
        if (player.HasMoveInput())
            player.SM.ChangeState(player.MoveState);
    }

    public void FixedUpdate()
    {
        player.Stop();
    }

    public void Exit() { }
}
