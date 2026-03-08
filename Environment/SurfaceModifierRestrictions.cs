using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;

/// <summary>
/// Inherits <see cref="SurfaceModifier"/> to add restrictions for characters standing on this surface,
/// e.g. disable jumping by revoking the jump ability permission. Friction and force from the base class still apply.
/// Add to a platform (Collider2D); when a character is on this surface, restrictions are applied and restored on exit.
/// </summary>
[AddComponentMenu("Gameplay/Surface Modifier Restrictions")]
public class SurfaceModifierRestrictions : SurfaceModifier
{
    [Header("Restrictions")]
    [Tooltip("If false, jump ability is disabled (PermitAbility(false)) on this surface. Restored on exit.")]
    [SerializeField] private bool allowJump = false;

    private readonly HashSet<Character> _charactersWithJumpDisabled = new HashSet<Character>();

    /// <summary>Applies base surface logic, then disables jump ability permission when allowJump is false.</summary>
    protected override void ProcessSurface()
    {
        base.ProcessSurface();

        if (!allowJump && _targets != null)
        {
            foreach (var target in _targets)
            {
                if (target.TargetCharacter == null || !target.TargetAffectedBySurfaceModifier) continue;
                var jump = target.TargetCharacter.GetComponent<CharacterJump>();
                if (jump == null) continue;

                _charactersWithJumpDisabled.Add(target.TargetCharacter);
                jump.PermitAbility(false);
            }
        }
    }

    /// <summary>Restores jump ability permission for the character that left the surface, then calls base.</summary>
    protected override void OnTriggerExit2D(Collider2D collider)
    {
        RestoreJumpForCollider(collider);
        base.OnTriggerExit2D(collider);
    }

    /// <summary>Finds character from collider and re-enables jump ability (PermitAbility(true)) if we had disabled it.</summary>
    private void RestoreJumpForCollider(Collider2D collider)
    {
        var controller = collider.GetComponent<CorgiController>() ?? collider.GetComponentInParent<CorgiController>();
        if (controller == null) return;
        var character = controller.GetComponent<Character>();
        if (character == null || !_charactersWithJumpDisabled.Remove(character)) return;

        var jump = character.GetComponent<CharacterJump>();
        if (jump != null) jump.PermitAbility(true);
    }

    /// <summary>Re-enables jump ability for all characters we had restricted when the component is disabled.</summary>
    private void OnDisable()
    {
        foreach (var character in _charactersWithJumpDisabled)
        {
            if (character == null) continue;
            var jump = character.GetComponent<CharacterJump>();
            if (jump != null) jump.PermitAbility(true);
        }
        _charactersWithJumpDisabled.Clear();
    }
}
