using UnityEngine;
using MoreMountains.CorgiEngine;

/// <summary>
/// Prevents the character from getting an unrealistic upward speed when transitioning
/// from a slope onto a one-way platform (fixes "flying into space" at slope/platform junction).
/// Add this to your Character if the hysteresis on AnyHeightOneWayPlatform is not enough.
/// </summary>
[RequireComponent(typeof(CorgiController))]
public class OneWayPlatformLandingFix : MonoBehaviour
{
    [Tooltip("Only clamp when upward speed exceeds this (spike from slope/one-way). Set above your jump force so normal jump works (e.g. 22).")]
    [SerializeField] private float spikeThreshold = 22f;
    [Tooltip("When a spike is detected, clamp speed to this (small value so character doesn't fly).")]
    [SerializeField] private float clampTo = 5f;

    private CorgiController _controller;

    private void Awake()
    {
        _controller = GetComponent<CorgiController>();
    }

    private void LateUpdate()
    {
        if (_controller == null) return;
        if (!_controller.State.IsGrounded) return;

        Vector2 speed = _controller.Speed;
        if (speed.y > spikeThreshold)
        {
            _controller.SetVerticalForce(clampTo);
        }
    }
}
