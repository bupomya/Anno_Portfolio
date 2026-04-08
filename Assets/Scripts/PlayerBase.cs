using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Animator))]
public abstract class PlayerBase : CharacterBase, IMovable
{
    [Header("Movement")]
    [Tooltip("플레이어 이동 속도")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("점프 시 적용되는 수직 속도")]
    [SerializeField] private float jumpForce = 12f;

    [Header("Physics")]
    [Tooltip("플레이어에 적용되는 중력 배율")]
    [SerializeField] private float gravityScale = 3f;

    [Header("Mana")]
    [Tooltip("최대 MP")]
    [SerializeField] private float maxMp = 100f;

    [Tooltip("MP 자동 회복 간격(초)")]
    [SerializeField] private float mpRegenInterval = 1f;

    [Tooltip("회복 간격마다 충전되는 MP 양")]
    [SerializeField] private float mpRegenAmount = 5f;

    [Header("MP Regen Delay")]
    [Tooltip("MP 소모 후 회복이 시작되기까지의 대기 시간(초)")]
    [SerializeField] private float mpRegenDelay = 2f;

    [Header("Ground Check")]
    [Tooltip("착지 판정에 사용할 레이어 마스크 (Ground 레이어 선택)")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("착지 판정 OverlapBox의 중심 오프셋 (캐릭터 기준)")]
    [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, 0f);

    [Tooltip("착지 판정 OverlapBox의 크기")]
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.1f);

    [Header("Rest")]
    [Tooltip("입력이 없을 때 휴식 애니메이션이 시작되기까지의 대기 시간(초)")]
    [SerializeField] private float restDelay = 5f;

    protected Rigidbody2D rb;
    protected Animator animator;

    public bool InputLocked { get; set; }
    public bool MovementLocked { get; set; }

    protected float moveInput;
    private bool jumpRequested;
    private bool isGroundedField;
    private float idleTimer;
    private float mpRegenTimer;
    private float mpRegenDelayTimer;
    private float currentMp;

    public float SpeedMultiplier { get; set; } = 1f;
    public float JumpMultiplier { get; set; } = 1f;

    public float CurrentMp
    {
        get => currentMp;
        set
        {
            float clamped = Mathf.Clamp(value, 0f, maxMp);
            if (clamped < currentMp)
                mpRegenDelayTimer = mpRegenDelay;
            currentMp = clamped;
        }
    }

    public float MaxMp => maxMp;
    public float JumpForce => jumpForce * JumpMultiplier;
    public bool IsGrounded => isGroundedField;
    public Vector2 GroundCheckOffset => groundCheckOffset;
    public Vector2 GroundCheckSize => groundCheckSize;

    protected static readonly int SpeedHash = Animator.StringToHash("Speed");
    protected static readonly int VelocityYHash = Animator.StringToHash("VelocityY");
    protected static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    protected static readonly int RestHash = Animator.StringToHash("Rest");
    protected static readonly int DeathHash = Animator.StringToHash("Death");

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.gravityScale = gravityScale;
        currentMp = maxMp;
    }

    // --- IMovable ---

    public void SetMoveInput(float input)
    {
        moveInput = input;
    }

    public void RequestJump()
    {
        if (isGroundedField && !IsMovementBlocked())
            jumpRequested = true;
    }

    // --- Public API ---

    public bool IsLocked()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("Lock");
    }

    public void ResetIdleTimer()
    {
        idleTimer = 0f;
        animator.SetBool(RestHash, false);
    }

    // --- Unity Lifecycle ---

    private void Update()
    {
        if (IsDead || InputLocked)
        {
            moveInput = 0f;
            return;
        }

        ReadInput();
        RegenMp();

        bool hasInput = Mathf.Abs(moveInput) > 0.1f || jumpRequested;
        HandleRest(hasInput);

        // Flip sprite
        if (moveInput != 0 && !IsMovementBlocked())
            spriteRenderer.flipX = moveInput < 0;

        // Animator parameters
        animator.SetFloat(SpeedHash, Mathf.Abs(moveInput));
        animator.SetFloat(VelocityYHash, rb.linearVelocity.y);
        animator.SetBool(IsGroundedHash, isGroundedField);
    }

    private void FixedUpdate()
    {
        if (IsDead) return;

        Collider2D groundCollider = Physics2D.OverlapBox(
            (Vector2)transform.position + groundCheckOffset,
            groundCheckSize, 0f, groundLayer);
        isGroundedField = groundCollider != null;

        Vector2 platformVelocity = Vector2.zero;
        if (groundCollider != null && groundCollider.attachedRigidbody != null)
            platformVelocity = groundCollider.attachedRigidbody.linearVelocity;

        if (InputLocked)
        {
            rb.linearVelocity = new Vector2(platformVelocity.x, rb.linearVelocity.y + platformVelocity.y);
            return;
        }

        float speed = IsMovementBlocked() ? 0f : moveInput * moveSpeed * SpeedMultiplier;
        rb.linearVelocity = new Vector2(speed + platformVelocity.x, rb.linearVelocity.y + platformVelocity.y);

        if (jumpRequested && isGroundedField)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * JumpMultiplier);
        }
        jumpRequested = false;
    }

    public override void Die()
    {
        if (IsDead) return;
        IsDead = true;
        animator.SetTrigger(DeathHash);
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    // --- Internal ---

    private bool IsMovementBlocked() => MovementLocked || IsLocked();

    private void HandleRest(bool hasInput)
    {
        if (hasInput)
        {
            idleTimer = 0f;
            animator.SetBool(RestHash, false);
            return;
        }

        if (isGroundedField && !IsMovementBlocked())
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= restDelay)
                animator.SetBool(RestHash, true);
        }
        else
        {
            idleTimer = 0f;
        }
    }

    private void RegenMp()
    {
        if (currentMp >= maxMp) return;

        if (mpRegenDelayTimer > 0f)
        {
            mpRegenDelayTimer -= Time.deltaTime;
            mpRegenTimer = 0f;
            return;
        }

        mpRegenTimer += Time.deltaTime;
        if (mpRegenTimer >= mpRegenInterval)
        {
            mpRegenTimer = 0f;
            currentMp = Mathf.Min(currentMp + mpRegenAmount, maxMp);
        }
    }

    /// <summary>
    /// Subclass reads input and calls SetMoveInput() / RequestJump().
    /// </summary>
    protected abstract void ReadInput();
}
