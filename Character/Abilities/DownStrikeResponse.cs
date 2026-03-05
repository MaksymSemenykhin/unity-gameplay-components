using UnityEngine;

/// <summary>
/// Response to a downward strike (from above). Attach to objects in the strike zone; defines bounce force.
/// Object may have no Health (e.g. bouncy platform). For side/forward strikes use other response components.
/// </summary>
public class DownStrikeResponse : MonoBehaviour
{
    /// <summary>Upward force applied to the striker. 0 = no bounce.</summary>
    [Tooltip("Upward force applied to the character that struck from above. 0 = no bounce.")]
    public float BounceForce = 12f;
}
