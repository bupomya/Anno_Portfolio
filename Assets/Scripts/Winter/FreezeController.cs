using UnityEngine;

public class FreezeController : MonoBehaviour
{
    public static FreezeController Instance { get; private set; }

    [Header("References")]
    [Tooltip("PlayerBase 참조 (MP 소모용)")]
    [SerializeField] private PlayerBase player;

    [Header("Freeze Settings")]
    [Tooltip("결빙 지속 시간(초). 이 시간이 지나면 원래 상태로 복원")]
    [SerializeField] private float duration = 5f;

    [Header("MP")]
    [Tooltip("1회 결빙에 소모되는 MP")]
    [SerializeField] private float mpCost = 20f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool TryFreeze(FreezeInteractable target)
    {
        if (player == null) return false;
        if (player.CurrentMp < mpCost) return false;

        player.CurrentMp -= mpCost;
        target.ApplyFreeze(duration);
        return true;
    }
}
