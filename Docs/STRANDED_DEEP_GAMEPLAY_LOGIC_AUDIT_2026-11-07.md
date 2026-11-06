# Stranded Deep Gameplay Logic Audit
**Date:** 2026-11-07  
**Repository:** `devmyatmin/Project-For-Game`  
**Branch:** `work/stranded-deep-original`  
**Audit scope:** Player coordination, survival statistics/status effects, inventory/crafting, construction, wildlife/animal logic.

## 1. Audit status and limitations

This document records the initial source-level audit of the decompiled C# code currently present in `DecompiledDLLs/Assembly-CSharp-firstpass/`. It identifies core classes, observed fields/methods, and major dependencies.

**Important scope limit:** this is not yet an exhaustive method-by-method trace across both `Assembly-CSharp.dll` and `Assembly-CSharp-firstpass.dll`. The full cross-DLL dependency graph, all interface implementations, every call site, and runtime behavior still require verification. Numeric values below are code defaults observed in the inspected source and may be changed at runtime by difficulty, skills, effects, or save data. No runtime tests are claimed.

## 2. Player logic

### Core coordinator
- `Beam/Player.cs` — central player coordinator and component owner.
- Main components referenced: `Character`, `Movement`, `Statistics`, `Inventory`, `Holder`, `Crafter`, `Interacter`, `PlayerCamera`, `PlayerUI`, `PlayerSkills`, `WatchController`, and `PlayerExperience`.
- Additional player systems observed: `PlayerSpeech`, `GameInputController`, `FireLighter`, `Skinner`, `Dragger`, `Achievements`, and `Hotkeys`.

### Supporting player files inspected
- `Beam/PlayerData.cs`
- `Beam/PlayerRegistry.cs`
- `Beam/PlayerSkills.cs`
- `Beam/PlayerEffect.cs`
- `Beam/Statistics.cs`
- `Beam/StatusEffect.cs`
- `Beam/PlayerUtilities.cs`
- `Beam/Movement.cs`
- `Beam/Character.cs`
- `Beam/PlayerCamera.cs`
- `GameInputController.cs`

### Responsibilities and dependencies
- `PlayerRegistry` provides local-player and multiplayer player references.
- `Movement` covers movement/physics, vehicle interaction, ledge climbing, ladders, velocity, and save/load.
- `Character` covers character animation, first-/third-person presentation, held-item presentation, and jump/climb/death animation.
- `PlayerCamera` provides camera ray position/direction and ray overrides used by interaction.
- `GameInputController` routes input into interaction and drag systems.
- Interfaces/contracts observed in the player area include `IPlayer`, `INeedPlayerComponent`, and `ISaveable`; multiplayer peer/message/replication types are also dependencies.

## 3. Survival statistics: Health, Hunger, Thirst, Oxygen

### Main source
`Beam/Statistics.cs` is the central statistics controller. Hunger is represented by **Calories**, and thirst by **Fluids**; the inspected source does not model them as separate `Hunger` and `Thirst` numeric fields.

### Observed default values

| Statistic | Observed default |
|---|---:|
| Health | 700 |
| Calories (hunger reserve) | 700 |
| Fluids (thirst reserve) | 700 |
| Sleep | 700 |
| Stamina | 100 |
| Oxygen | 100 |
| Maximum oxygen | 100 |
| Drowning timer | 15 seconds |
| Oxygen drain speed | 3.5 |
| Oxygen regain speed | 40 |

These are source defaults, not guaranteed runtime values.

### Main methods observed
- Health: `SetHealth()`, `AddHealth()`, `ApplyDamage()`, `Die()`, `Revive()`, `UpdatePlayerHealth()`.
- Hunger: `UpdatePlayerHunger()`.
- Thirst: `UpdatePlayerThirst()`.
- Oxygen: `UpdateOxygen()`, `AddOxygen()`.
- Stamina: `UpdateStamina()`, `StaminaUpdated` event, `OutOfBreath`.

### Depletion and health conditions
- Hunger depletion constant: `DAYS_TO_DEPLETE_HUNGER = 2`; hard mode uses 1 day.
- Thirst depletion constant: `DAYS_TO_DEPLETE_THIRST = 1`; hard mode uses 0.5 day.
- Health is clamped to 0–700 in the inspected logic.
- Health regeneration requires a healthy condition and Calories and Fluids at or above 500.
- Maximum oxygen is affected by `PlayerSkills.PhysicalMultiplier`.
- Oxygen depletion starts the drowning timer; the observed default timer is 15 seconds.

### Status effects
Relevant files: `Beam/StatusEffect.cs`, `Beam/PlayerEffect.cs`, and `Beam/IPlayerEffect.cs`.

Positive effects identified:
- Health Regeneration
- Sunblock
- Shark Repellent
- Breath Boost
- Splint

Negative effects identified:
- Tired
- Starvation
- Dehydration
- Bleeding
- Drowning
- Broken Bones
- Illness
- Poison
- Sunstroke
- Diarrhoea
- Malnutrition

Observed duration/depletion constants in `StatusEffect.cs`:
- Health regeneration full: 7 hours
- Breath boost: 3 hours
- Sunblock: 12 hours
- Shark repellent: 12 hours
- Splint: 48 hours
- Drowning days-to-kill: 0.007
- Starvation: 2 days (hard: 1)
- Dehydration: 2 days (hard: 1)
- Bleeding: 3 days (hard: 1.5)
- Illness: 6 days (hard: 3)
- Poison: 3 days (hard: 1.5)
- Malnutrition: 3 days (hard: 1.5)

The source also defines effect creation/calculated health-effect rates (including a 700-health basis over days and hours). Exact runtime application order and every interaction between effects still need a deeper trace.

## 4. Inventory and crafting

### Inventory
Main source: `Beam.Crafting/Inventory.cs`.

- Inventory uses `ISlotStorage<IPickupable>` as its backing storage.
- `Inventory.Save()` and `Inventory.Load()` delegate persistence to the slot-storage implementation.
- Related contracts/types include `ISlotStorage`, `SlotStorage`, `IPickupable`, `IStorage`, and `IPlayer`.

### Held item / holder
Source: `Beam.Crafting/Holder.cs`.

Observed responsibilities and methods:
- Current held-item tracking and selection.
- Pickup/drop: `CanPickup()`, `Pickup()`, `DropCurrent()`.
- Visibility: `HideCurrent()`, `ShowCurrent()`.
- Selection and release: `Select()`, `ReleaseAndDestroy()`.
- Multiplayer: `ReplicatedPickup()`.
- Persistence: `Save()`, `Load()`.

### Crafting flow
Main source: `Beam.Crafting/Crafter.cs`.

Observed high-level sequence:
1. `BeginPollMaterials()`
2. `PollMaterials()`
3. `GetCraftingMaterials()`
4. `GetCraftingCombinations()`
5. `Craft()`
6. If placement is required: `PlaceCraftable_Sequence()`
7. Cancellation paths: `CancelCrafting()` / `CancelPlacing()`

`CraftingCombination.IsValid(CraftingInfo)` checks recipe/material validity.

Related files:
- `CrafterState.cs`
- `CraftingCombination.cs`
- `CraftingCombinations.cs`
- `CraftingInfo.cs`
- `CraftingCategory.cs`
- `CraftingSubcategory.cs`
- `CraftingType.cs`

## 5. Construction logic

### Main source
`Beam.Crafting/Constructing.cs`.

The inspected class implements/uses the following contracts:
- `IConstructable`
- `ICraftable`
- `IBase`
- `IHasCraftingType`
- `IPlaceable`
- `ISelectable`

Observed methods:
- Crafting/placement lifecycle: `OnBeginCrafting()`, `OwnerPlacing()`, `ReplicantPlacing()`, `Place()`, `DoPlace()`.
- Placement validation: `GetConnectorsAtPlacement()`, `CheckPlacementIsSupported()`, `CheckProximity()`, `CheckStructure()`.
- Durability/lifecycle: `CheckHealth()`, `Damage()`, `Dismantle()`.
- Persistence: `Save()`, `Load()`.

### Connectors and placement modifiers
`Beam.Crafting/Connector.cs` includes `ConnectTo()`, `Refresh()`, `GetLocalId()`, `Save()`, and `Load()`.

Placement modifier classes found:
- `PlacementModifier`
- `PlacementModifier_LimitHeightToTerrain`
- `PlacementModifier_SnapBase`
- `PlacementModifier_SnapRotationToConnector`
- `PlacementModifier_SnapToConstructions`
- `PlacementModifier_SnapToLayer`
- `PlacementModifier_SnapToTerrain`
- `PlacementModifier_SnapToWater`

Construction-related types identified:
- `Construction_ARCH`
- `Construction_CAMPFIRE`
- `Construction_DOOR`
- `Construction_FLOOR`
- `Construction_FOUNDATION`
- `Construction_PANEL`
- `Construction_PIT`
- `Construction_RAFT`
- `Construction_STEPS`
- `Construction_STRUCTURE`
- `Construction_STRUCTURE_GYRO`
- `Construction_STRUCTURE_RAFT`
- `ConstructionObject_BED`
- `ConstructionObject_HAMMER`
- `ConstructionObject_ITEM_PILE`
- `ConstructionObject_TROPHY`
- `ConstructionObject_WATERBOTTLE`
- `ConstructionObject_SPEAR`

**Conversion note:** campfire behavior is not just a mesh. Its source includes fire/fuel, cooking, interaction, and save/load concerns; these need gameplay equivalents in Godot.

## 6. Wildlife and animal logic

### Land animals
Main source: `Beam/LandAnimal.cs`.

The inspected hierarchy uses state-based AI. Observed state/behavior methods include:
- `SEARCH_FOR_PLAYER()`
- `CAN_SEE_PLAYER()`
- `SteerAvoidance()`
- `FindHidingSpot()`
- `NewAvoidWaypoint()`
- `NewExploreWaypoint()`
- `NewHideWaypoint()`
- `NewChaseWaypoint()`
- `CheckIfStuck()`
- `Kill()`
- `ReplicatedChangeState()`

Related classes include `LandAnimalState`, `Boar`, `Snake`, `GiantCrab`, and `LandCrab`. Species-specific behavior is layered on the animal/AI hierarchy, including attack, damage, animation, and replication concerns.

### Fish and fishing
Files identified:
- `FishBase.cs`
- `FishSpawner.cs`
- `FishMovement.cs`
- `FishRendererBase.cs`
- `FishingData.cs`
- `FishingManager.cs`
- `FlockFish.cs`
- `SingleFish.cs`
- `Interactive_FISH.cs`
- `Interactive_FISHES.cs`
- `Interactive_FISH_RAGDOLL.cs`

`FishBase` was observed implementing `IFishable`, `IFishRenderer`, `ICellObject`, and `ISaveable`. Methods include `Hook()`, `Catch()`, `Free()`, `Save()`, and `Load()`.

### Boss creatures
Boss-related classes identified:
- `Boss_Shark`
- `Boss_Eel`
- `Boss_Squid`
- `Boss_Squid_Tentacle`
- `Boss_Squid_Environment`

These use the `Piscus_Creature` hierarchy and state/attack methods such as `SEARCH_FOR_PLAYER()`, `Initialize()`, `ApplyDamage()`, `Idle_EnterState()`, `Idle_FixedUpdate()`, `Attempt_Attacks()`, and `Reset()`.

### Animal interaction and spawning
Additional classes identified:
- `InteractiveObject_ANIMAL`
- `InteractiveObject_BOAR`
- `InteractiveObject_SNAKE`
- `InteractiveObject_GIANTCRAB`
- `Interactive_BIRD`
- `InteractiveObject_CRAB`
- `InteractiveObject_MIDFISH`
- `InteractiveObject_WHALE`
- `AnimalAudio`
- `AnimalAudioBase`
- `LandAnimalSpawner`

## 7. Godot porting implications

| Original system | Godot-side responsibility to implement or map |
|---|---|
| Player coordinator/components | Player scene + component/node ownership and signals |
| Statistics | Central player-statistics resource/controller |
| Calories / Fluids | Hunger and thirst values, depletion and thresholds |
| Oxygen / drowning | Underwater oxygen controller and drowning timer |
| Status effects | Timed effect/resource system with modifiers |
| Inventory / slot storage | Inventory data model, slots, item resources, save/load |
| Holder | Held-item state, equip/unequip and drop handling |
| Crafter / combinations | Recipe database, material polling, validation and craft execution |
| Constructing / connectors | Placement preview, snapping, support/proximity checks and structure links |
| Land animal state machine | Navigation + explicit AI state machine |
| Fish/fishing | Fish movement/spawning and hook/catch/release lifecycle |
| Boss creatures | Separate boss state machines, attack patterns and environment interactions |

This table is a porting map, not an assertion that these Godot systems already exist or are complete.

## 8. Dependency verification still required

The following contracts/types were observed as dependencies and should be located and traced across both decompiled assemblies before implementing replacements:
- Player: `IPlayer`, `INeedPlayerComponent`, `ISaveable`, multiplayer peer/message/replication types.
- Inventory/crafting: `ISlotStorage`, `SlotStorage`, `IPickupable`, `IStorage`, `IConstructable`, `ICraftable`, `IBase`, `IHasCraftingType`, `IPlaceable`, `ISelectable`.
- Wildlife/fish: `IFishable`, `IFishRenderer`, `ICellObject`, `Piscus_Creature`, and the full animal state/spawner/audio dependencies.

Next audit pass should:
1. Resolve each dependency to its exact source file and owning assembly.
2. Trace callers/callees for the methods listed above.
3. Identify missing, duplicated, stubbed, or obfuscated implementations.
4. Separate reusable gameplay rules from Unity-specific APIs, physics, animation, UI, and networking.
5. Validate behavior with runtime tests where a runnable original environment is available.

## 9. Source file entry points

- [Player.cs](../DecompiledDLLs/Assembly-CSharp-firstpass/Beam/Player.cs)
- [Statistics.cs](../DecompiledDLLs/Assembly-CSharp-firstpass/Beam/Statistics.cs)
- [StatusEffect.cs](../DecompiledDLLs/Assembly-CSharp-firstpass/Beam/StatusEffect.cs)
- [Inventory.cs](../DecompiledDLLs/Assembly-CSharp-firstpass/Beam.Crafting/Inventory.cs)
- [Holder.cs](../DecompiledDLLs/Assembly-CSharp-firstpass/Beam.Crafting/Holder.cs)
- [Crafter.cs](../DecompiledDLLs/Assembly-CSharp-firstpass/Beam.Crafting/Crafter.cs)
- [Constructing.cs](../DecompiledDLLs/Assembly-CSharp-firstpass/Beam.Crafting/Constructing.cs)
- [Connector.cs](../DecompiledDLLs/Assembly-CSharp-firstpass/Beam.Crafting/Connector.cs)
- [LandAnimal.cs](../DecompiledDLLs/Assembly-CSharp-firstpass/Beam/LandAnimal.cs)

---
**Integrity note:** This commit adds this documentation file only. It does not edit decompiled source, Unity assets, Godot project files, or existing audit documents.
