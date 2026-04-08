using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GrowthInteractable : MonoBehaviour
{
    [Tooltip("성장 시작 위치 오프셋 (오브젝트 기준)")]
    [SerializeField] private Vector2 growthOriginOffset = Vector2.zero;

    public Vector2 GrowthOrigin => (Vector2)transform.position + growthOriginOffset;

    private void OnMouseDown()
    {
        if (SkillManager.Instance == null || !SkillManager.Instance.IsActiveSkill(SeasonSkillType.Growth)) return;
        if (SplineGrowthController.Instance == null) return;
        if (SplineGrowthController.Instance.IsGrowing) return;

        SplineGrowthController.Instance.StartGrowth(GrowthOrigin);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GrowthOrigin, 0.15f);
    }
}
