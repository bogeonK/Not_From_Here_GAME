using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;

    public Rigidbody2D Rb { get; private set; }
    public Vector2 MoveInput { get; private set; }

    public PlayerStateMachine SM { get; private set; }

    // States
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
        Debug.Log(MoveInput);

        //대화시 이동차단
        if (IsDialogueOpen())
        {
            MoveInput = Vector2.zero;
            if (SM.Current != IdleState) SM.ChangeState(IdleState);
            Stop();
            return;
        }

        ReadInput();
        SM.Update();
    }

    private void FixedUpdate()
    {
        if (IsDialogueOpen())
        {
            Stop();
            return;
        }

        SM.FixedUpdate();
    }

    private bool IsDialogueOpen()
    {
        return GameController.instance != null
            && GameController.instance.DialogueUI != null
            && GameController.instance.DialogueUI.IsOpen;
    }

    private void ReadInput()
    {
        float x = Input.GetAxisRaw("Horizontal"); // A/D
        float y = Input.GetAxisRaw("Vertical");   // W/S
        MoveInput = new Vector2(x, y).normalized;
    }

    public bool HasMoveInput(float deadZone = 0.001f)
        => MoveInput.sqrMagnitude > deadZone;

    public void Stop()
        => Rb.linearVelocity = Vector2.zero;

    public void ApplyMove()
        => Rb.linearVelocity = MoveInput * moveSpeed;
}
