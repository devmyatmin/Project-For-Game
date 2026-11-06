# Original Unity Material Assets

## Location

- Source: `VisualAssets/ExportedProject/Assets/Material/`
- Project destination: `VisualAssets/OriginalMaterials/UnityMaterials/`
- Workflow: `.github/workflows/copy-original-materials.yml` (uses `cp -a`)
- Branch: `work/stranded-deep-original`

## Source inventory (2026-11-07)

- Unity `.mat` files: 119
- Unity `.meta` files: 119
- Total source files: 238
- Source size total: 291,614 bytes

## Copy status

The copy workflow has been added. Destination copy and source/destination SHA verification are pending a successful GitHub Actions run. These are original Unity serialized material files; they are not converted Godot `.tres` resources. Godot material conversion/import/runtime testing has not been performed.

See [ASSET_AUDIT.md](../ASSET_AUDIT.md) for the combined audit.
