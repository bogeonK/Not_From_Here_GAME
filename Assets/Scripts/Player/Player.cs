using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Animator")]
    [SerializeField] private Animator anim;
    public Animator Anim => anim;

    public Rigidbody2D Rb { get; private set; }
    public Vector2 MoveInput { get; private set; }

    public PlayerStateMachine SM { get; private set; }
    public PlayerIdle IdleState { get; private set; }
    public PlayerMove MoveState { get; private set; }

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();

        SM = new PlayerStateMachine();
        IdleState = new PlayerIdle(this);
        MoveState = new PlayerMove(this);
    }

    private void Start()
    {
        SM.ChangeState(IdleState);
    }

    private void Update()
    {
        //대화,전투 이동차단
        if (IsUIBlocking())
        {
            MoveInput = Vector2.zero;
            if (SM.Current != IdleState) SM.ChangeState(IdleState);
            Stop();
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        MoveInput = new Vector2(x, y).normalized;

        SM.Update();
    }

    private void FixedUpdate()
    {
        if (IsUIBlocking())
        {
            Stop();
            return;
        }

        SM.FixedUpdate();
    }

    private bool IsUIBlocking()
    {
        //대화 
        if (GameController.instance?.DialogueUI?.IsOpen == true)
            return true;

        //전투 상태
        var bm = GameController.instance?.GetManager<BattleManager>();
        if (bm != null && bm.State != BattleState.None)
            return true;

        return false;
    }

    private void ReadInput()
    {
        float x = Input.GetAxisRaw("Horizontal"); 
        float y = Input.GetAxisRaw("Vertical");   
        MoveInput = new Vector2(x, y).normalized;
    }

    public bool HasMoveInput(float deadZone = 0.001f)
        => MoveInput.sqrMagnitude > deadZone;

    public void Stop()
        => Rb.linearVelocity = Vector2.zero;

    public void ApplyMove()
        => Rb.linearVelocity = MoveInput * moveSpeed;
}
