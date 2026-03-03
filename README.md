# Unity Gameplay Components (Corgi Engine)

A collection of modular, reusable gameplay components for **Unity** projects using **[Corgi Engine](https://corgi-engine.moremountains.com/)**.

Components are extracted from real game development and tailored to typical scenarios in Corgi Engine–based games.

---

## Requirements

- **Unity** (version compatible with your Corgi Engine release)
- **Corgi Engine** (MoreMountains) — installed in the project

---

## Repository structure

```
├── Character/              # Character components
│   ├── OneWayPlatformLandingFix.cs
│   └── Abilities/
│       ├── AbilityLedgeGrab2D.cs
│       └── AbilityDownStrike.cs
├── Location/               # Level location randomization (loot caves)
│   ├── LocationRandomizer.cs
│   ├── LocationRandomizerConfig.cs
│   └── README.md
├── Telegraph/              # Attack/ability telegraph system
│   ├── TelegraphProgressController
│   ├── TelegraphProgressView_RectSprite
│   └── README.md
├── LICENSE
└── README.md
```

---

## Included systems

### Character

- **OneWayPlatformLandingFix** — Fixes unrealistic upward speed when transitioning from a slope onto a one-way platform (prevents the character from “flying” at the junction). Attach to an object with `CorgiController`; spike threshold and clamp speed are configurable in the Inspector.
- **AbilityLedgeGrab2D** — **Ledge grab** ability: detects wall edge, hang, climb on W/Up. Configurable layers, raycast distances, offsets, cooldown, and options (e.g. only when falling, ignore wall under feet). Add via menu: *Corgi Engine → Character → Abilities → Ability Ledge Grab 2D*.
- **AbilityDownStrike** — **Downward strike** ability: in the air, trigger a strike below (key or stick down). Spawns a hitbox zone (BoxCollider2D) so you can shape it in the Inspector; waits a few frames (DelayFrames) so visuals can render, then applies damage to all targets in the zone (each Health once) and applies bounce once to avoid duplicating the effect. Add via menu: *Corgi Engine → Character → Abilities → Ability Down Strike*.

### Location Randomizer

**Random sub-locations at level load:** loot caves (or other locations) are spawned from prefabs outside the main map. Spawn chance is set **per prefab** in the Inspector. See [Location/README.md](Location/README.md).

### Telegraph System

**Telegraphing** for attacks and abilities:

- **TelegraphProgressController** — Logic: phases (Idle → Telegraphing → Active → Finishing), progress 0…1, interrupt/cancel, optional reaction window with events.
- **TelegraphProgressView_RectSprite** — Visualization: frame and fill (SpriteRenderer), scale by progress, color changes (telegraph / reaction / active), hide and fade out on finish.

See [Telegraph/README.md](Telegraph/README.md) for details.

---

## License

MIT License. See [LICENSE](LICENSE).
