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

    [Header("BlueGate - GreenGate")]
    [Tooltip("If true, spawn one room per BlueGate and link each room's GreenGate to the corresponding BlueGate.")]
    [SerializeField] private bool useBlueGateCount;
    [Tooltip("BlueGate objects on the map (fill by hand). Count = how many rooms to spawn.")]
    [SerializeField] private Transform[] blueGates = new Transform[0];
    [Tooltip("Prefab of the room (must contain a child with GreenGate name). If not set, first from Config/Cave Entries is used.")]
    [SerializeField] private GameObject roomPrefabWithGreenGate;
    [Tooltip("Name of the GreenGate object inside the room prefab (searched in hierarchy).")]
    [SerializeField] private string greenGateObjectName = "GreenGate";

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
        var (toSpawn, blueGates) = CollectSpawnList();
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
            StartCoroutine(SpawnOverFrames(toSpawn, blueGates, count, startTime));
        }
        else
        {
            SpawnAll(toSpawn, blueGates);
            LogLoadTime(count, startTime);
            BroadcastLocationsSpawned();
        }
    }

    private (List<(GameObject prefab, Vector2 position)> toSpawn, IReadOnlyList<Transform> blueGatesOut) CollectSpawnList()
    {
        if (useBlueGateCount)
            return CollectSpawnListByBlueGates();

        var list = CollectSpawnListLegacy();
        return (list, null);
    }

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

        var prefab = roomPrefabWithGreenGate;
        if (prefab == null && config != null && config.CaveEntries.Count > 0)
            prefab = config.CaveEntries[0].prefab;
        if (prefab == null && caveEntries != null && caveEntries.Count > 0)
            prefab = caveEntries[0].prefab;
        if (prefab == null)
        {
            Debug.LogWarning("[LocationRandomizer] Use Blue Gate Count is on but no room prefab: set Room Prefab With Green Gate or add cave entries.", this);
            return (new List<(GameObject prefab, Vector2 position)>(), blueGatesList);
        }

        var list = new List<(GameObject prefab, Vector2 position)>();
        for (int i = 0; i < n; i++)
            list.Add((prefab, firstSpawnPosition + spawnStepPerCave * i));
        return (list, blueGatesList);
    }

    private List<(GameObject prefab, Vector2 position)> CollectSpawnListLegacy()
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

    private void SpawnAll(List<(GameObject prefab, Vector2 position)> toSpawn, IReadOnlyList<Transform> blueGatesOut)
    {
        for (int i = 0; i < toSpawn.Count; i++)
        {
            var (prefab, position) = toSpawn[i];
            var instance = Instantiate(prefab, position, Quaternion.identity, transform);
            if (blueGatesOut != null && i < blueGatesOut.Count)
                LinkRoomGreenGateToBlueGate(instance, blueGatesOut[i]);
        }
    }

    private void LinkRoomGreenGateToBlueGate(GameObject roomInstance, Transform blueGate)
    {
        var greenGateTransform = FindInChildrenByName(roomInstance.transform, greenGateObjectName);
        if (greenGateTransform == null)
        {
            Debug.LogWarning($"[LocationRandomizer] Room prefab has no child named \"{greenGateObjectName}\". Check hierarchy or Green Gate Object Name.", roomInstance);
            return;
        }
        greenGateTransform.SendMessage("SetLinkedGate", blueGate, SendMessageOptions.DontRequireReceiver);
        blueGate.SendMessage("SetLinkedGate", greenGateTransform, SendMessageOptions.DontRequireReceiver);
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

    private IEnumerator SpawnOverFrames(List<(GameObject prefab, Vector2 position)> toSpawn, IReadOnlyList<Transform> blueGatesOut, int count, float startTime)
    {
        int perFrame = Mathf.Max(1, cavesPerFrame);
        for (int i = 0; i < toSpawn.Count; i += perFrame)
        {
            int end = Mathf.Min(i + perFrame, toSpawn.Count);
            for (int j = i; j < end; j++)
            {
                var (prefab, position) = toSpawn[j];
                var instance = Instantiate(prefab, position, Quaternion.identity, transform);
                if (blueGatesOut != null && j < blueGatesOut.Count)
                    LinkRoomGreenGateToBlueGate(instance, blueGatesOut[j]);
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
