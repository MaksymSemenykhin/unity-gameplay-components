using UnityEngine;
using MoreMountains.CorgiEngine;

/// <summary>
/// One-shot damage on touch: inherits <see cref="DamageOnTouch"/>. After dealing damage to a damageable target once, destroys this GameObject.
/// Use as a drop-in replacement for DamageOnTouch when the hazard should only hit once (e.g. trap damage zone that disappears after hit).
/// </summary>
[AddComponentMenu("Gameplay/Environment/Damage On Touch One Shot")]
public class DamageOnTouchOneShot : DamageOnTouch
{
    /// <summary>Called when we hit something with Health. Applies damage then destroys this GameObject.</summary>
    protected override void OnCollideWithDamageable(Health health)
    {
        base.OnCollideWithDamageable(health);
        Destroy(gameObject);
    }
}
