using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("투사체가 자동 파괴되기까지의 시간(초)")]
    [SerializeField] private float lifetime = 5f;

    [Tooltip("충돌 감지에 사용되는 OverlapCircle의 반지름")]
    [SerializeField] private float hitRadius = 0.2f;

    private Vector2 direction;
    private float speed;
    private bool reflected;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private int preReflectMask;
    private int postReflectMask;

    public void Init(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
        if (rb != null)
            rb.linearVelocity = direction * speed;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        if (spriteRenderer.sprite == null)
        {
            var tex = new Texture2D(16, 16);
            tex.filterMode = FilterMode.Point;
            var center = new Vector2(7.5f, 7.5f);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y,
                        Vector2.Distance(new Vector2(x, y), center) <= 7f
                            ? Color.white : Color.clear);
            tex.Apply();
            spriteRenderer.sprite = Sprite.Create(tex,
                new Rect(0, 0, 16, 16), Vector2.one * 0.5f, 32f);
        }

        spriteRenderer.color = Color.cyan;

        preReflectMask = (1 << LayerMask.NameToLayer("Default"))
                       | (1 << LayerMask.NameToLayer("Ground"));
        postReflectMask = (1 << LayerMask.NameToLayer("Enemy"))
                        | (1 << LayerMask.NameToLayer("Ground"));

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        int mask = reflected ? postReflectMask : preReflectMask;
        var hit = Physics2D.OverlapCircle(rb.position, hitRadius, mask);
        if (hit != null)
            HandleCollision(hit);
    }

    private void HandleCollision(Collider2D other)
    {
        if (!reflected)
        {
            var player = other.GetComponent<PlayerBase>();
            if (player != null)
            {
                var combat = other.GetComponent<PlayerCombat>();
                if (combat != null && combat.IsGuarding)
                {
                    Vector2 guardCenter = combat.GetGuardCenter();
                    Vector2 guardSize = combat.GetGuardSize();
                    Rect guardRect = new Rect(guardCenter - guardSize * 0.5f, guardSize);

                    if (guardRect.Contains((Vector2)transform.position))
                    {
                        if (combat.TryParry())
                        {
                            CameraShake.Instance?.ShakeOnParry();
                            Reflect();
                            return;
                        }
                        else
                        {
                            Destroy(gameObject);
                            return;
                        }
                    }
                    else
                    {
                        player.Die();
                        CameraShake.Instance?.ShakeOnHit();
                        Destroy(gameObject);
                        return;
                    }
                }
                else
                {
                    player.Die();
                    CameraShake.Instance?.ShakeOnHit();
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            var enemy = other.GetComponent<EnemyShooter>();
            if (enemy != null)
                enemy.Die();
            Destroy(gameObject);
        }
    }

    private void Reflect()
    {
        reflected = true;
        direction = -direction;
        speed *= 1.5f;
        rb.linearVelocity = direction * speed;
        gameObject.layer = LayerMask.NameToLayer("ReflectedProjectile");
        spriteRenderer.color = new Color(1f, 0.84f, 0f);
    }
}
