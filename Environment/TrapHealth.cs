using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;

/// <summary>
/// Health for a trap: forwards damage to base, notifies <see cref="TrapZone"/> on hit and on death. Add with <see cref="DownStrikeResponse"/> for bounce on down strike.
/// Reference to <see cref="TrapZone"/> is set manually or looked up on the parent. On death calls <see cref="TrapZone.OnBroken"/>.
/// </summary>
[AddComponentMenu("Gameplay/Environment/Trap Health")]
public class TrapHealth : Health
{
    [Tooltip("Trap to \"break\" on death. If not set, looked up on this object or parent.")]
    [SerializeField] private TrapZone trapZone;

    private void Awake()
    {
        if (trapZone == null)
            trapZone = GetComponent<TrapZone>() ?? GetComponentInParent<TrapZone>();
    }

    /// <summary>Applies damage and notifies TrapZone so it can trigger on hit if needed.</summary>
    public override void Damage(
        float damage,
        GameObject instigator,
        float flickerDuration,
        float invincibilityDuration,
        Vector3 damageDirection,
        List<TypedDamage> typedDamages = null)
    {
        base.Damage(damage, instigator, flickerDuration, invincibilityDuration, damageDirection, typedDamages);
        trapZone?.NotifyHit();
    }

    /// <summary>Called on death (0 HP). Notifies the trap that it is broken.</summary>
    public override void Kill()
    {
        if (trapZone != null)
            trapZone.OnBroken();
        base.Kill();
    }
}
