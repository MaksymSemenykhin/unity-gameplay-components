using UnityEngine;
using MoreMountains.CorgiEngine;

/// <summary>
/// Inherits CharacterJump. When jumping, calls ResetHorizontalSpeed so CharacterHorizontalMovementRunJump
/// applies JumpSpeed cleanly each frame.
/// </summary>
[AddComponentMenu("Corgi Engine/Character/Abilities/Character Jump Run Speed")]
public class CharacterJumpRunSpeed : CharacterJump
{
    protected override void Initialization()
    {
        base.Initialization();
        _characterHorizontalMovement = _character?.FindAbility<CharacterHorizontalMovementRunJump>();

    }

    public override void ProcessAbility()
    {
        base.ProcessAbility();
        if (_controller != null && _controller.State.IsJumping && _characterHorizontalMovement != null)
            _characterHorizontalMovement.ResetHorizontalSpeed();
    }
}
