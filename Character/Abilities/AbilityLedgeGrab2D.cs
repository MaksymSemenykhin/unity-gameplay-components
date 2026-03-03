using System.Collections;
using UnityEngine;
using MoreMountains.CorgiEngine;

[AddComponentMenu("Corgi Engine/Character/Abilities/Ability Ledge Grab 2D")]
public class AbilityLedgeGrab2D : CharacterAbility
{
    [Header("Detection")] public LayerMask SolidLayers;
    public float WallCheckDistance = 0.25f;
    public float HeadClearanceHeight = 1.2f;
    public float TopDownCheckDistance = 2.4f;

    [Header("Raycast Offsets")] public Vector2 WallRayOriginOffset = new Vector2(0.2f, 0.2f);
    public Vector2 TopRayOriginOffset = new Vector2(0.2f, 1f);

    [Header("Hang")] public Vector2 HangOffset = new Vector2(-0.12f, -0.55f);

    [Header("Climb")] public float ClimbDuration = 0.12f;
    public Vector2 ClimbUpOffset = new Vector2(0f, 0.9f);

    [Header("Options")] 
    public bool OnlyWhenFalling = true;
    public float FallingVelocityThreshold = -0.05f;
    public float GrabCooldown = 0.8f;
    [Tooltip("Don't grab if we've been in the air less than this (stops grab when jumping from platform corner).")]
    public float MinAirTimeBeforeGrab = 0.2f;
    [Tooltip("Don't grab the same object we were standing on (e.g. corner of current platform).")]
    public bool IgnoreWallIfSameAsStandingOn = true;
    public KeyCode ClimbKey = KeyCode.W;
    public KeyCode ClimbUpKey = KeyCode.UpArrow;

    protected bool _isHanging;
    protected bool _isClimbing;
    protected float _nextGrabTime;
    protected Vector2 _lockedHangPosition;

    protected int _facingSign => _character.IsFacingRight ? 1 : -1;

    public override void ProcessAbility()
    {
        base.ProcessAbility();

        if (!AbilityAuthorized)
            return;

        if (_isClimbing)
            return;

        if (_isHanging)
        {
            HandleHang();
            return;
        }

        if (OnlyWhenFalling &&
            _controller.Speed.y > FallingVelocityThreshold)
            return;

        TryGrabLedge();
    }

    private void LateUpdate()
    {
        if (_isHanging && !_isClimbing)
        {
            float horizontal = _inputManager.PrimaryMovement.x;
            if (horizontal * _facingSign < -0.5f)
            {
                ExitHang();
            }
        }
    }

    protected virtual void TryGrabLedge()
    {
        if (Time.time < _nextGrabTime)
            return;

        if (_controller.TimeAirborne < MinAirTimeBeforeGrab)
            return;

        Vector2 wallOrigin = (Vector2)transform.position +
                             new Vector2(WallRayOriginOffset.x * _facingSign, WallRayOriginOffset.y);

        RaycastHit2D wallHit = Physics2D.Raycast(
            wallOrigin,
            Vector2.right * _facingSign,
            WallCheckDistance,
            SolidLayers
        );

        Debug.DrawRay(wallOrigin, Vector2.right * _facingSign * WallCheckDistance, Color.cyan);

        if (!wallHit)
            return;

        if (IgnoreWallIfSameAsStandingOn && _controller.StandingOnLastFrame != null &&
            wallHit.collider != null && wallHit.collider.gameObject == _controller.StandingOnLastFrame)
            return;

        Vector2 headOrigin = wallHit.point + Vector2.up * HeadClearanceHeight;
        RaycastHit2D headHit = Physics2D.Raycast(
            headOrigin,
            Vector2.right * _facingSign,
            WallCheckDistance,
            SolidLayers
        );

        if (headHit)
        {
            _nextGrabTime = Time.time + GrabCooldown;
            return;
        }

        Vector2 topOrigin = (Vector2)transform.position +
                            new Vector2(TopRayOriginOffset.x * _facingSign, TopRayOriginOffset.y);
        topOrigin.x = wallHit.point.x + 0.05f * _facingSign;

        RaycastHit2D topHit = Physics2D.Raycast(
            topOrigin,
            Vector2.down,
            TopDownCheckDistance,
            SolidLayers
        );

        Debug.DrawRay(topOrigin, Vector2.down * TopDownCheckDistance, Color.red);

        if (!topHit)
            return;

        if (topHit.point.y < transform.position.y + 0.1f)
            return;
        if (topHit.point.y - transform.position.y > 0.45f)
            return;

        Vector2 hangPos = new Vector2(
            wallHit.point.x + HangOffset.x * _facingSign,
            topHit.point.y + HangOffset.y
        );

        EnterHang(hangPos);
    }

    protected virtual void EnterHang(Vector2 hangPosition)
    {
        _isHanging = true;
        _lockedHangPosition = hangPosition;

        _controller.SetHorizontalForce(0);
        _controller.SetVerticalForce(0);
        _controller.GravityActive(false);

        _movement.ChangeState(CharacterStates.MovementStates.Idle);

        transform.position = hangPosition;
    }

    protected virtual void HandleHang()
    {
        if (_inputManager == null)
        {
            ExitHang();
            return;
        }

        transform.position = _lockedHangPosition;
        _controller.SetVerticalForce(0);

        float vertical = _inputManager.PrimaryMovement.y;

        if (vertical < -0.5f || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            ExitHang();
            return;
        }

        if (!_isClimbing && (Input.GetKeyDown(ClimbKey) || Input.GetKeyDown(ClimbUpKey) || vertical > 0.5f))
        {
            StartCoroutine(ClimbRoutine());
        }
    }

    protected virtual IEnumerator ClimbRoutine()
    {
        _isClimbing = true;

        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)ClimbUpOffset;

        float t = 0f;
        while (t < ClimbDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t / ClimbDuration);
            yield return null;
        }

        transform.position = end;

        ExitHang();
        _isClimbing = false;
    }

    protected virtual void ExitHang()
    {
        _isHanging = false;
        _nextGrabTime = Time.time + GrabCooldown;

        _controller.GravityActive(true);
    }
}
