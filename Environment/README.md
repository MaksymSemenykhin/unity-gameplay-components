# Environment

[English](README.md) | [Русский](README.ru.md)

---

Components for **surfaces and platforms**, aligned with Corgi Engine’s Environment concepts. They extend Corgi’s **SurfaceModifier** to add behaviour (e.g. restricting jump) while keeping base friction and force.

---

## Files

| File | Description |
|------|-------------|
| **SurfaceModifierRestrictions.cs** | Inherits Corgi **SurfaceModifier**. Add to a platform (Collider2D) to restrict characters on that surface. **Allow Jump** (default off) disables the jump ability via `PermitAbility(false)` while the character is on the surface; permission is restored on exit and when the component is disabled. Base friction and force from the parent class still apply. Menu: *Gameplay → Surface Modifier Restrictions*. |
