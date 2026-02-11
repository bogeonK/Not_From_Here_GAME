using UnityEngine;

public class PlayerMove : IPlayerState
{
    private readonly Player p;
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int LastX = Animator.StringToHash("LastX");
    private static readonly int LastY = Animator.StringToHash("LastY");


    public PlayerMove(Player player) => p = player;

    public void Enter() { }

    public void Update()
    {
        if (!p.HasMoveInput())
        {
            p.SM.ChangeState(p.IdleState);
            return;
        }

        var dir = p.MoveInput;
        p.Anim.SetFloat(Speed, 1f);
        p.Anim.SetFloat(MoveX, dir.x);
        p.Anim.SetFloat(MoveY, dir.y);

        p.Anim.SetFloat(LastX, dir.x);
        p.Anim.SetFloat(LastY, dir.y);
    }

    public void FixedUpdate()
    {
        p.ApplyMove();
    }

    public void Exit()
    {
        p.Stop();
    }
}
