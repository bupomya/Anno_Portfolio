using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;

    public bool IsDead { get; protected set; }

    /// <summary>
    /// Returns the facing direction: 1 = right, -1 = left.
    /// </summary>
    public int FacingDirection => (spriteRenderer != null && spriteRenderer.flipX) ? -1 : 1;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public abstract void Die();
}
