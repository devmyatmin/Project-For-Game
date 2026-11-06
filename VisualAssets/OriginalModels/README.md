# Original Unity Mesh Assets

## Location

This directory stores the copied original Unity Mesh serialization inside the Godot project repository.

- Source: `VisualAssets/ExportedProject/Assets/Mesh/`
- Stored here: `VisualAssets/OriginalModels/UnityMeshAssets/`
- Copy workflow: GitHub Actions workflow `.github/workflows/copy-original-models.yml`, using `cp -a`
- Branch: `work/stranded-deep-original`

## Recorded output (2026-11-07)

- Total files: 328
- `.asset` Mesh files: 164
- `.meta` files: 164
- Mesh asset bytes: 42,432,241 (approximately 40.5 MiB)

A sample inspection of `Buoy.asset` found Unity Mesh serialization with readable vertex data and an index buffer (6,146 vertices; 14,085 indices). The folder therefore contains mesh data rather than only metadata.

## Important

The files are **original Unity serialized assets**, not converted Godot models. They are retained for reference and future conversion. They have not yet been confirmed importable by Godot. Put any future GLB/GLTF/OBJ conversions in a separate folder and preserve this original set unchanged.

See [ASSET_AUDIT.md](../ASSET_AUDIT.md) for the broader asset audit and verification record.
