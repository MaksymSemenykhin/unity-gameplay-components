# Environment

[English](README.md) | [Русский](README.ru.md)

---

Components for **surfaces and platforms**, aligned with Corgi Engine’s Environment concepts. They extend Corgi’s **SurfaceModifier** to add behaviour (e.g. restricting jump) while keeping base friction and force. Also includes **trap zones**: trigger-based telegraph spawn and optional breakable traps (down strike / direct attack).

---

## Files

| File | Description |
|------|-------------|
| **SurfaceModifierRestrictions.cs** | Inherits Corgi **SurfaceModifier**. Add to a platform (Collider2D) to restrict characters on that surface. **Allow Jump** (default off) disables the jump ability via `PermitAbility(false)` while the character is on the surface; permission is restored on exit and when the component is disabled. Base friction and force from the parent class still apply. Menu: *Gameplay → Surface Modifier Restrictions*. |
| **TrapZone.cs** | **Trap zone**: telegraph prefab is spawned when any of three optional triggers fire — **(1)** an object with one of the **trigger tags** enters the collider (trigger), **(2)** **Trigger On Hit** — trap receives valid damage (TrapHealth), **(3)** **Trigger On Destroy** — trap is broken (OnBroken). All triggers use the same one-shot logic (first trigger wins). Optionally **Call Play On Spawn** — if on, `TelegraphProgressController.Play()` is called; if off, the prefab can auto-start. Optional **breakable** trap: **Break By** — **DownStrike**, **Direct**, or **Both**. Menu: *Gameplay → Environment → Trap Zone*. |
| **TrapHealth.cs** | **Trap health** (extends Corgi **Health**): used with **TrapZone** when the trap is breakable. Accepts damage only when the attack type matches the trap’s **Break By** (down strike = downward direction + instigator has **AbilityDownStrike**; direct = otherwise). On death calls **TrapZone.OnBroken()**. Add to the same object or child (with collider on the strike layer); add **DownStrikeResponse** for bounce on down strike. Menu: *Gameplay → Environment → Trap Health*. |
