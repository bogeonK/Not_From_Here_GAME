public class PlayerMove : IPlayerState
{
    private readonly Player player;

    public PlayerMove(Player player)
    {
        this.player = player;
    }

    public void Enter() { }

    public void Update()
    {

        if (!player.HasMoveInput())
            player.SM.ChangeState(player.IdleState);
    }

    public void FixedUpdate()
    {
        player.ApplyMove();
    }

    public void Exit()
    {
        player.Stop();
    }
}
