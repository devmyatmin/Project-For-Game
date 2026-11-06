# Original Stranded Deep Logic — Reference Index

## Scope and preservation rules

- This index is for the isolated `work/stranded-deep-original` branch only.
- The original extracted C# source files remain in their existing folders. They are not renamed, moved, rewritten, or duplicated here.
- This folder is an index, not a Godot runtime module.
- No C# to GDScript conversion has been performed.
- No replacement or invented gameplay logic has been added.
- Do not treat a Unity C# file as Godot-ready merely because it is present in this repository.

## Repository scan snapshot

The isolated branch currently contains:
- 613 C# source files.
- 0 active `.gd` scripts after removal of the temporary prototype scripts.
- The original Unity-export reference material remains under `VisualAssets/ExportedProject/`.

The source inventory is spread across the original domain folders. The list below identifies high-value systems and their integration status; it does not claim every dependency has been exhaustively resolved.

## Useful original logic areas

### Player state, movement, and presentation

- `Player_Controller/PlayerData.cs` — player position and rotation data.
- `Player_Controller/Movement.cs` — original player movement controller. It is a Unity `CharacterController` / physics / input / water / platform integration, not a standalone movement algorithm.
- `Player_Controller/PlayerCamera.cs` — camera modes and player camera integration; depends on Unity camera objects and game systems.
- `Player_Controller/PlayerOxygenHudViewAdapterBase.cs` — oxygen HUD view contract.
- `Player_Controller/PlayerStaminaHudViewAdapterBase.cs` — stamina HUD view contract.
- `Survival_Systems/StaminaHUDPresenter.cs` — presenter connecting stamina events to the view.

### Survival values and status effects

- `Player_Controller/PlayerEffect.cs` — original effect data, rates, duration, and save/load fields.
- `Survival_Systems/StatusEffect.cs` — original status-effect definitions and difficulty-dependent setup.
- `Survival_Systems/StatusEffectName.cs` — original status-effect identifiers.

These files preserve original rules, but they depend on the original Beam serialization, player/statistics, game-time, difficulty, and Unity systems. They are reference sources, not plug-in Godot scripts.

### Item-system source boundary

The repository contains additional item, storage, interaction, and crafting code. It is a tightly connected Unity subsystem with mixed gameplay dependencies. It is intentionally not selected or staged in this reference index during this pass; no combat-oriented logic is being integrated.

### World, island, weather, and terrain

- `World_Island/Island.cs` — island graph / biome layout logic; depends on the Delaunay/Voronoi implementation and Unity types/random state.
- `World_Island/WeatherEvent.cs` — weather event timing and fade lifecycle; depends on Unity components/gradients and MEC coroutine timing.
- `World_Island/WeatherEventEffectBase.cs` — base contract for weather effects.
- `World_Island/StrandedWorld.cs` — world/zone orchestration; tightly coupled to Unity Terrain, saved zones, and game services.
- `World_Island/World.cs` — original world/map bundle serialization; depends on original Map types and Unity/game utilities.
- `World_Island/ZoneLoader.cs` — asynchronous zone loading pipeline; depends on Unity, MEC, multiplayer fences, and original prefab/map systems.

### Save/load and map data

- `Save_Load/Saveable.cs`
- `Save_Load/Serializer.cs`
- `Save_Load/LE_SaveLoad.cs` — original 257×257 heightmap file read/write routine; uses Unity's application data path and binary file APIs.
- Related save/load interfaces and serializers remain in `Save_Load/`.

These are useful format and behavior references, but the complete save pipeline requires the original serializer, map, prefab, and object-reference systems.

## Integration assessment

| Area | Original source present | Self-contained | Ready to run in Godot now |
|---|---:|---:|---:|
| Player movement/camera | Yes | No | No |
| Survival/status effects | Yes | No | No |
| Inventory/crafting | Yes | No | No |
| World/island/weather | Yes | No | No |
| Save/load/map data | Yes | No | No |
| Oxygen/stamina HUD contracts | Yes | Partial interfaces | No |

**Result:** this scan found substantial original logic source, but did not verify a complete, self-contained Godot-ready gameplay subsystem. The correct preservation step is to keep these original files intact and use this index to select the exact source and dependencies when integration is explicitly requested.

## APK export boundary

The Android export preset and isolated-branch build workflow are separate from gameplay logic. The workflow is scoped to this branch. No gameplay source is required just to retain the APK export configuration.

## Explicitly not done

- No conversion to GDScript.
- No replacement gameplay implementation.
- No generated placeholder assets or gameplay.
- No changes to `main` or the other work branches.
- No edits to the original C# source files.
