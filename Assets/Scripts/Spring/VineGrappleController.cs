using UnityEngine;
using UnityEngine.InputSystem;

public class VineGrappleController : MonoBehaviour
{
    public static VineGrappleController Instance { get; private set; }

    [Header("References")]
    [Tooltip("PlayerBase 참조 (스킬 체크, MP 소모용)")]
    [SerializeField] private PlayerBase player;

    [Header("Grapple Settings")]
    [Tooltip("덩굴에 매달려 이동하는 속도")]
    [SerializeField] private float grappleSpeed = 15f;

    [Tooltip("덩굴 발사 최대 거리")]
    [SerializeField] private float maxDistance = 15f;

    [Tooltip("목표 지점 도착 판정 거리")]
    [SerializeField] private float arrivalThreshold = 0.5f;

    [Tooltip("덩굴이 부착 가능한 레이어 (Ground, Wall 등)")]
    [SerializeField] private LayerMask grappleLayerMask;

    [Header("MP")]
    [Tooltip("1회 덩굴 발사 고정 MP 비용")]
    [SerializeField] private float mpCost = 25f;

    [Header("Visual")]
    [Tooltip("덩굴 시각화용 LineRenderer (같은 오브젝트 또는 자식에 배치)")]
    [SerializeField] private LineRenderer lineRenderer;

    private Camera mainCamera;
    private Rigidbody2D playerRb;
    private InputAction jumpAction;
    private bool isGrappling;
    private Vector2 grapplePoint;

    public bool IsGrappling => isGrappling;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        mainCamera = Camera.main;
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void Start()
    {
        if (player != null)
            playerRb = player.GetComponent<Rigidbody2D>();

        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (SkillManager.Instance == null || !SkillManager.Instance.IsActiveSkill(SeasonSkillType.VineGrapple))
        {
            if (isGrappling) ReleaseGrapple();
            return;
        }

        if (player == null) return;

        if (Input.GetMouseButtonDown(1) && !isGrappling)
        {
            TryGrapple();
        }
        else if (isGrappling && Input.GetMouseButtonUp(1))
        {
            ReleaseGrapple();
        }

        if (isGrappling)
        {
            if (jumpAction != null && jumpAction.WasPressedThisFrame())
            {
                ReleaseGrappleWithJump();
                return;
            }

            MoveTowardsGrapplePoint();
            UpdateLineRenderer();

            float distance = Vector2.Distance(player.transform.position, grapplePoint);
            if (distance <= arrivalThreshold)
                ReleaseGrapple();
        }
    }

    private void TryGrapple()
    {
        if (player.CurrentMp < mpCost) return;

        Vector2 playerPos = player.transform.position;
        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorld - playerPos).normalized;

        RaycastHit2D hit = Physics2D.Raycast(playerPos, direction, maxDistance, grappleLayerMask);
        if (hit.collider == null) return;

        player.CurrentMp -= mpCost;

        isGrappling = true;
        grapplePoint = hit.point;
        player.MovementLocked = true;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = true;
            UpdateLineRenderer();
        }
    }

    private void MoveTowardsGrapplePoint()
    {
        if (playerRb == null) return;

        Vector2 direction = (grapplePoint - (Vector2)player.transform.position).normalized;
        playerRb.linearVelocity = direction * grappleSpeed;
    }

    private void UpdateLineRenderer()
    {
        if (lineRenderer == null) return;

        lineRenderer.SetPosition(0, player.transform.position);
        lineRenderer.SetPosition(1, (Vector3)grapplePoint);
    }

    private void ReleaseGrapple()
    {
        isGrappling = false;
        player.MovementLocked = false;

        if (playerRb != null)
            playerRb.linearVelocity *= 0.3f;

        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    private void ReleaseGrappleWithJump()
    {
        isGrappling = false;
        player.MovementLocked = false;

        if (playerRb != null)
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, player.JumpForce);

        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }
}
