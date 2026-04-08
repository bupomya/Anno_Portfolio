using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Tooltip("플레이어 피격 시 카메라 흔들림 강도")]
    [SerializeField] private float hitIntensity = 0.5f;

    [Tooltip("패리 성공 시 카메라 흔들림 강도")]
    [SerializeField] private float parryIntensity = 0.8f;

    [Tooltip("공격 시 카메라 흔들림 강도")]
    [SerializeField] private float attackIntensity = 0.15f;

    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shake(float intensity)
    {
        impulseSource.GenerateImpulse(intensity);
    }

    public void ShakeOnHit()
    {
        Shake(hitIntensity);
    }

    public void ShakeOnParry()
    {
        Shake(parryIntensity);
    }

    public void ShakeOnAttack()
    {
        Shake(attackIntensity);
    }
}
