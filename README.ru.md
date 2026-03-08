# Unity Gameplay Components (Corgi Engine)

[English](README.md) | [Русский](README.ru.md)

---

Набор модульных, переиспользуемых геймплейных компонентов для проектов **Unity** на базе **[Corgi Engine](https://corgi-engine.moremountains.com/)**.

Компоненты вынесены из реальной разработки игр и адаптированы под типичные сценарии игр на Corgi Engine.

---

## Требования

- **Unity** (версия, совместимая с вашей версией Corgi Engine)
- **Corgi Engine** (MoreMountains) — установлен в проекте

---

## Структура репозитория

```
├── Character/              # Компоненты персонажа
│   ├── OneWayPlatformLandingFix.cs
│   ├── ZoneTriggerSwitcher.cs
│   └── Abilities/
│       ├── AbilityLedgeGrab2D.cs
│       ├── AbilityDownStrike.cs
│       ├── StrikeZoneRunner.cs
│       ├── DownStrikeResponse.cs
│       └── README.md
├── Environment/            # Модификаторы поверхностей, ловушки (Corgi SurfaceModifier)
│   ├── SurfaceModifierRestrictions.cs
│   ├── TrapZone.cs
│   ├── TrapHealth.cs
│   └── README.md
├── Location/               # Рандомизация локаций уровня (пещеры с лутом)
│   ├── LocationRandomizer.cs
│   └── README.md
├── Telegraph/              # Система телеграфов атак/способностей
│   ├── TelegraphProgressController
│   ├── TelegraphProgressView_RectSprite
│   └── README.md
├── LICENSE
└── README.md
```

---

## Включённые системы

### Character (Персонаж)

Способности персонажа, исправления движения и зональные триггеры. Папка Abilities: [Character/Abilities/README.md](Character/Abilities/README.md) | [Character/Abilities/README.ru.md](Character/Abilities/README.ru.md).

- **OneWayPlatformLandingFix** — Устраняет нереалистичную скорость вверх при переходе со склона на одностороннюю платформу (предотвращает «полёт» на стыке). Вешается на объект с `CorgiController`; порог шипов и ограничение скорости настраиваются в Inspector.
- **AbilityLedgeGrab2D** — **Захват уступа**: обнаружение края стены, вис, подъём по W/Вверх. Настраиваются слои, дистанции рейкастов, смещения, откат и опции (например, только при падении, игнор стены под ногами). Добавление: *Corgi Engine → Character → Abilities → Ability Ledge Grab 2D*.
- **AbilityDownStrike** — **Удар вниз**: в воздухе удар вниз (Вниз + левая кнопка). Отскок задаётся через **DownStrikeResponse** или по умолчанию. Опция **Reset Horizontal Speed On Bounce** (по умолчанию вкл.) обнуляет горизонтальную скорость перед отскоком. Добавление: *Corgi Engine → Character → Abilities → Ability Down Strike*.
- **DownStrikeResponse** — Реакция только на **удар вниз**. Вешается на объекты в зоне удара (с Health или без, например платформа-отскок). Задаёт **BounceForce**. Для ударов вбок/вперёд — другие компоненты ответа.
- **StrikeZoneRunner** — **Общая логика удара** (статическая): спавн зоны, ожидание N кадров, разрешение попаданий (эффект из getEffectFromHit, урон при наличии Health), колбэк (anyHit, effect). Для удара вниз передаётся `GetBounceFromHit`; раннер остаётся универсальным для других типов ударов.
- **ZoneTriggerSwitcher** — **Зональный триггер** (Collider2D, trigger): при входе/выходе игрока — включение/выключение GameObject и изменение альфы SpriteRenderer. Опциональный фильтр по стороне (вход/выход только слева или справа). Меню: *Gameplay → Zone Trigger Switcher*.

### Environment (Окружение)

Компоненты для поверхностей и платформ (в духе Corgi Environment) и ловушки. Подробнее: [Environment/README.md](Environment/README.md) | [Environment/README.ru.md](Environment/README.ru.md).

- **SurfaceModifierRestrictions** — Наследует Corgi **SurfaceModifier**. Добавляется на платформу (Collider2D), чтобы ограничить персонажей на этой поверхности: **Allow Jump** (по умолчанию выкл) отключает прыжок через `PermitAbility(false)` на поверхности; разрешение восстанавливается при выходе и при отключении компонента. Базовое трение и сила по-прежнему применяются. Меню: *Gameplay → Surface Modifier Restrictions*.
- **TrapZone** — Ловушка: при входе объекта с одним из заданных тегов триггера спавнится префаб телеграфа; опционально вызов `Play()` или автостарт префаба. Опционально ломаемая ловушка (Break By: DownStrike / Direct / Both). Меню: *Gameplay → Environment → Trap Zone*.
- **TrapHealth** — Наследует Corgi **Health** для ломаемых ловушек: принимает урон только от типа атаки, заданного в **TrapZone** (удар вниз или прямая). При смерти вызывает **TrapZone.OnBroken()**. Использовать с **DownStrikeResponse** для отскока. Меню: *Gameplay → Environment → Trap Health*.

### Location Randomizer (Рандомизатор локаций)

**Случайные подлокации при загрузке уровня:** пещеры с лутом (или другие локации) спавнятся из префабов вне основной карты. Шанс спавна задаётся **для каждого префаба** в Inspector. Подробнее: [Location/README.md](Location/README.md) | [Location/README.ru.md](Location/README.ru.md).

### Telegraph System (Система телеграфов)

**Телеграфирование** атак и способностей:

- **TelegraphProgressController** — Логика: фазы (Idle → Telegraphing → Active → Finishing), прогресс 0…1, прерывание/отмена, опциональное окно реакции с событиями.
- **TelegraphProgressView_RectSprite** — Визуализация: рамка и заливка (SpriteRenderer), масштаб по прогрессу, смена цвета (телеграф / реакция / активная фаза), скрытие и затухание по завершении.

Подробнее: [Telegraph/README.md](Telegraph/README.md) | [Telegraph/README.ru.md](Telegraph/README.ru.md).

---

## Лицензия

MIT License. См. [LICENSE](LICENSE).
