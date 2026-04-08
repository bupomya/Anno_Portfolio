using UnityEngine;
using System.Collections;

public class EnemyShooter : CharacterBase
{
    [Tooltip("투사체 발사 간격(초)")]
    [SerializeField] private float fireInterval = 2.5f;

    [Tooltip("발사되는 투사체의 이동 속도")]
    [SerializeField] private float projectileSpeed = 6f;

    [Tooltip("투사체가 생성되는 위치. 비워두면 적 캐릭터 위치에서 발사")]
    [SerializeField] private Transform firePoint;

    private Transform playerTransform;
    private Transform playerHitPoint;
    private float fireTimer;

    protected override void Awake()
    {
        base.Awake();
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerHitPoint = player.transform.Find("HitPoint");
        }
    }

    private void Update()
    {
        if (IsDead || playerTransform == null) return;

        spriteRenderer.flipX = playerTransform.position.x > transform.position.x;

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Fire();
        }
    }

    private void Fire()
    {
        Vector2 spawnPos = firePoint != null
            ? (Vector2)firePoint.position
            : (Vector2)transform.position;
        Vector2 targetPos = playerHitPoint != null
            ? (Vector2)playerHitPoint.position
            : (Vector2)playerTransform.position;
        Vector2 dir = (targetPos - spawnPos).normalized;

        var go = new GameObject("EnemyProjectile");
        go.transform.position = spawnPos;
        go.layer = LayerMask.NameToLayer("EnemyProjectile");

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.15f;

        var proj = go.AddComponent<Projectile>();
        proj.Init(dir, projectileSpeed);
    }

    public override void Die()
    {
        if (IsDead) return;
        IsDead = true;
        StartCoroutine(DieSequence());
    }

    private IEnumerator DieSequence()
    {
        for (int i = 0; i < 5; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.08f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.08f);
        }
        Destroy(gameObject);
    }
}
