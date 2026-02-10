# Telegraph System (Corgi Engine)

The Telegraph System is a reusable gameplay module designed primarily for
**Corgi Engine–based Unity projects**.

It provides a clean and flexible way to **telegraph attacks, abilities and actions**
using progress-based visual indicators, with full support for interruption and cancellation.

The system is built to integrate naturally with:
- Corgi Engine abilities
- Enemy attacks and AI actions
- Charge / cast / wind-up mechanics
- World-space and UI-based telegraph visuals

---

## What is a Telegraph?

A telegraph is a **visual warning** shown to the player before an action is executed.

Typical use cases:
- Enemy attack wind-up
- Charged abilities
- Area-of-effect warnings
- Timed interactions
- Casting or channeling actions

This system separates:
- **Logic** (timing, states, interruption)
- **Visualization** (progress bars, frames, color changes)

---

## Core Concepts

### Telegraph Runner
Handles the telegraph lifecycle:
- Start
- Progress
- Interrupt / Cancel
- Complete
- Optional post-complete state (e.g. flash / danger window)

The runner contains **no visuals** and can be driven by:
- Corgi abilities
- AI scripts
- Custom gameplay code

---

### Telegraph View
Responsible only for visualization:
- Frames / shapes
- Progress fill
- Color transitions
- Visibility control

Views react to runner events instead of controlling logic directly.

---

### Event-driven Design
The system relies on events rather than hard references:
- Gameplay logic emits state changes
- Views listen and react
- Easy to swap visuals without touching logic

This approach fits well with Corgi Engine’s ability-driven architecture.

---

## Designed for Corgi Engine

The Telegraph System is optimized for Corgi Engine workflows:
- Ability-based execution flow
- Interruptible actions
- Clear separation of responsibilities
- Minimal coupling to Character or AI implementations

It is intended to **extend Corgi Engine**, not replace its systems.

---

## Typical Flow

1. Ability or AI triggers telegraph start
2. Telegraph runner begins progress
3. Visual telegraph appears and fills over time
4. Telegraph is either:
   - Completed → action executes
   - Interrupted → telegraph is canceled
5. Visuals react accordingly

---

## Usage

Each part of the system can be used independently.

You can:
- Drive the runner from a Corgi Ability
- Attach different views to the same runner
- Interrupt telegraphs from damage, movement or state changes

Sample scenes will demonstrate common Corgi Engine use cases.

---

## License

MIT License.
