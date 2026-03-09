# Character Abilities (Способности персонажа)

[English](README.md) | [Русский](README.ru.md)

---

**Способности персонажа** в стиле Corgi Engine и общая логика ударов: захват уступа, удар вниз, разрешение попаданий по зоне. Способности добавляются через *Corgi Engine → Character → Abilities*; компоненты ответа/раннера вешаются на объекты мира или используются способностями.

---

## Jump speed (прыжок с места = скорость бега)

| Файл | Описание |
|------|----------|
| **CharacterHorizontalMovementRunJump.cs** | Наследует **CharacterHorizontalMovement**. JumpSpeed из **CharacterRun.RunSpeed**. При прыжке (Jumping/DoubleJumping/WallJumping): MovementSpeed = JumpSpeed. **Замените** CharacterHorizontalMovement. |
| **CharacterJumpRunSpeed.cs** | Наследует **CharacterJump**. При IsJumping вызывает `ResetHorizontalSpeed`, чтобы JumpSpeed применялась корректно. В Initialization ищет **CharacterHorizontalMovementRunJump**. **Замените** CharacterJump. |

## Прочие способности

| Файл | Описание |
|------|----------|
| **AbilityLedgeGrab2D.cs** | **Захват уступа**: рейкастами определяется край стены, персонаж висит со смещением, подъём по W/Вверх. Настройки: SolidLayers, дистанции и смещения лучей стены/верха, смещения виса/подъёма, OnlyWhenFalling, MinAirTimeBeforeGrab, IgnoreWallIfSameAsStandingOn, MinWallNormalX, RequireSameColliderForWallAndLedge, GrabCooldown. Меню: *Corgi Engine → Character → Abilities → Ability Ledge Grab 2D*. |
| **AbilityDownStrike.cs** | **Удар вниз**: в воздухе удар вниз (клавиша Вниз + левая кнопка). Использует **StrikeZoneRunner**; сила отскока с каждого попадания из **DownStrikeResponse** или по умолчанию. Опции: ZonePrefab / ZoneSize, StrikeableLayers, DelayFrames, DamageAmount, TargetInvincibilityDuration, BounceForce, ResetHorizontalSpeedOnBounce, StrikeCooldown. Меню: *Corgi Engine → Character → Abilities → Ability Down Strike*. |
| **StrikeZoneRunner.cs** | **Общая логика удара** (статический класс): спавн зоны (префаб или процедурный бокс), ожидание N кадров, разрешение попаданий через OverlapBoxAll — эффект на объект через `getEffectFromHit` (например отскок), урон при наличии Health — затем вызов колбэка `onResolved(anyHit, effectFromLastHit)`. Для удара вниз используйте **GetBounceFromHit**; для других типов ударов — свой делегат. Без пункта меню (утилитный класс). |
| **DownStrikeResponse.cs** | Реакция только на **удар вниз**. Вешается на объекты в зоне удара (с Health или без, например платформа-отскок). Задаёт **BounceForce** (сила вверх, прикладываемая к атакующему). Для ударов вбок/вперёд — другие компоненты ответа. Без пункта меню (MonoBehaviour на объектах мира). |
