# Location Randomizer

[English](README.md) | [Русский](README.ru.md)

---

Spawns room/cave instances at level load from prefabs and optionally links them to gates. Two modes: **Legacy** (per-prefab spawn chance, no gates) and **BlueGate** (one room per gate, random prefab without repeat, gate linking).

---

## Modes

### Legacy (Use Blue Gate Count = off)

- **Cave Entries**: list of (prefab, spawn chance 0–1).
- For each entry the component rolls once; on success it spawns one instance of that prefab.
- Each prefab is used at most once per run (no duplicate room types).
- Positions come from **Spawn Positions** by index (0 = first room); missing indices use (0,0).
- No gate linking.

### BlueGate (Use Blue Gate Count = on)

- **BlueGates**: array of transforms (gates on the main map). Each gate gets exactly one room.
- **Cave Entries**: pool of room prefabs. For each gate a prefab is chosen **at random without repeat** (shuffle-style). So every room is a different prefab.
- **Rule: prefab count must be >= BlueGate count.** If you have fewer prefabs than gates, a warning is logged and only the first (prefab count) gates get a room; the rest get none.
- Each room is spawned at **Spawn Positions**[index]. The room prefab must contain a child named **Green Gate Object Name** (e.g. `"GreenGate"`) with a **Teleporter** component. The component links that GreenGate to the corresponding BlueGate (both `Teleporter.Destination` are set) so entering the BlueGate sends the player to the room and the room exit sends them back.

---

## Inspector reference

| Section | Field | Description |
|--------|--------|-------------|
| **Caves** | Cave Entries | Prefabs + spawn chance (Legacy) or prefab pool (BlueGate). BlueGate: count >= BlueGates length. |
| **Spawn positions** | Spawn Positions | World position per room by index. Missing indices = (0,0). Shown in Gizmos. |
| **When to run** | Run Mode | OnAwake or OnCorgiLevelStart. |
| **Performance** | Spread Over Frames | If on, spawn over multiple frames. Caves Per Frame = how many per frame. |
| **BlueGate – GreenGate** | Use Blue Gate Count | If on, one room per BlueGate, gate linking. |
| | BlueGates | Transforms of gates on the map (order = room index). |
| | Green Gate Object Name | Name of exit object inside room prefab (with Teleporter). |
| **Debug** | Log Load Time | Log spawn count and time when done. |

---

## Events and timing

- When spawning is finished, **MMGameEvent(LocationsSpawnedEventName)** is broadcast. Listen with `MMEventListener<MMGameEvent>` and check `e.EventName == LocationRandomizer.LocationsSpawnedEventName` to run logic after rooms are ready (e.g. enable gameplay, build navigation). Subscribe in OnEnable, unsubscribe in OnDisable.
- You can call **RunRandomization()** manually from your code (e.g. GameManager); the event is still broadcast when done.

---

## Gizmos (Scene view)

- With **Use Blue Gate Count** on: blue sphere = BlueGate position, green sphere = room spawn position, line between them.
- With Legacy: green spheres = Spawn Positions.
