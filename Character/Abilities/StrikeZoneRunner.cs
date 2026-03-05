using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;

/// <summary>
/// Shared logic for strike abilities: spawn a hitbox zone, wait N frames, resolve all hits
/// (damage each Health once), then invoke a callback so the ability can apply effect once (e.g. bounce).
/// Reuse for down strike, forward strike, upward strike, etc.
/// </summary>
public static class StrikeZoneRunner
{
    /// <summary>
    /// Runs the strike routine: spawns a zone (from prefab or procedural box), waits delayFrames,
    /// applies damage to all unique Health in the zone (StrikeableLayers), then calls onResolved(anyHit).
    /// </summary>
    /// <param name="zonePrefab">If set, instantiate this prefab at zoneCenter; zone must have a BoxCollider2D (size/position from it). If null, a procedural box is created using zoneSize.</param>
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
        Action<bool> onResolved,
        GameObject zonePrefab = null)
    {
        if (runner == null) return;
        Debug.Log("Running StrikeZone");
        Transform zoneParent = instigator != null ? instigator.transform.parent : null;
        runner.StartCoroutine(Routine(zoneCenter, zoneSize, zoneParent, strikeableLayers, delayFrames, damageAmount,
            invincibilityDuration, instigator, damageDirection, onResolved, zonePrefab));
    }

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
        Action<bool> onResolved,
        GameObject zonePrefab)
    {
        GameObject zone = zonePrefab != null
            ? UnityEngine.Object.Instantiate(zonePrefab, zoneCenter, Quaternion.identity, zoneParent)
            : CreateZone(zoneCenter, zoneSize, zoneParent);
        Debug.Log("Running Routine 1");

        for (int i = 0; i < delayFrames; i++)
            yield return null;
        Debug.Log("Running Routine 2");


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
        var damaged = new HashSet<Health>();
        foreach (Collider2D col in hits)
        {
            if (col == null) continue;
            Health health = col.GetComponent<Health>();
            if (health == null) health = col.GetComponentInParent<Health>();
            if (health == null || !damaged.Add(health) || !health.CanTakeDamageThisFrame()) continue;
            health.Damage(damageAmount, instigator, invincibilityDuration, invincibilityDuration, damageDirection);
            anyHit = true;
        }

        onResolved(anyHit);
    }

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