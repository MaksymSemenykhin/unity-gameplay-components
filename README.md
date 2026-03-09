# Unity Gameplay Components (Corgi Engine)

[English](README.md) | [Русский](README.ru.md)

---

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
│   ├── ZoneTriggerSwitcher.cs
│   └── Abilities/
│       ├── CharacterHorizontalMovementRunJump.cs
│       ├── CharacterJumpRunSpeed.cs
│       ├── AbilityLedgeGrab2D.cs
│       ├── AbilityDownStrike.cs
│       ├── StrikeZoneRunner.cs
│       ├── DownStrikeResponse.cs
│       └── README.md
├── Environment/            # Surface / platform modifiers, traps (Corgi SurfaceModifier)
│   ├── SurfaceModifierRestrictions.cs
│   ├── TrapZone.cs
│   ├── TrapHealth.cs
│   ├── DamageOnTouchOneShot.cs
│   └── README.md
├── Location/               # Level location randomization (loot caves)
│   ├── LocationRandomizer.cs
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

Character abilities, movement fixes, and zone triggers. Abilities folder: [Character/Abilities/README.md](Character/Abilities/README.md) | [Character/Abilities/README.ru.md](Character/Abilities/README.ru.md).

- **OneWayPlatformLandingFix** — Fixes unrealistic upward speed when transitioning from a slope onto a one-way platform (prevents the character from “flying” at the junction). Attach to an object with `CorgiController`; spike threshold and clamp speed are configurable in the Inspector.
- **Jump speed** — Replace CharacterHorizontalMovement and CharacterJump with **CharacterHorizontalMovementRunJump** and **CharacterJumpRunSpeed** so jump from stand = run speed.
- **AbilityLedgeGrab2D** — **Ledge grab** ability: detects wall edge, hang, climb on W/Up. Configurable layers, raycast distances, offsets, cooldown, and options (e.g. only when falling, ignore wall under feet). Add via menu: *Corgi Engine → Character → Abilities → Ability Ledge Grab 2D*.
- **AbilityDownStrike** — **Downward strike**: in the air, strike below (Down + left click). Bounce per hit object via **DownStrikeResponse** or default. Option **Reset Horizontal Speed On Bounce** (default on) zeroes horizontal speed before bounce so the bounce is always the same. Add via menu: *Corgi Engine → Character → Abilities → Ability Down Strike*.
- **DownStrikeResponse** — Response to **downward** strike only. Attach to objects in the strike zone (with or without Health, e.g. bouncy platform). Sets **BounceForce**. For side/forward strikes use other response components.
- **StrikeZoneRunner** — **Shared strike logic** (static): spawn zone, wait N frames, resolve hits (effect from getEffectFromHit, damage where Health present), callback (anyHit, effect). For down strike pass `GetBounceFromHit`; runner stays generic for other strike types.
- **ZoneTriggerSwitcher** — **Zone trigger** (Collider2D, trigger): on Player enter/exit, enable or disable GameObjects and fade SpriteRenderer alpha. Optional side filter (enter/exit from left or right only). Add via menu: *Gameplay → Zone Trigger Switcher*.

### Environment

Components for surfaces and platforms (aligned with Corgi’s Environment concepts), plus trap zones. See [Environment/README.md](Environment/README.md) | [Environment/README.ru.md](Environment/README.ru.md).

- **SurfaceModifierRestrictions** — Inherits Corgi **SurfaceModifier**. Add to a platform (Collider2D) to restrict characters on that surface: **Allow Jump** (default off) disables the jump ability via `PermitAbility(false)` while on the surface; permission is restored on exit and on component disable. Base friction and force still apply. Menu: *Gameplay → Surface Modifier Restrictions*.
- **TrapZone** — Trap zone: spawns a telegraph prefab when any trigger fires (enter by tag, on hit, on destroy); prefab controls how to start. Optional breakable: add **TrapHealth** (any damage breaks it). Menu: *Gameplay → Environment → Trap Zone*.
- **TrapHealth** — Extends Corgi **Health** for traps: forwards all damage, notifies **TrapZone** on hit and on death. Use with **DownStrikeResponse** for bounce. Menu: *Gameplay → Environment → Trap Health*.
- **DamageOnTouchOneShot** — Extends Corgi **DamageOnTouch**: destroys this GameObject after dealing damage once. Use for trap damage prefabs (e.g. TrapZone damage prefab). Menu: *Gameplay → Environment → Damage On Touch One Shot*.

### Location Randomizer

**Random sub-locations at level load:** loot caves (or other locations) are spawned from prefabs outside the main map. Spawn chance is set **per prefab** in the Inspector. See [Location/README.md](Location/README.md) | [Location/README.ru.md](Location/README.ru.md).

### Telegraph System

**Telegraphing** for attacks and abilities:

- **TelegraphProgressController** — Logic: phases (Idle → Telegraphing → Active → Finishing), progress 0…1, interrupt/cancel, optional reaction window with events.
- **TelegraphProgressView_RectSprite** — Visualization: frame and fill (SpriteRenderer), scale by progress, color changes (telegraph / reaction / active), hide and fade out on finish.

See [Telegraph/README.md](Telegraph/README.md) | [Telegraph/README.ru.md](Telegraph/README.ru.md) for details.

---

## License

MIT License. See [LICENSE](LICENSE).
