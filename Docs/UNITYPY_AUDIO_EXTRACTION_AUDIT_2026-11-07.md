# UnityPy Audio Extraction — Verification Record

Date: 2026-11-07
Branch: `work/stranded-deep-original`
Reviewed commit: `44bb8c22e958b94905d9a2aac4d5c03d120cb7e5`

## Result

**Not yet verified as a successful audio extraction.** The reviewed commit exists on the dedicated branch, but the GitHub Actions run associated with this push was the unrelated **Decompile Original Stranded Deep DLLs** workflow (run `42298959074`, conclusion: `success`). That success does not validate UnityPy audio extraction.

The UnityPy workflow is configured for manual `workflow_dispatch` and for pushes under `VisualAssets/ExportedProject/Assets/**`. The reviewed commit changes did not trigger it through the Assets-path push filter. No completed UnityPy extraction run or generated manifest was confirmed during this audit.

## Code review finding

The current `Tools/UnityPy/extract_audio.py` implementation recognizes WAV, OGG, FSB5, and MP3 signatures. It does **not** currently contain the M4A/MP4 `ftyp` signature check, despite the reviewed commit message and the documentation claiming M4A extension detection. Such payloads will therefore be named `.bin` at present.

No source code was changed during this audit.

## What is confirmed

- The extractor targets `VisualAssets/ExportedProject/Assets/`.
- It writes to `VisualAssets/OriginalAudio/UnityPyExtracted/`.
- It creates `UnityPy_audio_manifest.json` with per-sample status and SHA-256 information.
- It returns a failure exit code if no AudioClip objects or no non-empty sample payloads are exported.
- The workflow installs UnityPy and invokes the extractor on the dedicated branch.

## Remaining verification

1. Correct the M4A signature detection mismatch in the extractor.
2. Run **Actions → Extract Original Unity Audio with UnityPy → Run workflow** on `work/stranded-deep-original`.
3. Confirm the run concludes successfully.
4. Inspect `UnityPy_audio_manifest.json` and verify that actual audio files were committed under `VisualAssets/OriginalAudio/UnityPyExtracted/`.
5. Check exported samples for playable audio and record counts; a successful script exit alone does not prove complete recovery of every game sound or FMOD bank.

## Conclusion

Status: **Implementation present; runtime extraction not yet proven; M4A detection discrepancy found.** Do not mark the Stranded Deep original-audio extraction as complete until the workflow output and manifest have been inspected.
