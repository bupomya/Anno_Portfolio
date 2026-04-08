using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerBase))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Guard / Parry")]
    [Tooltip("가드 자세가 유지되는 시간(초)")]
    [SerializeField] private float guardDuration = 0.4f;

    [Tooltip("가드 시작 후 패리 판정이 가능한 시간(초)")]
    [SerializeField] private float parryWindow = 0.15f;

    [Tooltip("가드 히트박스의 중심 오프셋 (캐릭터 기준, 바라보는 방향으로 자동 반전)")]
    [SerializeField] private Vector2 guardCheckOffset = new Vector2(0.5f, 0.5f);

    [Tooltip("가드 히트박스의 크기. 이 범위 안에 투사체가 있어야 가드/패리 성공")]
    [SerializeField] private Vector2 guardCheckSize = new Vector2(0.8f, 1.0f);

    private PlayerBase player;
    private Animator animator;
    private InputAction attackAction;
    private InputAction guardAction;

    public bool IsGuarding { get; private set; }
    private float parryTimer;
    private float guardTimer;

    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int GuardHash = Animator.StringToHash("Guard");
    private static readonly int ParryHash = Animator.StringToHash("Parry");

    private void Awake()
    {
        player = GetComponent<PlayerBase>();
        animator = GetComponent<Animator>();
        attackAction = InputSystem.actions.FindAction("Attack");
        guardAction = InputSystem.actions.FindAction("Crouch");
    }

    private void Update()
    {
        if (player.IsDead || player.InputLocked) return;

        if (attackAction.WasPressedThisFrame() || guardAction.WasPressedThisFrame())
            player.ResetIdleTimer();

        // Attack
        if (attackAction.WasPressedThisFrame() && player.IsGrounded && !IsGuarding)
        {
            animator.SetTrigger(AttackHash);
            CameraShake.Instance?.ShakeOnAttack();
        }

        // Guard & Parry
        HandleGuard();

        player.MovementLocked = IsGuarding;
    }

    private void HandleGuard()
    {
        if (guardAction.WasPressedThisFrame() && player.IsGrounded && !IsGuarding && !player.IsLocked())
        {
            IsGuarding = true;
            guardTimer = guardDuration;
            parryTimer = parryWindow;
        }

        if (IsGuarding)
        {
            guardTimer -= Time.deltaTime;
            if (parryTimer > 0)
                parryTimer -= Time.deltaTime;

            if (guardTimer <= 0)
                IsGuarding = false;
        }

        animator.SetBool(GuardHash, IsGuarding);
    }

    public Vector2 GetGuardCenter()
    {
        float dirX = player.FacingDirection;
        Vector2 offset = new Vector2(guardCheckOffset.x * dirX, guardCheckOffset.y);
        return (Vector2)transform.position + offset;
    }

    public Vector2 GetGuardSize() => guardCheckSize;

    public bool TryParry()
    {
        if (IsGuarding && parryTimer > 0)
        {
            animator.SetTrigger(ParryHash);
            parryTimer = 0;
            return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        float dirX = 1f;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            dirX = sr.flipX ? -1f : 1f;

        Vector3 guardCenter = transform.position
            + new Vector3(guardCheckOffset.x * dirX, guardCheckOffset.y, 0f);

        if (IsGuarding && parryTimer > 0)
            Gizmos.color = Color.yellow;
        else if (IsGuarding)
            Gizmos.color = Color.cyan;
        else
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);

        Gizmos.DrawWireCube(guardCenter, (Vector3)guardCheckSize);
    }
}
