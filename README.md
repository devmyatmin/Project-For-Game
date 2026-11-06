# Project-For-Game

Stranded Deep project repository on branch `work/stranded-deep-original`.

## Unity asset file inventory

Inventory checked against the GitHub repository tree on 2026-11-07.

| Path / extension | Count | Finding |
|---|---:|---|
| `VisualAssets/ExportedProject/Assets/**/*.assets` | 0 | No Unity serialized bundle files with the `.assets` extension found in this branch. |
| `VisualAssets/ExportedProject/Assets/**/*.asset` | 209 | Extracted Unity serialized asset files (`.asset` extension); distinct from `.assets` containers. |
| Unity `Mesh/` inventory (per asset audit) | 164 | Mesh data exists in extracted Unity `.asset` files; not standalone interchange model files. |
| Standalone model formats (`.fbx`, `.obj`, `.glb`, `.gltf`, `.dae`, `.3ds`, `.blend`, `.mesh`, `.ma`, `.mb`, `.usd`, `.usdz`, `.abc`) | 0 | No files in these model/interchange formats found in the branch tree. |
| `Texture2D/` exported inventory (per audit/tree) | 289 | Extracted texture assets are present. |
| Texture image files (`.png`) under exported Assets | 293 | Tree scan counted 293 PNG files across the Assets tree; 289 are in the `Texture2D/` category. |
| `Material/` exported inventory (per audit/tree) | 119 | Unity material assets are present in the extracted `Material/` folder. |
| `.mat` files under exported Assets | 121 | Tree scan counted 121 `.mat` files total; folder inventory contains 119. |
| `.anim` files under exported Assets | 45 | Unity animation clip assets found in `AnimationClip/`; their combined Git blob size is about 100.0 MB. |
| `.anim.meta` files | 45 | Each of the 45 listed animation clips has a corresponding metadata file in the repository tree. |
| `.controller` files under exported Assets | 4 | Unity Animator Controllers are present: Boss Shark, Eel, Seaplane, and Squid. |
| `.ogg` files under exported Assets | 53 | Extracted audio clips in Ogg format. |
| `.wav` files under exported Assets | 1 | Extracted audio clip in WAV format. |
| Audio files total (`.ogg` + `.wav`) | 54 | Matches the existing `AudioClip/` inventory of 54. |
| Audio `.meta` files | 54 | Corresponding Unity metadata files found for these clips. |
| `.mp3`, `.aiff`, `.aif`, `.flac`, `.bank`, `.wem` | 0 | These audio formats were not found in the branch tree. |
| `.overrideController` files | 0 | No Animator Override Controller files found. |
| `Sprite/` serialized sprite assets | 21 | Unity sprite references/data files are present. |
| UI-related PNG atlas/texture candidates (named `*Sprite*.png` or `*Icon*.png`) | 13 | UI and HUD textures found in the `Texture2D/` folder; exact slice/atlas composition needs checking. |
| `Font/` files | 1 | `LiberationSans.ttf` is the only font file found. |
| `.ttf` / `.otf` fonts under Assets | 1 / 0 | No other standalone font formats found. |
| `.shader` files under exported Assets | 13 | Unity shader source files present. |
| `.shadervariants` files | 0 | No serialized shader-variant files found. |
| `.resS` | 0 | Not found in the repository tree. |
| `.resource` | 0 | Not found in the repository tree. |

### Textures and materials assessment

**Texture and Material assets are present in the repository's extracted Unity project.** Texture names in the tree include examples such as `Anchor1_albedo.png`, `Anchor1_NRM.png`, `BUOY_DIFF.png`, `BUOY_NRM.png`, `Terrain_DIFF.png`, and `Terrain_NRM.png`. The materials are Unity `.mat` files; their shader/property setup needs conversion or recreation in Godot. Texture availability does not itself verify that every mesh's material/texture references are intact, nor that the textures/materials import correctly in Godot. This was a repository tree/file-name inventory, not a runtime or conversion test.

### UI Sprite, Font, and Shader assessment

**UI Sprite assets:** The extracted Unity `Sprite/` folder contains 21 serialized `.asset` entries (excluding `.meta`): `Border`, `Classification_Food`, `Crafting_Icon`, `Fuel_Icon`, radial-bar background/fill, five skill sprites, `Sprite_Bars`, heatwave/temperature sprites, four vital HUD sprites (Health, Hunger, SPF, Thirst), `Water_Icon`, `Waypoint_Marker`, and `WhitePixel`. The Assets tree also includes UI-named PNG texture candidates such as skill sprite images and HUD bar/vital textures. These are Unity serialized sprite definitions and PNG source textures, not yet confirmed as Godot-ready sprite atlases or correctly sliced UI components. UI scene/layout/pivot/9-slice settings and sprite references need validation/reconstruction.

**Fonts:** Only one standalone font file was found under the extracted Assets tree: `Font/LiberationSans.ttf`. TextMeshPro font asset scripts exist, but those are scripts, not font files. TMP font atlas/material configuration and text rendering behavior have not been checked; a Godot font resource may need to be created and UI typography reconfigured.

**Shaders:** The exported `Shader/` directory contains 13 `.shader` source files, including Beam Team shaders for watch screen, additive night-fade particles, skin, terrain base/bumped-specular/vertex-lit, refractive glass, `Projector_Multiply`, two-sided Standard shaders, and TextMeshPro distance-field/mobile distance-field shaders. There are no `.shadervariants` in the tree. Unity shader code and render-pipeline/material properties are not directly Godot-compatible; shader logic/material parameters must be ported or recreated using Godot ShaderLanguage and Godot materials. The count confirms file presence only, not shader compilation, visual equivalence, or successful Godot import.

This UI/font/shader audit was based on repository tree paths and filenames. It does not verify the actual visual appearance, sprite atlas slices, dependency GUID resolution, font glyph coverage, shader compile results, or Godot runtime behavior.

### Model availability assessment

**Mesh/model data is present in the extracted project, but standalone original/interchange model files are not present in this branch.** The extracted Mesh files are Unity serialized `.asset` data and cannot be presumed to import directly into Godot. A verified export/conversion step to a Godot-supported model format (such as `.glb`) is still required. Counts above reflect repository file-tree inspection and the existing [`VisualAssets/ASSET_AUDIT.md`](VisualAssets/ASSET_AUDIT.md); individual mesh conversion and Godot import have not been tested.

### Audio availability assessment

**Audio clips are present in the extracted Unity project:** 53 `.ogg` files and 1 `.wav` file (54 total), matching the existing `AudioClip/` inventory. The tree also contains 54 corresponding `.meta` files. Sample clip names indicate intro/cinematic sounds (`Intro_Crash`, `Intro_Explosion`, `Intro_Jet_Engine`), endgame/cockpit audio, and creature/boss effects (`eel_boss_attack`, `shark_attack_1` through `_4`, squid boss attack sounds), plus ambience and object sounds such as `Buoy_Loop` and metal door sounds.

This is a repository tree/name inventory only: audio playback, completeness of references, encoding quality, and Godot import were not tested. Ogg and WAV are formats Godot can generally import, but Unity AudioClip settings, looping/3D attenuation, mixer routing, and gameplay triggers may need to be recreated in Godot.

### Animation availability assessment

**Animation data is present in the repository as Unity-native clips and Animator Controller state-machine assets.** The tree inventory found 45 `.anim` clips (each with a matching `.anim.meta`) and 4 `.controller` files. Clip examples in the repository include aircraft crash/takeoff, eel idle/swimming/attack/damage/death, great-white shark swim/bite/damage/turning, seaplane cockpit actions, and squid attack/swim/damage/death clips. The four controllers are `Boss_Shark_Controller.controller`, `EelController.controller`, `Seaplane Animator.controller`, and `SquidController.controller`.

The Eel controller file itself contains states, transitions, parameters (including Speed, Direction, TakeDamage, Attack, Dead), and Blend Trees referencing animation clips by Unity GUID. This supports that controller/state-machine data exists, not only standalone clips. However, the audit does not verify that every GUID resolves to an included clip, that rig/skeleton dependencies are complete, or that animations play correctly. Unity `.anim` and `.controller` formats are not directly Godot-ready; clip curves, skeleton/rig mapping, Blend Trees, and transitions need conversion/reconstruction (for example into Godot AnimationPlayer/AnimationTree). No Godot animation import/playback test has been performed.

The original Unity container files commonly named `globalgamemanagers.assets`, `resources.assets`, and `sharedassets*.assets` were not found in the repository tree during this check. This records repository presence only; it does not mean the original game files do not exist elsewhere.

For the extracted VisualAssets category counts and conversion notes, see [`VisualAssets/ASSET_AUDIT.md`](VisualAssets/ASSET_AUDIT.md).


### Prefab and Scene assessment

**Unity Prefab inventory:** The repository tree contains 25 Unity `.prefab` files (about 5.28 MiB of Git blob content). They are distributed across extracted `VisualAssets/ExportedProject/Assets` folders:

| Folder | Prefab count | Included assets |
|---|---:|---|
| `GameObject/` | 5 | `Explosion_JetDebris`, `Explosion_Jetcrash`, `Label - Statistics Names`, `Label - Statistics Values`, `Label - Status Effect` |
| `resources asset bundles/missions/mission_eel/` | 2 | `ENVIRONMENT`, `GOAL_EEL` |
| `resources asset bundles/missions/mission_shark/` | 2 | `ENVIRONMENT`, `GOAL_SHARK` |
| `resources asset bundles/missions/mission_squid/` | 2 | `ENVIRONMENT`, `GOAL_SQUID` |
| `resources asset bundles/scenes/end game/` | 9 | `AIRCRAFT_ASSET`, `ENVIRONMENT`, `REPAIR_ENGINE`, `REPAIR_PROPELLER`, `REPAIR_RUDDER_PEDAL`, `STATION_AIRCRAFT`, `STATION_FOOD`, `STATION_FUEL`, `STATION_WATER` |
| `resources asset bundles/scenes/intro/` | 1 | `Intro_Assets` |
| `resources asset bundles/watches/beepi/` | 2 | `UI - Watch`, `View - 3D Watch` |
| `resources asset bundles/watches/getbit/` | 2 | `UI - Watch`, `View - 3D Watch` |
| **Total** | **25** | **Prefab files present in the tree** |

**Scene inventory:** No Unity `.unity` scene files were found anywhere in the branch tree. The repository does contain a Godot scene, `Scenes/Main.tscn` (54 bytes), which is not an original Unity scene. The Unity-extracted prefab paths include bundle-named folders such as `resources asset bundles/scenes/end game` and `.../intro`, but those are folders containing prefabs—not scene files. Therefore the original Unity scene hierarchy/layout and scene-level wiring are not present as standalone scene assets in this branch based on extension/path inventory.

**Godot conversion implications:** Prefabs are Unity serialized GameObject/component hierarchies. Their existence does not guarantee all external model, material, texture, script, or GUID dependencies resolve. The end-game environment/aircraft and intro prefab are relatively large serialized files; their contents and dependency references should be checked during import/conversion. Prefabs may be useful as hierarchy/component reference material, but they are not directly Godot `.tscn` scenes. Scene composition, node structure, script bindings, physics/collider setup, and references will need a conversion/reconstruction pass.

**Audit limits:** Counts and grouping above come from repository tree paths, filenames, and Git blob sizes. This pass did not parse Unity YAML contents, enumerate prefab GameObjects/components, resolve GUIDs, inspect nested prefab links, or test importing/running these assets in Unity or Godot. No claim is made that prefab dependencies are complete or functional.
