# Location Randomizer

Randomly spawns loot caves (or other sub-locations) at level load from a set of prefabs, outside the main map. **Spawn chance is set per prefab.**

Approach: one component in the scene; “what to spawn” can be configured in the scene (list) or in a **ScriptableObject** (reuse across scenes). “Where” and “when” are set on the component. To wait until done, listen for the `LocationsSpawned` event.

## How it works

On level load, `LocationRandomizer` rolls spawn chance once per entry (prefab + chance). On success, it instantiates that prefab in the spawn area (outside the main map). Caves are laid out in a row (or grid) using First Spawn Position and Spawn Step Per Cave.

## Setup

### LocationRandomizer (on a scene object or next to GameController)

| Field | Description |
|-------|-------------|
| **Config** | Optional: ScriptableObject with cave set and chances (Create → Gameplay → Location Randomizer Config). If set, it is used; otherwise the list below is used. |
| **Cave Entries** | List of prefab + chance (0–1) when Config is not set. One roll per entry at start; on success one instance is spawned. |
| **First Spawn Position** | World position of the first cave (e.g. far from the main map). |
| **Spawn Step Per Cave** | Offset to the next cave (order follows which entries passed the roll). |
| **When to run** | **OnAwake** — run in Awake. **OnCorgiLevelStart** — when Corgi fires LevelStart. |
| **Spread Over Frames** | Enable if you have many or heavy prefabs: spawn one (or several) caves per frame to avoid a single heavy frame. |
| **Caves Per Frame** | When Spread is on: how many caves to spawn per frame (1 = smoothest, higher = finishes sooner). |

## Performance

A few caves (e.g. 5–10) usually cause no noticeable lag. If prefabs are heavy (many objects, Awake/Start) or the list is long, enable **Spread Over Frames** so spawn cost is spread across frames.

## Waiting for spawn to finish (for GameController)

When the randomizer has finished spawning caves, it broadcasts **MMGameEvent("LocationsSpawned")**. To have GameController (or other code) wait for locations to be ready:

1. Implement `MMEventListener<MMGameEvent>`.
2. In `OnMMEvent(MMGameEvent e)` check `e.EventName == LocationRandomizer.LocationsSpawnedEventName` (or `"LocationsSpawned"`).
3. At that point all caves are spawned — enable gameplay, build navigation, etc.

Subscribe in OnEnable and unsubscribe in OnDisable.

## When to run

- **OnAwake** — usually enough: caves are created before gameplay starts.
- **OnCorgiLevelStart** — when you need to run strictly after Corgi level init (LevelManager, player spawn).
- **Manually** — from your code: `GetComponent<LocationRandomizer>().RunRandomization();` (LocationsSpawned is still broadcast when done).
