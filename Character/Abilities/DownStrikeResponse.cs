using UnityEngine;

/// <summary>
/// Response to a <strong>downward</strong> strike (strike from above).
/// Attach to objects that can be hit by down strike; defines the bounce force applied to the striker.
/// For side/forward strikes use other response components (e.g. forward strike response).
/// If absent, the ability's default BounceForce is used.
/// </summary>
public class DownStrikeResponse : MonoBehaviour
{
    /// <summary>
    /// Upward force applied to the character that struck this object from above. 0 = no bounce.
    /// If the hit object has no DownStrikeResponse, the ability's default BounceForce is used.
    /// </summary>
    [Tooltip("Upward force applied to the character that struck this object from above. 0 = no bounce. If no DownStrikeResponse on object, ability's default is used.")]
    public float BounceForce = 12f;
}
