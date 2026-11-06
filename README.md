# Project-For-Game

Stranded Deep project repository on branch `work/stranded-deep-original`.

## Unity asset file inventory

Inventory checked against the GitHub repository tree on 2026-11-07.

| Path / extension | Count | Finding |
|---|---:|---|
| `VisualAssets/ExportedProject/Assets/**/*.assets` | 0 | No Unity serialized bundle files with the `.assets` extension found in this branch. |
| `VisualAssets/ExportedProject/Assets/**/*.asset` | 209 | Extracted Unity serialized asset files (`.asset` extension); distinct from `.assets` containers. |
| Unity `Mesh/` inventory (per asset audit) | 164 | Mesh data exists in extracted Unity `.asset` files. These are Unity-serialized meshes, not standalone interchange model files. |
| Standalone model formats (`.fbx`, `.obj`, `.glb`, `.gltf`, `.dae`, `.3ds`, `.blend`, `.mesh`, `.ma`, `.mb`, `.usd`, `.usdz`, `.abc`) | 0 | No files in these model/interchange formats found in the branch tree. |
| `.resS` | 0 | Not found in the repository tree. |
| `.resource` | 0 | Not found in the repository tree. |

### Model availability assessment

**Mesh/model data is present in the extracted project, but standalone original/interchange model files are not present in this branch.** The extracted Mesh files are Unity serialized `.asset` data and cannot be presumed to import directly into Godot. A verified export/conversion step to a Godot-supported model format (such as `.glb`) is still required. Counts above reflect repository file-tree inspection and the existing [`VisualAssets/ASSET_AUDIT.md`](VisualAssets/ASSET_AUDIT.md); individual mesh conversion and Godot import have not been tested.

The original Unity container files commonly named `globalgamemanagers.assets`, `resources.assets`, and `sharedassets*.assets` were not found in the repository tree during this check. This records repository presence only; it does not mean the original game files do not exist elsewhere.

For the extracted VisualAssets category counts and conversion notes, see [`VisualAssets/ASSET_AUDIT.md`](VisualAssets/ASSET_AUDIT.md).
