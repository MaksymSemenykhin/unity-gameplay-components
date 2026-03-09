# Character Abilities

[English](README.md) | [Русский](README.ru.md)

---

Corgi Engine–style **character abilities** and shared strike logic: ledge grab, downward strike, zone-based hit resolution. Add abilities via *Corgi Engine → Character → Abilities*; response/runner components attach to world objects or are used by abilities.

---

## Jump speed (jump from stand = run speed)

| File | Description |
|------|-------------|
| **CharacterHorizontalMovementRunJump.cs** | Inherits **CharacterHorizontalMovement**. JumpSpeed from **CharacterRun.RunSpeed**. When jumping (Jumping/DoubleJumping/WallJumping), MovementSpeed = JumpSpeed. **Replace** CharacterHorizontalMovement. |
| **CharacterJumpRunSpeed.cs** | Inherits **CharacterJump**. When IsJumping, calls `ResetHorizontalSpeed` so JumpSpeed applies cleanly. Finds **CharacterHorizontalMovementRunJump** in Initialization. **Replace** CharacterJump. |

## Other abilities

| File | Description |
|------|-------------|
| **AbilityLedgeGrab2D.cs** | **Ledge grab** ability: detects wall edge via raycasts, character hangs at offset, climbs on W/Up. Configurable: SolidLayers, wall/top ray distances and offsets, hang/climb offsets, OnlyWhenFalling, MinAirTimeBeforeGrab, IgnoreWallIfSameAsStandingOn, MinWallNormalX, RequireSameColliderForWallAndLedge, GrabCooldown. Add via menu: *Corgi Engine → Character → Abilities → Ability Ledge Grab 2D*. |
| **AbilityDownStrike.cs** | **Downward strike**: in the air, strike below (Down key + left click). Uses **StrikeZoneRunner**; bounce force per hit from **DownStrikeResponse** or default. Options: ZonePrefab / ZoneSize, StrikeableLayers, DelayFrames, DamageAmount, TargetInvincibilityDuration, BounceForce, ResetHorizontalSpeedOnBounce, StrikeCooldown. Add via menu: *Corgi Engine → Character → Abilities → Ability Down Strike*. |
| **StrikeZoneRunner.cs** | **Shared strike logic** (static): spawns a zone (prefab or procedural box), waits N frames, resolves hits with OverlapBoxAll — applies effect per object via `getEffectFromHit` (e.g. bounce), damage where Health is present — then invokes callback `onResolved(anyHit, effectFromLastHit)`. Use **GetBounceFromHit** for down-strike bounce; other strike types pass their own delegate. No menu (utility class). |
| **DownStrikeResponse.cs** | Response to **downward strike** only. Attach to objects inside the strike zone (with or without Health, e.g. bouncy platform). Exposes **BounceForce** (upward force applied to the striker). For side/forward strikes use other response components. No menu (MonoBehaviour on world objects). |
