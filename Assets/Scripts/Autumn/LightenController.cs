using UnityEngine;

public class LightenController : MonoBehaviour
{
    public static LightenController Instance { get; private set; }

    [Header("References")]
    [Tooltip("PlayerBase 참조 (MP 소모용)")]
    [SerializeField] private PlayerBase player;

    [Header("Lighten Settings")]
    [Tooltip("경감 지속 시간(초). 이 시간이 지나면 원래 무게로 복원")]
    [SerializeField] private float duration = 5f;

    [Tooltip("중력 배율 (원래 gravityScale에 곱함). 낮을수록 가벼움")]
    [SerializeField] private float gravityMultiplier = 0.1f;

    [Tooltip("질량 배율 (원래 mass에 곱함). 낮을수록 가벼움")]
    [SerializeField] private float massMultiplier = 0.1f;

    [Header("MP")]
    [Tooltip("1회 경감에 소모되는 MP")]
    [SerializeField] private float mpCost = 20f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool TryLighten(LightenInteractable target)
    {
        if (player == null) return false;
        if (player.CurrentMp < mpCost) return false;

        player.CurrentMp -= mpCost;
        target.ApplyLighten(gravityMultiplier, massMultiplier, duration);
        return true;
    }
}
