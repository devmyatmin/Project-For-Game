# Stranded Deep Logic Port (Godot)

This directory contains an original GDScript implementation of gameplay rules described by the repository's source-level audit. It is a logic-only runtime test layer: it does not import Unity scenes, models, textures, shaders, audio, UI, or networking.

## Modules

- `survival_stats.gd`: health, calories/hunger reserve, fluids/thirst reserve, oxygen, drowning grace timer and timed effect storage.
- `inventory_logic.gd`: bounded slot storage, stack capacity, add/remove and persistence data.
- `crafting_logic.gd`: recipe validation, ingredient checks and output capacity validation.
- `construction_logic.gd`: support, underwater and connector/proximity acceptance checks.
- `wildlife_logic.gd`: explicit animal state machine and health/death transition.
- `vehicle_logic.gd`: generic configurable vehicle motion, steering, fuel gate and enter/exit state.
- `save_load_logic.gd`: versioned JSON state encode/decode and file IO.
- `player_logic.gd`: player component composition, held-item validation and state snapshot.

## Fidelity boundaries

The audit establishes a 700-point health/calorie/fluid scale, 100-point oxygen, oxygen drain/regain defaults of 3.5/40, a 15-second drowning timer, normal/hard hunger and thirst depletion periods, and health regeneration thresholds. These are represented as testable baseline rules.

This is not a line-by-line or binary-compatible translation of the game's C# code. Unity physics, animation, multiplayer replication, UI, save container formats, species-specific AI, individual vehicle classes, exact recipe catalogue, and all method call chains are not represented yet. Vehicle motion and wildlife transitions here are generic adapters, not claims of exact original-game parity. Unknown behavior is left configurable rather than guessed.

## Run tests locally

```sh
godot --headless --path . --script res://GodotLogic/tests/run_tests.gd
```

GitHub Actions runs the same Godot headless test harness on pushes to `work/godot-logic-runtime`.
