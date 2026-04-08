using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class LightenInteractable : MonoBehaviour
{
    [Header("Lighten Visual")]
    [Tooltip("경감 상태일 때 스프라이트에 적용할 틴트 색상")]
    [SerializeField] private Color lightenTint = new Color(0.7f, 1f, 0.9f, 0.6f);

    [Tooltip("복원 경고 깜박임이 시작되기까지 남은 시간 비율 (0.3 = 남은 시간 30%부터 깜박임)")]
    [SerializeField] private float blinkStartRatio = 0.3f;

    [Tooltip("깜박임 최소 간격(초). 복원 직전 가장 빠른 속도")]
    [SerializeField] private float minBlinkInterval = 0.08f;

    [Tooltip("깜박임 최대 간격(초). 깜박임 시작 시 가장 느린 속도")]
    [SerializeField] private float maxBlinkInterval = 0.4f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float originalGravityScale;
    private float originalMass;
    private RigidbodyConstraints2D originalConstraints;
    private bool isLightened;
    private Coroutine restoreCoroutine;

    public bool IsLightened => isLightened;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        originalGravityScale = rb.gravityScale;
        originalMass = rb.mass;
        originalConstraints = rb.constraints;
    }

    private void OnMouseDown()
    {
        if (SkillManager.Instance == null || !SkillManager.Instance.IsActiveSkill(SeasonSkillType.Lighten)) return;
        if (LightenController.Instance == null) return;
        if (isLightened) return;

        LightenController.Instance.TryLighten(this);
    }

    public void ApplyLighten(float gravityMultiplier, float massMultiplier, float duration)
    {
        isLightened = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = originalGravityScale * gravityMultiplier;
        rb.mass = originalMass * massMultiplier;

        spriteRenderer.color = lightenTint;

        if (restoreCoroutine != null)
            StopCoroutine(restoreCoroutine);
        restoreCoroutine = StartCoroutine(LightenRoutine(duration));
    }

    private IEnumerator LightenRoutine(float duration)
    {
        float blinkTime = duration * blinkStartRatio;
        yield return new WaitForSeconds(duration - blinkTime);

        float elapsed = 0f;
        bool showOriginal = false;

        while (elapsed < blinkTime)
        {
            float progress = elapsed / blinkTime;
            float interval = Mathf.Lerp(maxBlinkInterval, minBlinkInterval, progress);

            showOriginal = !showOriginal;
            spriteRenderer.color = showOriginal ? originalColor : lightenTint;

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        Restore();
    }

    private void Restore()
    {
        rb.gravityScale = originalGravityScale;
        rb.mass = originalMass;
        rb.constraints = originalConstraints;
        rb.linearVelocity = Vector2.zero;
        spriteRenderer.color = originalColor;
        isLightened = false;
        restoreCoroutine = null;
    }
}
