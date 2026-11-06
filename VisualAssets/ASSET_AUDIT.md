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


## Original Unity Mesh copy record

Date: 2026-11-07
Branch: `work/stranded-deep-original`

The workflow copied the extracted Unity Mesh folder into the Godot project repository at:

- Source: `VisualAssets/ExportedProject/Assets/Mesh/`
- Project destination: `VisualAssets/OriginalModels/UnityMeshAssets/`
- Copy method: `cp -a "$SRC/." "$DEST/"` (preserves the source directory contents and file metadata where supported)

### Copy verification

- Destination files: **328 total**
- Unity Mesh `.asset` files: **164**
- Unity metadata `.meta` files: **164**
- Mesh `.asset` file size total: **42,432,241 bytes** (about 40.5 MiB)
- All destination entries are present in the repository branch.

A content inspection of `Buoy.asset` confirmed Unity Mesh serialization (`!u!43`), `m_IsReadable: 1`, vertex data and index buffer. The mesh reports **6,146 vertices** and **14,085 submesh indices**. This is genuine serialized mesh data, not just a filename placeholder.

### Import/conversion status

These are still Unity serialized Mesh assets. The copy is now stored inside the Godot project repository, but it is **not yet converted to Godot-importable GLB/GLTF/OBJ**, and no Godot import/runtime test has been done. Keep this folder as original/reference source; converted Godot assets should be kept in a separate destination so the original data is not overwritten.


## Original Unity Texture2D copy record

Date: 2026-11-07
Branch: `work/stranded-deep-original`

The original extracted Unity Texture2D files were copied into the Godot project repository.

- Source: `VisualAssets/ExportedProject/Assets/Texture2D/`
- Project destination: `VisualAssets/OriginalTextures/UnityTexture2D/`
- Copy method: workflow `.github/workflows/copy-original-textures.yml` uses `cp -a "$SRC/." "$DEST/"`

### Copy verification

- Destination entries: **578 files**
- PNG texture images: **289**
- Unity `.meta` files: **289**
- PNG file size total: **773,316,109 bytes** (about **737.4 MiB**)
- SHA/blob identity comparison against the source folder: **578/578 files matched**, with no name or blob SHA mismatches.

The source and destination inventories are equal; this validates that the texture files and their Unity metadata were copied without changes. These PNGs are image files and may be imported by Godot, but Unity-specific `.meta` data is retained as reference and is not itself Godot metadata. Godot import/runtime verification has not yet been performed.


## Original Unity Material copy workflow

Date: 2026-11-07
Branch: `work/stranded-deep-original`

- Source: `VisualAssets/ExportedProject/Assets/Material/`
- Intended destination: `VisualAssets/OriginalMaterials/UnityMaterials/`
- Workflow: `.github/workflows/copy-original-materials.yml`, copy method `cp -a "$SRC/." "$DEST/"`
- Source inventory: **238 files** — 119 Unity `.mat` and 119 `.meta`
- Source total size: **291,614 bytes**

The workflow and destination README have been committed. Destination copy and source/destination SHA verification are **pending a successful GitHub Actions run**. Unity `.mat` assets are preserved as originals; they are not Godot `.tres` materials. Godot conversion/import/runtime tests have not been performed.
