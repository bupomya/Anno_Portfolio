using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    public static ExplosionController Instance { get; private set; }

    [Header("References")]
    [Tooltip("PlayerBase 참조 (스킬 체크, MP 소모용)")]
    [SerializeField] private PlayerBase player;

    [Header("Charge Settings")]
    [Tooltip("최대 차징 시간(초). 이 시간 동안 누르면 최대 위력")]
    [SerializeField] private float maxChargeTime = 3f;

    [Tooltip("최소 폭발 반경 (차징 시작 시)")]
    [SerializeField] private float minRadius = 0.5f;

    [Tooltip("최대 폭발 반경 (최대 차징 시)")]
    [SerializeField] private float maxRadius = 5f;

    [Header("Force Settings")]
    [Tooltip("최소 발사 힘 (차징 최소 시)")]
    [SerializeField] private float minForce = 5f;

    [Tooltip("최대 발사 힘 (최대 차징 시)")]
    [SerializeField] private float maxForce = 20f;

    [Tooltip("발사 방향에 추가하는 상향 비율. 높을수록 포물선이 높아짐")]
    [SerializeField] private float upwardBias = 0.5f;

    [Header("MP")]
    [Tooltip("최소 차징 시 소모 MP (즉시 릴리즈)")]
    [SerializeField] private float minMpCost = 10f;

    [Tooltip("최대 차징 시 소모 MP (maxChargeTime까지 홀드)")]
    [SerializeField] private float maxMpCost = 50f;

    [Header("Indicator")]
    [Tooltip("차징 범위 시각화용 프리팹 (원형 SpriteRenderer). null이면 인디케이터 없이 동작")]
    [SerializeField] private GameObject chargeIndicatorPrefab;

    private Camera mainCamera;
    private bool isCharging;
    private float chargeTimer;
    private float chargeRatio;
    private float currentRadius;
    private float currentMpCost;
    private float mpAtChargeStart;
    private Vector2 chargeCenter;
    private GameObject chargeIndicatorInstance;

    public bool IsCharging => isCharging;
    public float CurrentMpCost => currentMpCost;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (SkillManager.Instance == null || !SkillManager.Instance.IsActiveSkill(SeasonSkillType.Explosion)) return;
        if (player == null) return;

        if (Input.GetMouseButtonDown(0) && !isCharging)
        {
            TryStartCharge();
        }
        else if (isCharging && Input.GetMouseButton(0))
        {
            UpdateCharge();
        }
        else if (isCharging && Input.GetMouseButtonUp(0))
        {
            Explode();
        }
    }

    private void TryStartCharge()
    {
        if (player.CurrentMp < minMpCost) return;

        isCharging = true;
        chargeTimer = 0f;
        chargeRatio = 0f;
        currentRadius = minRadius;
        currentMpCost = minMpCost;
        mpAtChargeStart = player.CurrentMp;
        chargeCenter = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        ShowIndicator();
    }

    private void UpdateCharge()
    {
        chargeTimer = Mathf.Min(chargeTimer + Time.deltaTime, maxChargeTime);
        chargeRatio = chargeTimer / maxChargeTime;
        currentRadius = Mathf.Lerp(minRadius, maxRadius, chargeRatio);

        float desiredCost = Mathf.Lerp(minMpCost, maxMpCost, chargeRatio);
        currentMpCost = Mathf.Min(desiredCost, mpAtChargeStart);
        chargeRatio = Mathf.InverseLerp(minMpCost, maxMpCost, currentMpCost);
        currentRadius = Mathf.Lerp(minRadius, maxRadius, chargeRatio);

        player.CurrentMp = mpAtChargeStart - currentMpCost;

        UpdateIndicator();
    }

    private void Explode()
    {
        isCharging = false;
        HideIndicator();

        player.CurrentMp = mpAtChargeStart - currentMpCost;

        float currentForce = Mathf.Lerp(minForce, maxForce, chargeRatio);

        Collider2D[] hits = Physics2D.OverlapCircleAll(chargeCenter, currentRadius);
        foreach (Collider2D hit in hits)
        {
            ExplosionInteractable interactable = hit.GetComponent<ExplosionInteractable>();
            if (interactable == null || interactable.IsFlying) continue;

            Vector2 direction = ((Vector2)hit.transform.position - chargeCenter);
            if (direction.sqrMagnitude < 0.001f)
                direction = Vector2.up;
            else
                direction.Normalize();

            direction.y += upwardBias;
            direction.Normalize();

            interactable.Launch(direction * currentForce);
        }

        if (CameraShake.Instance != null)
            CameraShake.Instance.ShakeOnHit();
    }

    private void ShowIndicator()
    {
        if (chargeIndicatorPrefab == null) return;

        if (chargeIndicatorInstance == null)
            chargeIndicatorInstance = Instantiate(chargeIndicatorPrefab);

        chargeIndicatorInstance.transform.position = (Vector3)chargeCenter;
        chargeIndicatorInstance.transform.localScale = Vector3.one * (currentRadius * 2f);
        chargeIndicatorInstance.SetActive(true);
    }

    private void UpdateIndicator()
    {
        if (chargeIndicatorInstance == null) return;

        chargeIndicatorInstance.transform.localScale = Vector3.one * (currentRadius * 2f);
    }

    private void HideIndicator()
    {
        if (chargeIndicatorInstance != null)
            chargeIndicatorInstance.SetActive(false);
    }
}
