using UnityEngine;

/// <summary>
/// GameObject를 좌우로 왕복 이동시키는 디버그용 컴포넌트.
/// Rigidbody2D.MovePosition을 사용하여 물리 시스템과 호환.
/// </summary>
public class PingPongMover : MonoBehaviour
{
    [Tooltip("왕복 이동 속도")]
    [SerializeField] private float speed = 3f;

    [Tooltip("시작 위치 기준 좌우 이동 거리")]
    [SerializeField] private float distance = 3f;

    private Rigidbody2D rb;
    private Vector2 startPosition;
    private int direction = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        if (rb.constraints == RigidbodyConstraints2D.FreezeAll) return;

        Vector2 current = rb.position;
        Vector2 next = current + Vector2.right * (direction * speed * Time.fixedDeltaTime);

        if (next.x >= startPosition.x + distance)
            direction = -1;
        else if (next.x <= startPosition.x - distance)
            direction = 1;

        rb.MovePosition(next);
    }
}
