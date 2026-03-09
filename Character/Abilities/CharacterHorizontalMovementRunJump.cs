using UnityEngine;
using MoreMountains.CorgiEngine;

/// <summary>
/// Inherits CharacterHorizontalMovement. When jumping, MovementSpeed = RunSpeed (from Run ability) —
/// jump from stand = same as from run. Use with CharacterJumpRunSpeed.
/// </summary>
[AddComponentMenu("Corgi Engine/Character/Abilities/Character Horizontal Movement Run Jump")]
public class CharacterHorizontalMovementRunJump : CharacterHorizontalMovement
{
    private float _runSpeed;
    private CharacterRun _characterRun;

    protected override void Initialization()
    {
        base.Initialization();
        _characterRun = _character?.FindAbility<CharacterRun>();
    }

    protected override void HandleHorizontalMovement()
    {
        var state = _movement.CurrentState;
        if (state == CharacterStates.MovementStates.Jumping
            || state == CharacterStates.MovementStates.DoubleJumping
            || state == CharacterStates.MovementStates.WallJumping)
        {
            if (_characterRun != null)
            {
                _runSpeed = _characterRun.RunSpeed;
                MovementSpeed = _runSpeed;
            }
        }
        base.HandleHorizontalMovement();
    }
}
