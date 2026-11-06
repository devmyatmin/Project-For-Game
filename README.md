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
| `.overrideController` files | 0 | No Animator Override Controller files found. |
| `.resS` | 0 | Not found in the repository tree. |
| `.resource` | 0 | Not found in the repository tree. |

### Textures and materials assessment

**Texture and Material assets are present in the repository's extracted Unity project.** Texture names in the tree include examples such as `Anchor1_albedo.png`, `Anchor1_NRM.png`, `BUOY_DIFF.png`, `BUOY_NRM.png`, `Terrain_DIFF.png`, and `Terrain_NRM.png`. The materials are Unity `.mat` files; their shader/property setup needs conversion or recreation in Godot. Texture availability does not itself verify that every mesh's material/texture references are intact, nor that the textures/materials import correctly in Godot. This was a repository tree/file-name inventory, not a runtime or conversion test.

### Model availability assessment

**Mesh/model data is present in the extracted project, but standalone original/interchange model files are not present in this branch.** The extracted Mesh files are Unity serialized `.asset` data and cannot be presumed to import directly into Godot. A verified export/conversion step to a Godot-supported model format (such as `.glb`) is still required. Counts above reflect repository file-tree inspection and the existing [`VisualAssets/ASSET_AUDIT.md`](VisualAssets/ASSET_AUDIT.md); individual mesh conversion and Godot import have not been tested.

### Animation availability assessment

**Animation data is present in the repository as Unity-native clips and Animator Controller state-machine assets.** The tree inventory found 45 `.anim` clips (each with a matching `.anim.meta`) and 4 `.controller` files. Clip examples in the repository include aircraft crash/takeoff, eel idle/swimming/attack/damage/death, great-white shark swim/bite/damage/turning, seaplane cockpit actions, and squid attack/swim/damage/death clips. The four controllers are `Boss_Shark_Controller.controller`, `EelController.controller`, `Seaplane Animator.controller`, and `SquidController.controller`.

The Eel controller file itself contains states, transitions, parameters (including Speed, Direction, TakeDamage, Attack, Dead), and Blend Trees referencing animation clips by Unity GUID. This supports that controller/state-machine data exists, not only standalone clips. However, the audit does not verify that every GUID resolves to an included clip, that rig/skeleton dependencies are complete, or that animations play correctly. Unity `.anim` and `.controller` formats are not directly Godot-ready; clip curves, skeleton/rig mapping, Blend Trees, and transitions need conversion/reconstruction (for example into Godot AnimationPlayer/AnimationTree). No Godot animation import/playback test has been performed.

The original Unity container files commonly named `globalgamemanagers.assets`, `resources.assets`, and `sharedassets*.assets` were not found in the repository tree during this check. This records repository presence only; it does not mean the original game files do not exist elsewhere.

For the extracted VisualAssets category counts and conversion notes, see [`VisualAssets/ASSET_AUDIT.md`](VisualAssets/ASSET_AUDIT.md).
