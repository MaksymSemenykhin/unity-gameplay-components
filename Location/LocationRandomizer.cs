using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

/// <summary>
/// At level load, randomly spawns loot caves (or other sub-locations) from prefabs
/// outside the main map. Each prefab has its own spawn chance (0–1).
/// Optionally spreads spawn over frames to avoid a single-frame spike; broadcasts
/// LocationsSpawned when done so GameController can wait.
/// </summary>
public class LocationRandomizer : MonoBehaviour, MMEventListener<CorgiEngineEvent>
{
    /// <summary> Event name broadcast when all caves have been spawned. Listen via MMEventListener&lt;MMGameEvent&gt; and check EventName == LocationsSpawnedEventName. </summary>
    public const string LocationsSpawnedEventName = "LocationsSpawned";

    public enum RunMode
    {
        [Tooltip("Run in Awake(). Use when order doesn't matter.")]
        OnAwake,
        [Tooltip("Run when Corgi broadcasts LevelStart (after LevelManager init).")]
        OnCorgiLevelStart,
    }

    [System.Serializable]
    public class CaveEntry
    {
        [Tooltip("Prefab of the cave/room to spawn.")]
        public GameObject prefab;
        [Tooltip("Chance this prefab will appear (0 = never, 1 = always).")]
        [Range(0f, 1f)]
        public float spawnChance = 0.5f;
    }

    [Header("Caves (prefab + chance per prefab)")]
    [Tooltip("If set, use entries from this asset (reuse across scenes). Otherwise use the list below.")]
    [SerializeField] private LocationRandomizerConfig config;
    [SerializeField] private List<CaveEntry> caveEntries = new List<CaveEntry>();

    [Header("Spawn area (outside main map)")]
    [Tooltip("World position of first cave; subsequent caves are offset by Spawn Step.")]
    [SerializeField] private Vector2 firstSpawnPosition = new Vector2(100f, 0f);
    [Tooltip("Offset for each next spawned cave (e.g. 50,0 to place them in a row).")]
    [SerializeField] private Vector2 spawnStepPerCave = new Vector2(50f, 0f);

    [Header("When to run")]
    [Tooltip("OnAwake = run in Awake. OnCorgiLevelStart = run when Corgi fires LevelStart.")]
    [SerializeField] private RunMode runMode = RunMode.OnAwake;

    [Header("Performance & waiting")]
    [Tooltip("If true, spawn one cave per frame to avoid a single heavy frame. Disable if you have few/simple prefabs.")]
    [SerializeField] private bool spreadOverFrames;
    [Tooltip("When spreadOverFrames is true, spawn this many caves per frame (1 = smoothest, higher = faster finish).")]
    [SerializeField] private int cavesPerFrame = 1;

    [Header("Debug")]
    [Tooltip("Log spawn count and elapsed time (seconds + ms) when randomization finishes. Uses real time (unaffected by time scale).")]
    [SerializeField] private bool logLoadTime;

    private int _spawnIndex;

    private void Awake()
    {
        if (runMode == RunMode.OnAwake)
            RunRandomization();
    }

    private void OnEnable()
    {
        if (runMode == RunMode.OnCorgiLevelStart)
            this.MMEventStartListening<CorgiEngineEvent>();
    }

    private void OnDisable()
    {
        if (runMode == RunMode.OnCorgiLevelStart)
            this.MMEventStopListening<CorgiEngineEvent>();
    }

    public void OnMMEvent(CorgiEngineEvent engineEvent)
    {
        if (runMode == RunMode.OnCorgiLevelStart && engineEvent.EventType == CorgiEngineEventTypes.LevelStart)
            RunRandomization();
    }

    /// <summary>
    /// Call manually from your code (e.g. from GameManager) if you need exact timing.
    /// When done, broadcasts MMGameEvent(LocationsSpawnedEventName) so listeners can wait.
    /// </summary>
    public void RunRandomization()
    {
        var toSpawn = CollectSpawnList();
        if (toSpawn == null)
        {
            Debug.LogWarning("[LocationRandomizer] No cave entries: assign Config or fill Cave Entries.", this);
            BroadcastLocationsSpawned();
            return;
        }
        if (toSpawn.Count == 0)
        {
            BroadcastLocationsSpawned();
            return;
        }

        float startTime = logLoadTime ? Time.realtimeSinceStartup : 0f;
        int count = toSpawn.Count;

        if (spreadOverFrames && gameObject.activeInHierarchy)
        {
            StartCoroutine(SpawnOverFrames(toSpawn, count, startTime));
        }
        else
        {
            SpawnAll(toSpawn);
            LogLoadTime(count, startTime);
            BroadcastLocationsSpawned();
        }
    }

    private List<(GameObject prefab, Vector2 position)> CollectSpawnList()
    {
        if (config != null && config.CaveEntries.Count > 0)
            return CollectFrom(config.CaveEntries);
        if (caveEntries != null && caveEntries.Count > 0)
            return CollectFromLocal();
        return null;
    }

    private List<(GameObject prefab, Vector2 position)> CollectFrom(IReadOnlyList<LocationRandomizerConfig.CaveEntry> entries)
    {
        var list = new List<(GameObject prefab, Vector2 position)>();
        var seen = new HashSet<GameObject>();
        int index = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry.prefab == null) continue;
            if (Random.value > entry.spawnChance) continue;
            if (!seen.Add(entry.prefab)) continue; // already added, keep rooms unique
            list.Add((entry.prefab, firstSpawnPosition + spawnStepPerCave * index));
            index++;
        }
        return list;
    }

    private List<(GameObject prefab, Vector2 position)> CollectFromLocal()
    {
        var list = new List<(GameObject prefab, Vector2 position)>();
        var seen = new HashSet<GameObject>();
        int index = 0;
        foreach (var entry in caveEntries)
        {
            if (entry?.prefab == null) continue;
            if (Random.value > entry.spawnChance) continue;
            if (!seen.Add(entry.prefab)) continue; // already added, keep rooms unique
            list.Add((entry.prefab, firstSpawnPosition + spawnStepPerCave * index));
            index++;
        }
        return list;
    }

    private void SpawnAll(List<(GameObject prefab, Vector2 position)> toSpawn)
    {
        foreach (var (prefab, position) in toSpawn)
            Instantiate(prefab, position, Quaternion.identity, transform);
    }

    private IEnumerator SpawnOverFrames(List<(GameObject prefab, Vector2 position)> toSpawn, int count, float startTime)
    {
        int perFrame = Mathf.Max(1, cavesPerFrame);
        for (int i = 0; i < toSpawn.Count; i += perFrame)
        {
            int end = Mathf.Min(i + perFrame, toSpawn.Count);
            for (int j = i; j < end; j++)
            {
                var (prefab, position) = toSpawn[j];
                Instantiate(prefab, position, Quaternion.identity, transform);
            }
            yield return null;
        }
        LogLoadTime(count, startTime);
        BroadcastLocationsSpawned();
    }

    private void LogLoadTime(int caveCount, float startTime)
    {
        if (!logLoadTime || startTime <= 0f) return;
        float elapsed = Time.realtimeSinceStartup - startTime;
        float ms = elapsed * 1000f;
        Debug.Log($"[LocationRandomizer] Spawned {caveCount} cave(s) in {elapsed:F3}s ({ms:F1} ms)", this);
    }

    private void BroadcastLocationsSpawned()
    {
        MMEventManager.TriggerEvent(new MMGameEvent(LocationsSpawnedEventName));
    }
}
