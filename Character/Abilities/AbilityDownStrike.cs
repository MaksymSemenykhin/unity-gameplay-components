using UnityEngine;
using MoreMountains.CorgiEngine;

/// <summary>
/// Downward strike: in the air, strike below (Down + left click). Uses <see cref="StrikeZoneRunner"/>; bounce from <see cref="DownStrikeResponse"/> or default.
/// </summary>
[AddComponentMenu("Corgi Engine/Character/Abilities/Ability Down Strike")]
public class AbilityDownStrike : CharacterAbility
{
    [Header("Input")]
    [Tooltip("Key that must be held for Down (e.g. S or DownArrow). Strike fires on left mouse click while this is held.")]
    public KeyCode DownKey = KeyCode.S;
    [Tooltip("When using stick: PrimaryMovement.y below this counts as Down held.")]
    public float MovementDownThreshold = -0.5f;

    [Header("Hitbox zone")]
    [Tooltip("Layers that can be hit (e.g. Enemies).")]
    public LayerMask StrikeableLayers;
    [Tooltip("Prefab for the strike zone (must have BoxCollider2D). If set, zone shape/size/visuals come from the prefab; otherwise a procedural box is used with Zone Size.")]
    public GameObject ZonePrefab;
    [Tooltip("Offset from character origin where the strike zone is spawned (below the character).")]
    public Vector2 ZoneOffset = new Vector2(0f, -0.5f);
    [Tooltip("Size of the strike zone when Zone Prefab is not set. Ignored when using a prefab.")]
    public Vector2 ZoneSize = new Vector2(0.7f, 0.5f);

    [Header("Timing")]
    [Tooltip("Frames to wait after spawning the zone before applying damage (lets visuals render).")]
    public int DelayFrames = 2;

    [Header("Strike")]
    [Tooltip("Damage applied to the hit target.")]
    public float DamageAmount = 10f;
    [Tooltip("Invincibility duration (seconds) given to the target after damage.")]
    public float TargetInvincibilityDuration = 0.5f;
    [Tooltip("Default bounce when hit object has no DownStrikeResponse.")]
    public float BounceForce = 12f;
    [Tooltip("If true, set horizontal speed to 0 before applying bounce so the bounce is always the same (straight up). If false, horizontal run speed is preserved.")]
    public bool ResetHorizontalSpeedOnBounce = true;

    [Header("Cooldown")]
    [Tooltip("Minimum time between strikes.")]
    public float StrikeCooldown = 0.3f;

    private float _lastStrikeTime = -999f;
    private bool _strikeInProgress;

    /// <inheritdoc />
    public override void ProcessAbility()
    {
        base.ProcessAbility();
        if (!AbilityAuthorized) return;
        if (_controller.State.IsGrounded) return;
        if (_strikeInProgress) return;
        if (Time.time < _lastStrikeTime + StrikeCooldown) return;
        if (!DownStrikeInput()) return;

        _strikeInProgress = true;
        _lastStrikeTime = Time.time;
        Vector2 zoneCenter = (Vector2)transform.position + ZoneOffset;

        StrikeZoneRunner.Run(
            this,
            zoneCenter,
            ZoneSize,
            StrikeableLayers,
            DelayFrames,
            DamageAmount,
            TargetInvincibilityDuration,
            gameObject,
            Vector3.down,
            (go) => StrikeZoneRunner.GetBounceFromHit(go, BounceForce),
            OnStrikeResolved,
            ZonePrefab);
    }

    /// <summary>
    /// Callback when the strike zone is resolved. Applies bounce and clears strike-in-progress.
    /// </summary>
    /// <param name="anyHit">True if at least one collider was in the zone.</param>
    /// <param name="bounceForce">Effect from last hit (DownStrikeResponse or default).</param>
    private void OnStrikeResolved(bool anyHit, float bounceForce)
    {
        if (anyHit && bounceForce > 0f)
        {
            if (ResetHorizontalSpeedOnBounce)
                _controller.SetHorizontalForce(0f);
            _controller.SetVerticalForce(bounceForce);
        }
        _strikeInProgress = false;
    }

    /// <summary>
    /// True when the player just pressed left mouse button while holding Down (key or stick).
    /// </summary>
    private bool DownStrikeInput()
    {
        if (!Input.GetMouseButtonDown(0)) return false;

        bool downHeld = (DownKey != KeyCode.None && Input.GetKey(DownKey))
                       || _inputManager.PrimaryMovement.y < MovementDownThreshold;
        return downHeld;
    }

    /// <summary>
    /// Draws the strike zone in the editor when this component is selected.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector2 center = (Vector2)transform.position + ZoneOffset;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Vector2 size = ZonePrefab != null && ZonePrefab.TryGetComponent<BoxCollider2D>(out var box) ? box.size : ZoneSize;
        Gizmos.DrawWireCube(center, size);
    }
}
