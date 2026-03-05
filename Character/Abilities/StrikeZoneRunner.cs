using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;

/// <summary>
/// Shared strike logic: spawn zone, wait N frames, resolve hits (effect per object via getEffectFromHit, damage where Health present), then callback.
/// Use <see cref="GetBounceFromHit"/> for down-strike bounce; other strikes pass their own delegate.
/// </summary>
public static class StrikeZoneRunner
{
    /// <summary>
    /// Runs the strike routine: spawn zone, wait delayFrames, resolve hits (effect per object, damage where Health), then onResolved(anyHit, effect from last hit).
    /// </summary>
    /// <param name="runner">MonoBehaviour that starts the coroutine (typically the ability).</param>
    /// <param name="zoneCenter">World position of the zone center.</param>
    /// <param name="zoneSize">Size of the box when zonePrefab is null.</param>
    /// <param name="strikeableLayers">Layers to detect with OverlapBoxAll.</param>
    /// <param name="delayFrames">Frames to wait after spawning the zone before resolving hits.</param>
    /// <param name="damageAmount">Damage applied to each hit Health.</param>
    /// <param name="invincibilityDuration">Invincibility duration passed to Health.Damage.</param>
    /// <param name="instigator">GameObject that caused the strike (e.g. the character).</param>
    /// <param name="damageDirection">Direction passed to Health.Damage.</param>
    /// <param name="getEffectFromHit">Called per hit object; returns effect value (e.g. bounce). Use <see cref="GetBounceFromHit"/> for down strike.</param>
    /// <param name="onResolved">Called when done: (anyHit, effectValue from last hit).</param>
    /// <param name="zonePrefab">If set, instantiate this prefab at zoneCenter; zone must have a BoxCollider2D. If null, a procedural box is created using zoneSize.</param>
    public static void Run(
        MonoBehaviour runner,
        Vector2 zoneCenter,
        Vector2 zoneSize,
        LayerMask strikeableLayers,
        int delayFrames,
        float damageAmount,
        float invincibilityDuration,
        GameObject instigator,
        Vector3 damageDirection,
        Func<GameObject, float> getEffectFromHit,
        Action<bool, float> onResolved,
        GameObject zonePrefab = null)
    {
        if (runner == null) return;
        Transform zoneParent = instigator != null ? instigator.transform.parent : null;
        runner.StartCoroutine(Routine(zoneCenter, zoneSize, zoneParent, strikeableLayers, delayFrames, damageAmount,
            invincibilityDuration, instigator, damageDirection, getEffectFromHit, onResolved, zonePrefab));
    }

    /// <summary>
    /// Bounce force for downward strike: <see cref="DownStrikeResponse"/> on hit object or parent, otherwise defaultBounce.
    /// </summary>
    /// <param name="hitObject">GameObject that was hit (may have no Health).</param>
    /// <param name="defaultBounce">Used when hit object has no DownStrikeResponse.</param>
    /// <returns>Bounce force to apply.</returns>
    public static float GetBounceFromHit(GameObject hitObject, float defaultBounce)
    {
        var response = hitObject.GetComponent<DownStrikeResponse>() ?? hitObject.GetComponentInParent<DownStrikeResponse>();
        return response != null ? response.BounceForce : defaultBounce;
    }

    /// <summary>
    /// Coroutine: spawn zone, wait delayFrames, resolve hits (effect from each, damage where Health present), invoke onResolved.
    /// </summary>
    private static IEnumerator Routine(
        Vector2 zoneCenter,
        Vector2 zoneSize,
        Transform zoneParent,
        LayerMask strikeableLayers,
        int delayFrames,
        float damageAmount,
        float invincibilityDuration,
        GameObject instigator,
        Vector3 damageDirection,
        Func<GameObject, float> getEffectFromHit,
        Action<bool, float> onResolved,
        GameObject zonePrefab)
    {
        GameObject zone = zonePrefab != null
            ? UnityEngine.Object.Instantiate(zonePrefab, zoneCenter, Quaternion.identity, zoneParent)
            : CreateZone(zoneCenter, zoneSize, zoneParent);

        for (int i = 0; i < delayFrames; i++)
            yield return null;

        Vector2 checkCenter = zoneCenter;
        Vector2 checkSize = zoneSize;
        float checkAngle = 0f;
        if (zone != null)
        {
            var box = zone.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                checkCenter = box.bounds.center;
                checkSize = box.bounds.size;
                checkAngle = zone.transform.eulerAngles.z;
            }

            UnityEngine.Object.Destroy(zone);
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(checkCenter, checkSize, checkAngle, strikeableLayers);

        bool anyHit = false;
        float effect = 0f;
        var damaged = new HashSet<Health>();
        foreach (Collider2D col in hits)
        {
            if (col == null) continue;
            anyHit = true;
            if (getEffectFromHit != null)
                effect = getEffectFromHit(col.gameObject);
            Health health = col.GetComponent<Health>() ?? col.GetComponentInParent<Health>();
            if (health == null || !damaged.Add(health) || !health.CanTakeDamageThisFrame()) continue;
            health.Damage(damageAmount, instigator, invincibilityDuration, invincibilityDuration, damageDirection);
        }

        onResolved(anyHit, effect);
    }

    /// <summary>
    /// Creates a procedural strike zone GameObject with BoxCollider2D (trigger) at the given center and size.
    /// </summary>
    /// <param name="center">World position of the zone.</param>
    /// <param name="size">Size of the box collider.</param>
    /// <param name="parent">Parent transform; can be null.</param>
    /// <returns>The created zone GameObject.</returns>
    private static GameObject CreateZone(Vector2 center, Vector2 size, Transform parent)
    {
        var go = new GameObject("StrikeZone");
        go.transform.position = center;
        if (parent != null) go.transform.SetParent(parent);
        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = size;
        box.enabled = true;
        return go;
    }
}