# UnityPy Audio Extraction — Verification Record

Date: 2026-11-07
Branch: `work/stranded-deep-original`
Reviewed commit: `44bb8c22e958b94905d9a2aac4d5c03d120cb7e5`

## Executive result

Two different audio workflows must be distinguished:

1. **Original Unity audio folder copy — verified successful.** The separate `Copy Original Unity Audio` workflow copied the existing exported `AudioClip` files and `AudioMixerController` files into `VisualAssets/OriginalAudio/`, verified the copy and SHA-256 checks, and pushed the results to this branch.
2. **UnityPy AudioClip extraction — not yet verified.** No run of `.github/workflows/extract-unity-audio-unitypy.yml` was found in the 74 workflow runs inspected. The expected `VisualAssets/OriginalAudio/UnityPyExtracted/` directory and `UnityPy_audio_manifest.json` are absent from the branch.

Do not describe the UnityPy extraction as completed. The existing original-audio folder copy is a separate successful result.

## Verified: original audio folder copy

Successful workflow run:
- `Copy Original Unity Audio`: [run 42297027404](https://github.com/devmyatmin/Project-For-Game/actions/runs/42297027404)
- The job completed with conclusion `success`.
- Its log shows source and destination counts of 108 files for AudioClip and 6 files for AudioMixerController.
- The source/destination `diff -qr` comparisons passed.
- The script identified two identical payloads and moved them, with their matching `.meta` files, into `UnityAudioClips/Duplicates/`.
- The SHA-256 manifest check completed successfully; the log reports the listed files as `OK`.
- The workflow committed the copied files as `a962ade` (`Copy original Unity audio assets`) and pushed successfully.

A later rerun after the duplicate-folder comparison fix also passed:
- [run 42298141943](https://github.com/devmyatmin/Project-For-Game/actions/runs/42298141943)
- It completed successfully, rechecked the copied files and SHA-256 manifest, and reported that there were no new audio files to commit.

Current branch contents independently confirm:
- Source `VisualAssets/ExportedProject/Assets/AudioClip/`: 108 files — 54 audio payloads (53 OGG and 1 WAV) plus 54 `.meta` files.
- Copied `VisualAssets/OriginalAudio/UnityAudioClips/`: 104 files at the top level, plus 4 files in `Duplicates/`. Together these preserve the 54 payloads and 54 metadata files.
- Copied `VisualAssets/OriginalAudio/UnityAudioMixerControllers/`: 6 files — 3 mixer files and 3 `.meta` files.
- `VisualAssets/OriginalAudio/SHA256SUMS.txt` contains 114 entries matching the 114 copied files (audio + metadata + mixer + metadata).

This proves the copy workflow preserved the contents of those two exported Unity folders. It does not prove that all game audio in bundles, FMOD banks, or other external sources has been recovered, nor that every file has been auditioned or imported into Godot.

## Failed copy-workflow attempts and fix

Some earlier reruns failed because the comparison treated the workflow-created `Duplicates/` quarantine directory as an unexpected source file:
- [run 42297912709](https://github.com/devmyatmin/Project-For-Game/actions/runs/42297912709) failed at the source/destination comparison.
- [run 42296316022](https://github.com/devmyatmin/Project-For-Game/actions/runs/42296316022) and [run 42296287415](https://github.com/devmyatmin/Project-For-Game/actions/runs/42296287415) also failed in the copy/inspection step.

The comparison was corrected in commit `7b9c5ea6543e8c05d411ce0815b374c15342604e` to exclude only the `Duplicates` directory:
`diff -qr --exclude=Duplicates "$CLIPS" "$DEST/UnityAudioClips"`

The successful rerun 42298141943 confirms that fix works with the already-populated destination.

## UnityPy implementation review

The current `Tools/UnityPy/extract_audio.py`:
- Reads `VisualAssets/ExportedProject/Assets/` with UnityPy.
- Iterates Unity `AudioClip` objects and their `.samples`.
- Writes to `VisualAssets/OriginalAudio/UnityPyExtracted/`.
- Writes a per-sample `UnityPy_audio_manifest.json`.
- Returns a failure exit code if no AudioClip objects or no non-empty sample payloads are exported.

However, the current extension detector recognizes WAV, OGG, FSB5, and MP3 signatures only. It has no M4A/MP4 `ftyp` signature check. M4A-like payloads therefore fall through to `.bin`, despite the reviewed commit message and documentation claiming M4A extension detection.

The reviewed commit `44bb8c22e958b94905d9a2aac4d5c03d120cb7e5` triggered the unrelated `Decompile Original Stranded Deep DLLs` workflow, not the UnityPy audio workflow. Its successful conclusion is not evidence of UnityPy extraction.

## Remaining verification

1. Add and test M4A/MP4 signature detection in the UnityPy extractor.
2. Run **Actions → Extract Original Unity Audio with UnityPy → Run workflow** on `work/stranded-deep-original`.
3. Confirm that the UnityPy workflow itself concludes successfully.
4. Inspect `UnityPy_audio_manifest.json` and verify non-empty exported sample files, counts, formats, and hashes.
5. Test representative exported audio files for actual playback and Godot import compatibility.
6. Assess FMOD banks and any audio stored outside the exported Assets tree separately.

## Final status

- **Original Unity AudioClip/AudioMixer folder copy:** successful and verified.
- **UnityPy extraction:** implementation present, but no successful runtime run or manifest confirmed.
- **M4A detection in UnityPy script:** not implemented in the inspected source.
- **Complete recovery of all Stranded Deep audio:** not established.
