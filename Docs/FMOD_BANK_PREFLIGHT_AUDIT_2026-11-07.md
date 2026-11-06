# FMOD Bank Preflight Audit — 2026-11-07

## Scope
Repository: `devmyatmin/Project-For-Game`  
Branch: `work/stranded-deep-original`  
Inspected branch head: `ed249ca017fe1717f967c9beb23f44f5ef4586ff`

This is a read-only preflight audit. No game source files, extracted audio files, or project files were modified.

## 1. Bank format and version check

The recursive repository tree contained 6,994 paths. No files were found with these extensions:

- `.bank`
- `.strings.bank`
- `.fsb`
- `.fev`
- `.wem`
- `.bnk`

No `StreamingAssets` path or FMOD-named plugin/library path was found. A repository code search for the exact term `FMOD` returned no matches.

**Result:** No FMOD bank is available in this branch to identify or version-check. This does not prove that the original installed game has no external banks; those files may be outside the repository.

## 2. Audio implementation evidence

The decompiled `DecompiledDLLs/Assembly-CSharp-firstpass/AudioManager.cs` references Unity APIs including `UnityEngine.Audio`, `AudioClip`, `AudioMixer`, `AudioMixerGroup`, and `AudioMixerSnapshot`.

The decompiled `Beam/Audio.cs` plays Unity `AudioClip` assets through the game's audio-player abstraction. `Beam/UAudioMixer.cs` wraps Unity's `AudioMixer`.

**Interpretation:** The inspected game code points to Unity's native audio/mixer system. No FMOD integration was identified in the inspected repository. This is stronger evidence for continuing with Unity AudioClip/AudioMixer extraction than assuming an FMOD-bank pipeline.

## 3. Payload extraction

**Blocked:** No FMOD bank files are present, so no FMOD extractor was run and no bank payloads were extracted.

The separately audited Unity AudioClip copy workflow remains verified: 54 audio payloads (53 OGG and 1 WAV) plus their 54 `.meta` files were copied and checksum-verified. That result is not an FMOD extraction result.

## 4. Event / GUID / bank metadata

**Not applicable yet:** There are no bank files or FMOD event/string-bank metadata in the repository to parse. Unity `.meta` files and AudioMixerController assets are separate Unity metadata and must not be mislabeled as FMOD event/GUID metadata.

## 5. WAV / OGG conversion

**Not run:** Without FMOD payloads there is nothing to convert. Existing OGG/WAV AudioClip files are already encoded audio files; they were not converted by this audit.

## 6. Godot compatibility

**Not tested against FMOD output:** No FMOD output was produced.

The existing Godot-side `AudioManager.cs` uses Godot `AudioStreamPlayer` nodes and accepts `AudioStream` resources. Existing OGG/WAV files can be imported as Godot audio resources, subject to testing in the actual project. Unity mixer routing, snapshots, 3D attenuation, looping, and event behavior still require mapping to Godot's audio buses and playback logic.

## Required input to continue

Provide the original game's audio-bank/resource files if present, especially `.bank`, `.strings.bank`, `.fsb`, or the complete original `StreamingAssets` directory. If none exist, continue with the Unity AudioClip, AudioMixerController, and serialized scene/prefab references instead of an FMOD workflow.

## Final status

| Requested stage | Status |
|---|---|
| Identify bank format/version | Blocked — no bank files available |
| Extract audio payloads | Not run — no bank input |
| Preserve FMOD event/GUID metadata | Not run — no FMOD metadata input |
| Convert to WAV/OGG | Not run — no bank payloads |
| Test Godot compatibility | Existing Unity OGG/WAV path is viable to test; FMOD path untested |

No extraction or conversion success is claimed.
