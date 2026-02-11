using UnityEngine;

public class PlayerIdle : IPlayerState
{
    private readonly Player p;
    private static readonly int Speed = Animator.StringToHash("Speed");

    public PlayerIdle(Player player) => p = player;

    public void Enter()
    {
        p.Stop();
        if (p.Anim != null)
            p.Anim.SetFloat(Speed, 0f); 
    }

    public void Update()
    {
        if (p.HasMoveInput())
            p.SM.ChangeState(p.MoveState);
    }

    public void FixedUpdate() { }
    public void Exit() { }
}
