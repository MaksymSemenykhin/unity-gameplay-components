using UnityEngine;

/// <summary>
/// Trap: when an object with one of the given tags enters the collider (trigger), a telegraph is spawned from a prefab.
/// Optionally the trap can be broken — specify which attack type: down strike, direct attack, or any.
/// For break-by-down-strike add <see cref="DownStrikeResponse"/> and <see cref="TrapHealth"/> on this object (or a child with a collider on the strike layer).
/// </summary>
[RequireComponent(typeof(Collider2D))]
[AddComponentMenu("Gameplay/Environment/Trap Zone")]
public class TrapZone : MonoBehaviour
{
    /// <summary>Which attack type breaks the trap (if breakable).</summary>
    public enum BreakBy
    {
        /// <summary>Does not break.</summary>
        None,
        /// <summary>Down strike only (AbilityDownStrike).</summary>
        DownStrike,
        /// <summary>Direct attack only (not down strike).</summary>
        Direct,
        /// <summary>Any damage.</summary>
        Both
    }

    [Header("Trigger")]
    [Tooltip("Tags of objects that trigger the telegraph when entering. Empty array or empty strings do not trigger.")]
    [SerializeField] private string[] triggerTags = { "Player" };
    [Tooltip("If true, spawning the telegraph is also triggered when the trap is hit (TrapHealth receives valid damage).")]
    [SerializeField] private bool triggerOnHit = false;
    [Tooltip("If true, spawning the telegraph is also triggered when the trap is destroyed (OnBroken).")]
    [SerializeField] private bool triggerOnDestroy = false;

    [Header("Telegraph")]
    [Tooltip("Prefab with TelegraphProgressController (and TelegraphProgressView if needed). Spawned when the trigger fires.")]
    [SerializeField] private GameObject telegraphPrefab;
    [Tooltip("Spawn position for the telegraph. If not set, this object's position is used.")]
    [SerializeField] private Transform telegraphSpawnPoint;
    [Tooltip("If true, call Play() after spawn (with the given durations). If false, telegraph does not start here (e.g. prefab has autoPlayOnStart).")]
    [SerializeField] private bool callPlayOnSpawn = true;
    [Tooltip("Telegraph phase duration (sec). 0 = do not override (use prefab default). Only used when callPlayOnSpawn = true.")]
    [SerializeField] private float telegraphDuration = 0f;
    [Tooltip("Active phase duration (sec). 0 = do not override. Only used when callPlayOnSpawn = true.")]
    [SerializeField] private float activeDuration = 0f;
    [Tooltip("Finish fade duration (sec). 0 = do not override. Only used when callPlayOnSpawn = true.")]
    [SerializeField] private float finishFadeDuration = 0f;

    [Header("Break (optional)")]
    [Tooltip("Whether the trap can be broken by attacks. Requires TrapHealth on this object or a child with a collider on the strike layer.")]
    [SerializeField] private bool breakable = false;
    [Tooltip("Which attack type breaks it: down strike, direct attack, or any.")]
    [SerializeField] private BreakBy breakBy = BreakBy.DownStrike;

    [Header("Behaviour")]
    [Tooltip("One-shot: after firing once, the trap does not activate again (until scene reload).")]
    [SerializeField] private bool oneShot = false;
    [Tooltip("Whether to destroy the telegraph instance when it finishes (if false, prefab may disable itself).")]
    [SerializeField] private bool destroyTelegraphOnFinish = true;

    private Collider2D _collider;
    private bool _alreadyTriggered;

    /// <summary>Current break-by attack type (for TrapHealth).</summary>
    public BreakBy BreakByMode => breakBy;

    /// <summary>Whether the trap is marked as breakable.</summary>
    public bool IsBreakable => breakable;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }

    /// <summary>Attempts to trigger once: respects oneShot, sets _alreadyTriggered and spawns the telegraph if allowed.</summary>
    private void TryTrigger()
    {
        if (oneShot && _alreadyTriggered) return;
        _alreadyTriggered = true;
        SpawnAndPlayTelegraph();
    }

    /// <summary>Called when an object enters the trigger: if tag is in triggerTags, tries to trigger (spawn telegraph).</summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!MatchesTriggerTags(other)) return;
        TryTrigger();
    }

    private bool MatchesTriggerTags(Collider2D other)
    {
        if (triggerTags == null || triggerTags.Length == 0) return false;
        string tag = other.tag;
        for (int i = 0; i < triggerTags.Length; i++)
        {
            if (!string.IsNullOrEmpty(triggerTags[i]) && other.CompareTag(triggerTags[i]))
                return true;
        }
        return false;
    }

    /// <summary>Spawns the telegraph prefab at the point; if callPlayOnSpawn calls Play(), otherwise the telegraph may start itself (autoPlay on prefab).</summary>
    public void SpawnAndPlayTelegraph()
    {
        if (telegraphPrefab == null) return;

        Vector3 position = telegraphSpawnPoint != null ? telegraphSpawnPoint.position : transform.position;
        Quaternion rotation = telegraphSpawnPoint != null ? telegraphSpawnPoint.rotation : transform.rotation;
        GameObject instance = Instantiate(telegraphPrefab, position, rotation);

        var controller = instance.GetComponent<TelegraphProgressController>();
        if (controller != null)
        {
            if (callPlayOnSpawn)
            {
                float? t = telegraphDuration > 0f ? telegraphDuration : null;
                float? a = activeDuration > 0f ? activeDuration : null;
                float? f = finishFadeDuration > 0f ? finishFadeDuration : null;
                controller.Play(t, a, f);
            }

            if (destroyTelegraphOnFinish)
            {
                controller.OnFinished += () =>
                {
                    if (instance != null) Destroy(instance);
                };
            }
        }
    }

    /// <summary>Called by TrapHealth when the trap receives valid damage. If triggerOnHit is set, tries to trigger (spawn telegraph).</summary>
    public void NotifyHit()
    {
        if (!triggerOnHit) return;
        TryTrigger();
    }

    /// <summary>Called when the trap is broken (from TrapHealth): if triggerOnDestroy, tries to trigger (spawn telegraph), then disables the trigger and component.</summary>
    public void OnBroken()
    {
        if (triggerOnDestroy)
            TryTrigger();

        if (_collider != null) _collider.enabled = false;
        enabled = false;
    }
}
