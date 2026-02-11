public class PlayerStateMachine
{
    public IPlayerState Current { get; private set; }

    public void ChangeState(IPlayerState next)
    {
        if (Current == next) return;

        Current?.Exit();
        Current = next;
        Current?.Enter();
    }

    public void Update() => Current?.Update();
    public void FixedUpdate() => Current?.FixedUpdate();
}
