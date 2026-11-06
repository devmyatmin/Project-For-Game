# NYAMIST Game Project

A separate workspace for assembling the game-related material from this repository into a Godot-oriented project.

## Isolation

- Working branch: `work/nyamist-game-project`
- Base branch: `main`
- This branch is for the NYAMIST Game Project only.
- Do not merge this branch into `main` unless explicitly approved.
- Do not modify or delete existing repository files as part of this project setup.

## Planned structure

```text
NYAMIST_GAME_PROJECT/
├── Godot_Project/       # Godot-ready scenes, scripts, project configuration
├── Game_Assets/         # Models, textures, materials, animations, audio, UI
├── Source_Reference/    # Original Unity/C# logic kept as reference
├── Conversion/          # Import queues, converted files, conversion reports
└── Documentation/       # Catalogs, compatibility notes, build status
```

## Selection rules

1. Include files only after identifying their role and checking their format.
2. Keep original/reference files separate from Godot-ready files.
3. Do not label Unity-native assets as Godot-ready until conversion/import is verified.
4. Preserve source paths and record provenance for copied material.
5. Do not overwrite or delete existing files in the repository.

## Current status

- Project folder initialized.
- Content selection and compatibility audit are pending.
- No original project files have been changed.
