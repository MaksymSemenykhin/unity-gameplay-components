using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;

/// <summary>
/// Inherits <see cref="SurfaceModifier"/> to add restrictions for characters standing on this surface,
/// e.g. disable jumping. Friction and force from the base class still apply.
/// Add to a platform (Collider2D, trigger or solid); when a character is on this surface, restrictions are applied and restored on exit.
/// </summary>
[AddComponentMenu("Gameplay/Surface Modifier Restrictions")]
public class SurfaceModifierRestrictions : SurfaceModifier
{
    [Header("Restrictions")]
    [Tooltip("If false, characters on this surface cannot jump (NumberOfJumps set to 0). Restored on exit.")]
    [SerializeField] private bool allowJump = false;

    private readonly Dictionary<Character, int> _savedNumberOfJumps = new Dictionary<Character, int>();

    /// <summary>Applies base surface logic, then enforces jump restriction (NumberOfJumps = 0) when allowJump is false.</summary>
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

                if (!_savedNumberOfJumps.ContainsKey(target.TargetCharacter))
                    _savedNumberOfJumps[target.TargetCharacter] = jump.NumberOfJumps;
                jump.NumberOfJumps = 0;
            }
        }
    }

    /// <summary>Restores jump setting for the character that left the surface, then calls base.</summary>
    protected override void OnTriggerExit2D(Collider2D collider)
    {
        RestoreJumpForCollider(collider);
        base.OnTriggerExit2D(collider);
    }

    /// <summary>Finds character from collider and restores its NumberOfJumps from saved value.</summary>
    private void RestoreJumpForCollider(Collider2D collider)
    {
        var controller = collider.GetComponent<CorgiController>() ?? collider.GetComponentInParent<CorgiController>();
        if (controller == null) return;
        var character = controller.GetComponent<Character>();
        if (character == null || !_savedNumberOfJumps.TryGetValue(character, out int saved)) return;

        var jump = character.GetComponent<CharacterJump>();
        if (jump != null) jump.NumberOfJumps = saved;
        _savedNumberOfJumps.Remove(character);
    }

    /// <summary>Restores all saved jump settings when the component is disabled.</summary>
    private void OnDisable()
    {
        foreach (var kv in _savedNumberOfJumps)
        {
            if (kv.Key == null) continue;
            var jump = kv.Key.GetComponent<CharacterJump>();
            if (jump != null) jump.NumberOfJumps = kv.Value;
        }
        _savedNumberOfJumps.Clear();
    }
}
