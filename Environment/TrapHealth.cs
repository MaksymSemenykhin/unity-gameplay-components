using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;

/// <summary>
/// Trap health with attack-type filter. Add to an object with a collider on the strike layer together with <see cref="DownStrikeResponse"/> for down strike.
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

    /// <summary>
    /// Accepts damage only if the attack type matches the trap setting (DownStrike / Direct / Both).
    /// Down strike is detected by damageDirection (downward) and AbilityDownStrike on instigator.
    /// </summary>
    public override void Damage(
        float damage,
        GameObject instigator,
        float flickerDuration,
        float invincibilityDuration,
        Vector3 damageDirection,
        List<TypedDamage> typedDamages = null)
    {
        if (trapZone == null || !trapZone.IsBreakable)
        {
            base.Damage(damage, instigator, flickerDuration, invincibilityDuration, damageDirection, typedDamages);
            return;
        }

        TrapZone.BreakBy breakBy = trapZone.BreakByMode;
        if (breakBy == TrapZone.BreakBy.None)
            return;

        bool isDownStrike = IsDownStrike(instigator, damageDirection);

        bool accept = breakBy switch
        {
            TrapZone.BreakBy.Both => true,
            TrapZone.BreakBy.DownStrike => isDownStrike,
            TrapZone.BreakBy.Direct => !isDownStrike,
            _ => false
        };

        if (!accept) return;

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

    private static bool IsDownStrike(GameObject instigator, Vector3 damageDirection)
    {
        if (instigator == null) return false;
        bool directionDown = damageDirection.y < -0.3f;
        if (!directionDown) return false;
        var downStrike = instigator.GetComponent<AbilityDownStrike>() ?? instigator.GetComponentInChildren<AbilityDownStrike>();
        return downStrike != null;
    }
}
