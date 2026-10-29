# VisualAssets Audit (Initial)

Date: 2026-10-29
Branch: main

## Scope

Reviewed the GitHub recursive tree for `VisualAssets/`. This is a file-structure audit only; no Godot import or runtime test has been performed yet.

## Inventory

- `VisualAssets/` contains 1,686 tracked files (including Unity metadata and auxiliary files), approximately 901 MB by the GitHub tree's uncompressed file-size totals.
- Main extracted Unity content is under `VisualAssets/ExportedProject/Assets/`.
- `VisualAssets/AuxiliaryFiles/path_id_map.json` is present.

Extracted content counts (excluding `.meta` files):

| Unity asset folder | Files |
|---|---:|
| Mesh | 164 |
| Texture2D | 289 |
| Material | 119 |
| AudioClip | 54 |
| AnimationClip | 45 |
| Sprite | 21 |
| GameObject | 5 |
| TerrainData | 1 |
| AnimatorController | 4 |
| Shader | 13 |
| resources asset bundles | 30 |

## Initial assessment

### Potentially useful now

- Terrain textures: `Terrain_DIFF.png`, `Terrain_NRM.png`, `Terrain_Detail.png`, `Terrain_Sand_*`, `Terrain_Grass_Dirt_*`.
- General reusable textures and icons: `Texture2D/` and `Sprite/`.
- Some sound effects in `AudioClip/` (review and map to actual gameplay events before wiring).
- Some meshes that appear to belong to the aircraft, wreck, survivor, watch, shark, eel, and squid.

### Important limitations

- Many models are Unity serialized `.asset` files rather than interchange formats such as `.glb`, `.gltf`, `.fbx`, or `.obj`. Godot cannot be assumed to import these directly.
- Unity `.mat`, `.prefab`, `.anim`, `.controller`, and `.asset` files are not directly equivalent to Godot scenes/materials/animations.
- The visible mesh inventory is weighted toward aircraft/endgame/intro, survivor, and boss content. Core island survival props (for example, palm trees, rocks, ordinary crafting resources, and common wildlife) were not evident in the listed mesh names. This may mean the current extraction is partial, not that those assets do not exist in the original game.
- Unity shaders and material settings will need translation to Godot materials/shaders; visual parity should not be assumed.
- Large textures and animation assets need mobile memory/performance review.

## Recommended port order

1. Keep Unity-extracted content as reference; do not compile it as Godot code.
2. Prioritize the minimum playable loop: island ground/terrain, player, basic interactable resource props, inventory/crafting data, survival stats, save/load, and mobile controls.
3. Determine whether source mesh data can be exported to an interchange format. If not, identify an authorized conversion/export route before attempting bulk conversion.
4. Build Godot-native `.tscn` scenes and `.tres` materials for converted assets.
5. Import assets in small batches and test on Android before adding high-cost boss/intro/endgame content.
6. Review asset provenance and license/usage permissions before redistributing builds or source.

## Verification status

- GitHub tree inventory: completed.
- Individual asset serialization/conversion tests: not yet performed.
- Godot editor import/build: not yet verified.
- Android device performance: not yet verified.
