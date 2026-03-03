using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;

/// <summary>
/// Downward strike ability: in the air, the player triggers a strike below (key or stick down).
/// Spawns a real hitbox zone in the scene, waits a few frames so visuals can render, then
/// applies damage to all targets in the zone (each Health once) and applies bounce once to avoid duplicating the effect.
/// </summary>
[AddComponentMenu("Corgi Engine/Character/Abilities/Ability Down Strike")]
public class AbilityDownStrike : CharacterAbility
{
    [Header("Input")]
    [Tooltip("Key to trigger the downward strike (e.g. DownArrow or S).")]
    public KeyCode DownStrikeKey = KeyCode.S;
    [Tooltip("Also trigger when movement stick is pushed down (PrimaryMovement.y below this). One trigger per press.")]
    public float MovementDownThreshold = -0.5f;

    [Header("Hitbox zone")]
    [Tooltip("Layers that can be hit (e.g. Enemies).")]
    public LayerMask StrikeableLayers;
    [Tooltip("Offset from character origin where the strike zone is spawned (below the character).")]
    public Vector2 ZoneOffset = new Vector2(0f, -0.5f);
    [Tooltip("Size of the strike zone (hitbox). You can change this to shape the hitbox.")]
    public Vector2 ZoneSize = new Vector2(0.7f, 0.5f);

    [Header("Timing")]
    [Tooltip("Frames to wait after spawning the zone before applying damage (lets visuals render).")]
    public int DelayFrames = 2;

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
    private bool _strikeInProgress;

    public override void ProcessAbility()
    {
        base.ProcessAbility();
        if (!AbilityAuthorized) return;
        if (_controller.State.IsGrounded) return;
        if (_strikeInProgress) return;
        if (Time.time < _lastStrikeTime + StrikeCooldown) return;
        if (!DownStrikeInput()) return;

        StartCoroutine(StrikeRoutine());
    }

    /// <summary>
    /// Spawns hitbox zone, waits DelayFrames so it can render, then resolves hits and applies damage + single bounce.
    /// </summary>
    private IEnumerator StrikeRoutine()
    {
        _strikeInProgress = true;
        _lastStrikeTime = Time.time;

        Vector2 zoneCenter = (Vector2)transform.position + ZoneOffset;
        GameObject zone = CreateStrikeZone(zoneCenter);

        for (int i = 0; i < DelayFrames; i++)
            yield return null;

        Collider2D[] hits = Physics2D.OverlapBoxAll(zoneCenter, ZoneSize, 0f, StrikeableLayers);
        if (zone != null) Destroy(zone);

        bool anyHit = false;
        var damaged = new HashSet<Health>();
        foreach (Collider2D col in hits)
        {
            if (col == null) continue;
            Health health = col.GetComponent<Health>();
            if (health == null) health = col.GetComponentInParent<Health>();
            if (health == null || !damaged.Add(health) || !health.CanTakeDamageThisFrame) continue;
            health.Damage(DamageAmount, gameObject, TargetInvincibilityDuration, TargetInvincibilityDuration, Vector3.up);
            anyHit = true;
        }

        if (anyHit)
            _controller.SetVerticalForce(BounceForce);

        _strikeInProgress = false;
    }

    /// <summary>
    /// Creates a temporary GameObject with a trigger BoxCollider2D so the hitbox exists in the scene and can be shaped via ZoneSize.
    /// </summary>
    private GameObject CreateStrikeZone(Vector2 center)
    {
        var go = new GameObject("DownStrikeZone");
        go.transform.position = center;
        go.transform.SetParent(transform.parent);
        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = ZoneSize;
        box.enabled = true;
        return go;
    }

    /// <summary>
    /// True when the player just pressed the strike key or pushed the stick down (edge trigger for stick so it doesn't repeat every frame).
    /// </summary>
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

    /// <summary>
    /// Draws the strike zone in the Scene view when the component is selected.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector2 center = (Vector2)transform.position + ZoneOffset;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireCube(center, ZoneSize);
    }
}
