using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

/// <summary>
/// At level load, spawns room/cave instances from prefabs and optionally links them to gates.
/// Two modes:
/// 1) Legacy: for each Cave Entry roll spawn chance; on success spawn one instance at Spawn Positions[index]. No gate linking.
/// 2) BlueGate: one room per BlueGate, prefab chosen at random from Cave Entries without repeat (prefabs count must be >= BlueGates count). Each room's GreenGate is linked to the corresponding BlueGate (Teleporter.Destination).
/// Broadcasts LocationsSpawned when done. Optional: spread spawn over frames, log load time.
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
        [Tooltip("Chance this prefab will appear (0 = never, 1 = always). Used only in Legacy mode.")]
        [Range(0f, 1f)]
        public float spawnChance = 0.5f;
    }

    [Header("Caves")]
    [Tooltip("List of room prefabs. Legacy: each entry has independent spawn chance. BlueGate: pool for random pick (no repeat); count must be >= BlueGates length.")]
    [SerializeField] private List<CaveEntry> caveEntries = new List<CaveEntry>();

    [Header("Spawn positions")]
    [Tooltip("World position per room by index (0 = first). Missing indices use (0,0). Shown in Gizmos when selected.")]
    [SerializeField] private List<Vector2> spawnPositions = new List<Vector2>();

    [Header("When to run")]
    [Tooltip("OnAwake = run in Awake. OnCorgiLevelStart = run when Corgi fires LevelStart.")]
    [SerializeField] private RunMode runMode = RunMode.OnAwake;

    [Header("Performance")]
    [Tooltip("If true, spawn caves over multiple frames to avoid a single heavy frame.")]
    [SerializeField] private bool spreadOverFrames;
    [Tooltip("When Spread Over Frames is on: caves to spawn per frame (1 = smoothest).")]
    [SerializeField] private int cavesPerFrame = 1;

    [Header("BlueGate – GreenGate (gate-linked rooms)")]
    [Tooltip("If on: spawn one room per BlueGate, link each room's GreenGate to that BlueGate. Prefabs are drawn at random without repeat; prefab count must be >= BlueGate count.")]
    [SerializeField] private bool useBlueGateCount;
    [Tooltip("BlueGate transforms on the map (assign in order). Each gets exactly one room when Use Blue Gate Count is on.")]
    [SerializeField] private Transform[] blueGates = new Transform[0];
    [Tooltip("Name of the child object in the room prefab that is the exit (GreenGate). Must have a Teleporter component; Destination will be set to the BlueGate.")]
    [SerializeField] private string greenGateObjectName = "GreenGate";

    [Header("Debug")]
    [Tooltip("Log spawn count and elapsed time (seconds + ms) when randomization finishes.")]
    [SerializeField] private bool logLoadTime;

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
    /// Runs randomization: builds spawn list (Legacy or BlueGate), spawns instances, links gates if BlueGate mode, then broadcasts LocationsSpawned.
    /// Call manually if you need exact timing (e.g. from GameManager).
    /// </summary>
    public void RunRandomization()
    {
        var (toSpawn, gatesToLink) = CollectSpawnList();
        if (toSpawn == null)
        {
            Debug.LogWarning("[LocationRandomizer] No cave entries: fill Cave Entries.", this);
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
            StartCoroutine(SpawnOverFrames(toSpawn, gatesToLink, count, startTime));
        else
        {
            SpawnAll(toSpawn, gatesToLink);
            LogLoadTime(count, startTime);
            BroadcastLocationsSpawned();
        }
    }

    private (List<(GameObject prefab, Vector2 position)> toSpawn, IReadOnlyList<Transform> gatesToLink) CollectSpawnList()
    {
        if (useBlueGateCount)
            return CollectSpawnListByBlueGates();
        var list = CollectSpawnListLegacy();
        return (list, null);
    }

    /// <summary>
    /// BlueGate mode: one room per BlueGate. Prefab for each room is picked at random from Cave Entries without repeat.
    /// Requires prefab count >= BlueGate count; otherwise only the first (prefab count) gates get a room and a warning is logged.
    /// </summary>
    private (List<(GameObject prefab, Vector2 position)>, IReadOnlyList<Transform>) CollectSpawnListByBlueGates()
    {
        var blueGatesList = new List<Transform>();
        if (blueGates != null)
        {
            foreach (var t in blueGates)
                if (t != null) blueGatesList.Add(t);
        }

        int n = blueGatesList.Count;
        if (n == 0)
        {
            Debug.LogWarning("[LocationRandomizer] Use Blue Gate Count is on but BlueGates array is empty. Assign BlueGate transforms.", this);
            return (new List<(GameObject prefab, Vector2 position)>(), blueGatesList);
        }

        var prefabPool = GetCavePrefabPool();
        if (prefabPool.Count == 0)
        {
            Debug.LogWarning("[LocationRandomizer] Use Blue Gate Count is on but Cave Entries has no prefab. Add at least one cave entry.", this);
            return (new List<(GameObject prefab, Vector2 position)>(), blueGatesList);
        }

        if (prefabPool.Count < n)
        {
            Debug.LogWarning($"[LocationRandomizer] Prefabs ({prefabPool.Count}) must be >= BlueGates ({n}) so rooms don't repeat. Only first {prefabPool.Count} gates will get a room; add more cave prefabs for the rest.", this);
            n = prefabPool.Count;
        }

        if (spawnPositions != null && spawnPositions.Count < n)
            Debug.LogWarning($"[LocationRandomizer] Spawn Positions has fewer than {n} entries; missing rooms will spawn at (0,0).", this);

        var list = new List<(GameObject prefab, Vector2 position)>();
        var gatesToLink = new List<Transform>();
        var available = new List<GameObject>(prefabPool);
        for (int i = 0; i < n; i++)
        {
            int idx = Random.Range(0, available.Count);
            GameObject prefab = available[idx];
            available.RemoveAt(idx);
            list.Add((prefab, GetSpawnPosition(i)));
            gatesToLink.Add(blueGatesList[i]);
        }
        return (list, gatesToLink);
    }

    private List<GameObject> GetCavePrefabPool()
    {
        var pool = new List<GameObject>();
        if (caveEntries == null) return pool;
        foreach (var e in caveEntries)
            if (e?.prefab != null) pool.Add(e.prefab);
        return pool;
    }

    private Vector2 GetSpawnPosition(int index)
    {
        if (spawnPositions != null && index < spawnPositions.Count)
            return spawnPositions[index];
        return Vector2.zero;
    }

    /// <summary>
    /// Legacy mode: for each Cave Entry roll spawn chance; on success add one instance (unique prefab per run). Positions from Spawn Positions by index.
    /// </summary>
    private List<(GameObject prefab, Vector2 position)> CollectSpawnListLegacy()
    {
        if (caveEntries == null || caveEntries.Count == 0)
            return null;
        var list = new List<(GameObject prefab, Vector2 position)>();
        var seen = new HashSet<GameObject>();
        int index = 0;
        foreach (var entry in caveEntries)
        {
            if (entry?.prefab == null) continue;
            if (Random.value > entry.spawnChance) continue;
            if (!seen.Add(entry.prefab)) continue;
            list.Add((entry.prefab, GetSpawnPosition(index)));
            index++;
        }
        return list;
    }

    private void SpawnAll(List<(GameObject prefab, Vector2 position)> toSpawn, IReadOnlyList<Transform> gatesToLink)
    {
        for (int i = 0; i < toSpawn.Count; i++)
        {
            var (prefab, position) = toSpawn[i];
            var instance = Instantiate(prefab, position, Quaternion.identity, transform);
            if (gatesToLink != null && i < gatesToLink.Count)
                LinkRoomGreenGateToBlueGate(instance, gatesToLink[i]);
        }
    }

    /// <summary>
    /// Finds the GreenGate child in the room by name, then sets Teleporter.Destination on both GreenGate and BlueGate so they link.
    /// </summary>
    private void LinkRoomGreenGateToBlueGate(GameObject roomInstance, Transform blueGate)
    {
        var greenGateTransform = FindInChildrenByName(roomInstance.transform, greenGateObjectName);
        if (greenGateTransform == null)
        {
            Debug.LogWarning($"[LocationRandomizer] Room prefab has no child named \"{greenGateObjectName}\". Check hierarchy or Green Gate Object Name.", roomInstance);
            return;
        }
        var greenTeleporter = greenGateTransform.GetComponent<Teleporter>();
        var blueTeleporter = blueGate.GetComponent<Teleporter>();
        if (greenTeleporter != null && blueTeleporter != null)
        {
            greenTeleporter.Destination = blueTeleporter;
            blueTeleporter.Destination = greenTeleporter;
        }
        else if (greenTeleporter != null || blueTeleporter != null)
            Debug.LogWarning("[LocationRandomizer] Both BlueGate and GreenGate need a Teleporter component to link.", roomInstance);
    }

    private static Transform FindInChildrenByName(Transform root, string name)
    {
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var found = FindInChildrenByName(root.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    private IEnumerator SpawnOverFrames(List<(GameObject prefab, Vector2 position)> toSpawn, IReadOnlyList<Transform> gatesToLink, int count, float startTime)
    {
        int perFrame = Mathf.Max(1, cavesPerFrame);
        for (int i = 0; i < toSpawn.Count; i += perFrame)
        {
            int end = Mathf.Min(i + perFrame, toSpawn.Count);
            for (int j = i; j < end; j++)
            {
                var (prefab, position) = toSpawn[j];
                var instance = Instantiate(prefab, position, Quaternion.identity, transform);
                if (gatesToLink != null && j < gatesToLink.Count)
                    LinkRoomGreenGateToBlueGate(instance, gatesToLink[j]);
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
        Debug.Log($"[LocationRandomizer] Spawned {caveCount} cave(s) in {elapsed:F3}s ({elapsed * 1000f:F1} ms)", this);
    }

    private void BroadcastLocationsSpawned()
    {
        MMGameEvent.Trigger(LocationsSpawnedEventName);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (useBlueGateCount && blueGates != null)
        {
            for (int i = 0; i < blueGates.Length; i++)
            {
                if (blueGates[i] == null) continue;
                Vector2 spawnPos = GetSpawnPosition(i);
                Vector3 blue = blueGates[i].position;
                Vector3 green = new Vector3(spawnPos.x, spawnPos.y, blue.z);
                Gizmos.color = new Color(0.2f, 0.5f, 1f);
                Gizmos.DrawWireSphere(blue, 0.5f);
                Gizmos.color = new Color(0.2f, 0.9f, 0.3f);
                Gizmos.DrawWireSphere(green, 0.5f);
                Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.6f);
                Gizmos.DrawLine(blue, green);
            }
        }
        else if (spawnPositions != null && spawnPositions.Count > 0)
        {
            Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.8f);
            for (int i = 0; i < spawnPositions.Count; i++)
            {
                var p = spawnPositions[i];
                Gizmos.DrawWireSphere(new Vector3(p.x, p.y, 0f), 0.5f);
            }
        }
    }
#endif
}
