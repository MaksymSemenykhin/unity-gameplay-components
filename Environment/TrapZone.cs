using UnityEngine;

/// <summary>
/// Trap: when an object with one of the given tags enters the collider (trigger), a telegraph is spawned from a prefab.
/// Optionally the trap can be broken by any damage — add <see cref="TrapHealth"/> (and <see cref="DownStrikeResponse"/> for bounce on down strike) on this object or a child with a collider on the strike layer.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[AddComponentMenu("Gameplay/Environment/Trap Zone")]
public class TrapZone : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("Tags of objects that trigger the telegraph when entering. Empty array or empty strings do not trigger.")]
    [SerializeField] private string[] triggerTags = { "Player" };
    [Tooltip("If true, spawning the telegraph is also triggered when the trap is hit (TrapHealth receives damage).")]
    [SerializeField] private bool triggerOnHit = false;
    [Tooltip("If true, spawning the telegraph is also triggered when the trap is destroyed (OnBroken).")]
    [SerializeField] private bool triggerOnDestroy = false;

    [Header("Telegraph")]
    [Tooltip("Prefab with TelegraphProgressController (and TelegraphProgressView if needed). Spawned when the trigger fires; the prefab decides how to start (e.g. autoPlayOnStart).")]
    [SerializeField] private GameObject telegraphPrefab;
    [Tooltip("Spawn position for the telegraph. If not set, this object's position is used.")]
    [SerializeField] private Transform telegraphSpawnPoint;

    [Header("Damage")]
    [Tooltip("Optional. Prefab spawned when the telegraph finishes (e.g. damage zone, hazard). Uses the same position and rotation as the telegraph.")]
    [SerializeField] private GameObject damagePrefab;

    [Header("Behaviour")]
    [Tooltip("One-shot: after firing once, the trap does not activate again (until scene reload).")]
    [SerializeField] private bool oneShot = false;
    [Tooltip("Whether to destroy the telegraph instance when it finishes (if false, prefab may disable itself).")]
    [SerializeField] private bool destroyTelegraphOnFinish = true;

    private Collider2D _collider;
    private bool _alreadyTriggered;

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
        SpawnTelegraph();
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
        foreach (var t in triggerTags)
        {
            if (!string.IsNullOrEmpty(t) && other.CompareTag(t))
                return true;
        }
        return false;
    }

    /// <summary>Spawns the telegraph prefab; on OnTelegraphComplete spawns damagePrefab, on OnFinished destroys the damage instance and optionally the telegraph.</summary>
    public void SpawnTelegraph()
    {
        if (telegraphPrefab == null) return;

        Vector3 position = telegraphSpawnPoint != null ? telegraphSpawnPoint.position : transform.position;
        Quaternion rotation = telegraphSpawnPoint != null ? telegraphSpawnPoint.rotation : transform.rotation;
        GameObject instance = Instantiate(telegraphPrefab, position, rotation);
        GameObject damageInstance = null;

        var controller = instance.GetComponent<TelegraphProgressController>();
        if (controller != null)
        {
            controller.OnTelegraphComplete += () =>
            {
                if (damagePrefab != null)
                    damageInstance = Instantiate(damagePrefab, position, rotation);
            };

            controller.OnFinished += () =>
            {
                if (damageInstance != null)
                {
                    Destroy(damageInstance);
                    damageInstance = null;
                }
                if (destroyTelegraphOnFinish && instance != null)
                    Destroy(instance);
            };
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
