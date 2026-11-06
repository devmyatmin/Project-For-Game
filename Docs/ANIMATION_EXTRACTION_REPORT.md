# Stranded Deep — Original Unity Animation Extraction Report

## Status
- **Workflow:** Copy Original Unity Animations
- **Result:** SUCCESS
- **Workflow run ID:** 42295582545
- **Run URL:** https://github.com/devmyatmin/Project-For-Game/actions/runs/42295582545
- **Branch:** `work/stranded-deep-original`
- **Workflow definition commit:** `dd8d8009acd08371180a54ab375c9ab575d2cd92`
- **Asset copy commit:** `7863a21` (full SHA recorded in GitHub history)
- **Job:** `copy-original-animations` (job ID 128159840242)

## Source and destination
Source inside exported Unity project:
- `VisualAssets/ExportedProject/Assets/AnimationClip/`
- `VisualAssets/ExportedProject/Assets/AnimatorController/`
- `VisualAssets/ExportedProject/Assets/Avatar/`

Copied into dedicated separated folders:
- `VisualAssets/OriginalAnimations/UnityAnimationClips/`
- `VisualAssets/OriginalAnimations/UnityAnimatorControllers/`
- `VisualAssets/OriginalAnimations/UnityAvatars/`

## Verified copy counts
| Asset class | Native asset files | .meta files | Total |
|---|---:|---:|---:|
| Unity Animation Clips (.anim) | 45 | 45 | 90 |
| Animator Controllers (.controller) | 4 | 4 | 8 |
| Avatars (.asset) | 4 | 4 | 8 |
| **Total** | **53** | **53** | **106** |

- Workflow log reported copied folder size: **97M** (du -sh).
- Git commit recorded **106 files changed**, with **1,984,342 insertions**.
- Git push completed on first synchronization attempt.

## Inspection notes
The copied clip names include animation groups for Eel, great white shark (GW), Squid, Seaplane, Aircraft, pilot/cockpit, and interaction clips (examples: `Eel_Swim_Forward_Animation.anim`, `GW_Bite.anim`, `Squid_Swim.anim`, `Seaplane_Enter_Cockpit.anim`, `Aircraft_TakeOff.anim`).
Controllers copied include `Boss_Shark_Controller.controller`, `EelController.controller`, `Seaplane Animator.controller`, and `SquidController.controller`.
Avatars copied include Eel, GW, Seaplane, and Squid avatar assets.

## Limits of verification
This report verifies workflow success, source presence, copied file counts, retained metadata files, commit, and push. It **does not** certify that Unity animation curves/controllers are already converted to Godot Animation/AnimationTree, that skeleton paths are compatible, or that animations play correctly in Godot. That requires a separate conversion/import and runtime validation stage.
