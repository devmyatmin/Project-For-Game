# Unity UI and Prefab Extraction Audit — 2026-11-07

## Repository and scope

- Repository: `devmyatmin/Project-For-Game`
- Branch: `work/stranded-deep-original`
- Source: `VisualAssets/ExportedProject/Assets/`
- Extracted copy: `VisualAssets/UI_Prefabs_Extracted/SourceAssets/`
- Extraction commit: `f02345af7d296121da8817adf4d218d5e2b40167`

The extraction is a separate copy of the assets already present in the exported Unity project. Original source paths were preserved below `SourceAssets/`; source files were not edited or deleted. Unity `.meta` files were copied with their assets to retain GUIDs.

## Inventory

Copied 1,347 files in total, including 25 prefabs and their metadata/support assets:

| Asset group | Files excluding .meta |
|---|---:|
| Prefabs | 25 |
| Texture2D PNG | 289 |
| Materials | 119 |
| Sprite assets | 21 |
| Mesh assets | 164 |
| Animation clips | 45 |
| Animator controllers | 4 |
| Font | 1 |
| UI / TextMeshPro / Unity UI scripts and assembly definitions | 19 |
| **Total payload/source files** | **687** |
| Unity .meta files | 660 |
| **Total copied files** | **1,347** |

The extracted support set deliberately includes the complete exported Texture2D, Material, Sprite, Mesh, AnimationClip, AnimatorController and Font directories, plus UI adapter scripts and Unity UI/TextMeshPro scripts. This is a broad support set, not a minimal dependency closure.

## Prefab inventory

### GameObject prefabs (5)
- `GameObject/Explosion_JetDebris.prefab`
- `GameObject/Explosion_Jetcrash.prefab`
- `GameObject/Label - Statistics Names.prefab`
- `GameObject/Label - Statistics Values.prefab`
- `GameObject/Label - Status Effect.prefab`

### Mission prefabs (6)
- `resources asset bundles/missions/mission_eel/ENVIRONMENT.prefab`
- `resources asset bundles/missions/mission_eel/GOAL_EEL.prefab`
- `resources asset bundles/missions/mission_shark/ENVIRONMENT.prefab`
- `resources asset bundles/missions/mission_shark/GOAL_SHARK.prefab`
- `resources asset bundles/missions/mission_squid/ENVIRONMENT.prefab`
- `resources asset bundles/missions/mission_squid/GOAL_SQUID.prefab`

### End-game scene prefabs (9)
- `resources asset bundles/scenes/end game/AIRCRAFT_ASSET.prefab`
- `resources asset bundles/scenes/end game/ENVIRONMENT.prefab`
- `resources asset bundles/scenes/end game/REPAIR_ENGINE.prefab`
- `resources asset bundles/scenes/end game/REPAIR_PROPELLER.prefab`
- `resources asset bundles/scenes/end game/REPAIR_RUDDER_PEDAL.prefab`
- `resources asset bundles/scenes/end game/STATION_AIRCRAFT.prefab`
- `resources asset bundles/scenes/end game/STATION_FOOD.prefab`
- `resources asset bundles/scenes/end game/STATION_FUEL.prefab`
- `resources asset bundles/scenes/end game/STATION_WATER.prefab`

### Intro prefab (1)
- `resources asset bundles/scenes/intro/Intro_Assets.prefab`

### Watch prefabs (4)
- `resources asset bundles/watches/beepi/UI - Watch.prefab`
- `resources asset bundles/watches/beepi/View - 3D Watch.prefab`
- `resources asset bundles/watches/getbit/UI - Watch.prefab`
- `resources asset bundles/watches/getbit/View - 3D Watch.prefab`

## Detailed structural inspection

Prefab YAML was inspected for object names, Unity component records, external GUID references, and file sizes. The 24 non-empty prefab files contain 3,723 serialized Unity component records in total. The `Intro_Assets.prefab` file returned zero bytes and must be treated as an empty/unusable placeholder until checked against the original game export.

Notable structures:
- Beepi watch UI: 97 GameObjects and 396 serialized component records. It contains a Canvas, watch camera/view, time/date/temperature labels and multiple UI elements.
- Getbit watch UI: 7 GameObjects and 26 serialized component records; includes Canvas, watch camera/view, clock, date and time labels.
- Beepi 3D watch: 7 GameObjects / 22 component records; male and female watch models and screen positions.
- Getbit 3D watch: 4 GameObjects / 15 component records.
- End-game aircraft prefab: 1,036 component records and 175 distinct referenced GUIDs.
- End-game environment prefab: 1,380 component records.
- Squid goal prefab: 199 component records.
- The three label prefabs are compact single-GameObject UI elements with RectTransform and UI/text-related components.

## UI-related scripts copied

The extraction includes the exported `Beam/UI` adapters, including:
- `TMPBeepiWatchViewAdapter.cs`
- `TMPLabelViewAdapter.cs`
- `UCanvasGroupViewAdapter.cs`
- `UFilledImageViewAdapter.cs`
- `UImageViewAdapter.cs`
- `USliderViewAdapter.cs`
- `WatchScreenViewAdapter.cs`

It also includes the exported Unity UI and TextMeshPro support scripts from their original folders.

## Limitations / next validation

1. This extraction includes every prefab found under the current exported Assets tree, not a claim that every prefab/UI asset from the complete original game installation has been exported.
2. The broad support directories were copied to preserve likely UI/prefab dependencies and Unity GUID metadata. A strict per-prefab GUID dependency-closure report has not yet been generated.
3. Unity prefab YAML and GUID references have been preserved, but the copied set has not yet been reopened in Unity or converted into Godot scenes/resources.
4. Godot conversion still needs mapping of RectTransform/Canvas, TMP text, Unity UI Image/Slider/Mask/LayoutGroup, materials/shaders, animation controllers and watch-camera rendering.
5. `Intro_Assets.prefab` is empty in the current export; do not treat it as a valid extracted prefab.

## Result

**Extraction copy completed and committed.** The 25 available prefab files and their associated broad UI/rendering support set are present under `VisualAssets/UI_Prefabs_Extracted/SourceAssets/`. This is a preservation/extraction result, not yet a validated Godot-ready UI implementation.
