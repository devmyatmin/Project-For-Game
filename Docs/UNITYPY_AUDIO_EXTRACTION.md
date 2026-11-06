# UnityPy AudioClip Extraction

## Workflow
- GitHub Actions workflow: `.github/workflows/extract-unity-audio-unitypy.yml`
- Extractor: `Tools/UnityPy/extract_audio.py`
- Input: `VisualAssets/ExportedProject/Assets/`
- Output: `VisualAssets/OriginalAudio/UnityPyExtracted/`

Run it manually from **Actions → Extract Original Unity Audio with UnityPy → Run workflow**. It also runs when files under the exported Unity Assets tree are pushed to the dedicated branch.

The script scans Unity serialized assets and AssetBundles loaded by UnityPy, then exports each AudioClip's `.samples` payload. UnityPy normally converts samples to WAV bytes; already encoded OGG/WAV/M4A payloads retain their matching extension. Each output name includes its Unity path ID to avoid same-name collisions. SHA-256-identical payloads are reported in the JSON manifest rather than duplicated.

## Output and verification
The run writes audio payloads and `UnityPy_audio_manifest.json` to the output directory. The manifest records AudioClip path IDs, clip names, statuses, file names, byte sizes, and SHA-256 hashes. The workflow commits generated files back to `work/stranded-deep-original`.

This workflow is an extraction implementation, not proof that every game audio asset has been recovered. AudioClip objects without embedded/external samples, unsupported FMOD banks, or audio data outside the exported Assets tree may need a separate pass. Check the Actions run and manifest after the first manual run.
