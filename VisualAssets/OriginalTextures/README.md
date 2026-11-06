# Original Unity Texture2D Assets

## Location

- Source: `VisualAssets/ExportedProject/Assets/Texture2D/`
- Project destination: `VisualAssets/OriginalTextures/UnityTexture2D/`
- Workflow: `.github/workflows/copy-original-textures.yml` (uses `cp -a`)
- Branch: `work/stranded-deep-original`

## Verified inventory (2026-11-07)

- PNG textures: 289
- Unity `.meta` files: 289
- Total files: 578
- PNG byte size total: 773,316,109 bytes (approximately 737.4 MiB)
- Comparison against source: all 578 file names and Git blob SHAs match; no mismatches found.

The PNGs are original extracted texture images. The Unity `.meta` files are preserved for reference; Godot does not use them as native import metadata. Godot import/runtime checks have not yet been performed.

See [ASSET_AUDIT.md](../ASSET_AUDIT.md) for the combined audit and copy verification.
