using UnityEngine;
using MoreMountains.CorgiEngine;

/// <summary>
/// Downward strike ability: in the air, the player triggers a strike below (key or stick down).
/// On hit: applies damage to the target and bounces the character up. Requires a Health on the target.
/// </summary>
[AddComponentMenu("Corgi Engine/Character/Abilities/Ability Down Strike")]
public class AbilityDownStrike : CharacterAbility
{
    [Header("Input")]
    [Tooltip("Key to trigger the downward strike (e.g. DownArrow or S).")]
    public KeyCode DownStrikeKey = KeyCode.S;
    [Tooltip("Also trigger when movement stick is pushed down (PrimaryMovement.y below this). One trigger per press.")]
    public float MovementDownThreshold = -0.5f;

    [Header("Detection")]
    [Tooltip("Layers that can be hit (e.g. Enemies).")]
    public LayerMask StrikeableLayers;
    [Tooltip("Offset from character origin to the strike check (below the character).")]
    public Vector2 CheckOffset = new Vector2(0f, -0.5f);
    [Tooltip("Radius of the overlap circle below.")]
    public float CheckRadius = 0.35f;

    [Header("Strike")]
    [Tooltip("Damage applied to the hit target.")]
    public float DamageAmount = 10f;
    [Tooltip("Invincibility duration (seconds) given to the target after damage.")]
    public float TargetInvincibilityDuration = 0.5f;
    [Tooltip("Upward force applied to the character on successful hit.")]
    public float BounceForce = 12f;

    [Header("Cooldown")]
    [Tooltip("Minimum time between strikes.")]
    public float StrikeCooldown = 0.3f;

    private float _lastStrikeTime = -999f;
    private bool _wasMovementDown;

    public override void ProcessAbility()
    {
        base.ProcessAbility();
        if (!AbilityAuthorized) return;
        if (_controller.State.IsGrounded) return;
        if (Time.time < _lastStrikeTime + StrikeCooldown) return;
        if (!DownStrikeInput()) return;

        Vector2 center = (Vector2)transform.position + CheckOffset;
        Collider2D hit = Physics2D.OverlapCircle(center, CheckRadius, StrikeableLayers);
        if (hit == null) return;

        Health health = hit.GetComponent<Health>();
        if (health == null) health = hit.GetComponentInParent<Health>();
        if (health == null) return;
        if (!health.CanTakeDamageThisFrame) return;

        health.Damage(DamageAmount, gameObject, TargetInvincibilityDuration, TargetInvincibilityDuration, Vector3.up);
        _controller.SetVerticalForce(BounceForce);
        _lastStrikeTime = Time.time;
    }

    private bool DownStrikeInput()
    {
        if (DownStrikeKey != KeyCode.None && Input.GetKeyDown(DownStrikeKey))
            return true;
        bool isDown = _inputManager.PrimaryMovement.y < MovementDownThreshold;
        if (isDown && !_wasMovementDown)
        {
            _wasMovementDown = true;
            return true;
        }
        if (!isDown) _wasMovementDown = false;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 center = (Vector2)transform.position + CheckOffset;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(center, CheckRadius);
    }
}
