extends CharacterBody3D
class_name PlayerController

@export var move_speed: float = 4.0
@export var jump_velocity: float = 4.5
@export var gravity_multiplier: float = 1.0
@export var look_sensitivity: float = 1.0

var _gravity: float = 9.8
var _camera_pivot: Node3D
var _camera: Camera3D
var _terrain: OriginalTerrain
var _mobile_controls: MobileControls
var _jump_requested := false
var _is_crouching := false
var _is_running := false
var _camera_pitch := 0.0
var _inventory: Dictionary = {}

func _ready() -> void:
    _gravity = float(ProjectSettings.get_setting("physics/3d/default_gravity", 9.8))
    _camera_pivot = get_node_or_null("CameraPivot") as Node3D
    if _camera_pivot:
        _camera = _camera_pivot.get_node_or_null("Camera3D") as Camera3D
    _terrain = get_node_or_null("../Ground") as OriginalTerrain
    _mobile_controls = get_tree().root.find_child("MobileControls", true, false) as MobileControls
    if _mobile_controls:
        _mobile_controls.jump_requested.connect(_on_jump_requested)
        _mobile_controls.crouch_changed.connect(_on_crouch_changed)
        _mobile_controls.run_changed.connect(_on_run_changed)
        _mobile_controls.interact_requested.connect(_on_interact_requested)

func _unhandled_input(event: InputEvent) -> void:
    if event is InputEventKey and event.pressed and event.keycode == KEY_ESCAPE:
        get_tree().quit()

func _physics_process(delta: float) -> void:
    var input_vector := Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
    if _mobile_controls and _mobile_controls.movement_input.length() > 0.08:
        input_vector = _mobile_controls.movement_input
    var direction := (global_transform.basis * Vector3(input_vector.x, 0.0, input_vector.y)).normalized()
    var current_speed := move_speed * (0.55 if _is_crouching else (1.65 if _is_running else 1.0))
    if direction != Vector3.ZERO:
        velocity.x = direction.x * current_speed
        velocity.z = direction.z * current_speed
    else:
        velocity.x = 0.0
        velocity.z = 0.0

    if _mobile_controls:
        var look: Vector2 = _mobile_controls.consume_look_delta() * look_sensitivity
        if look != Vector2.ZERO:
            rotate_y(-look.x)
            _camera_pitch = clampf(_camera_pitch - look.y, -1.15, 1.15)
            if _camera_pivot:
                _camera_pivot.rotation = Vector3(_camera_pitch, 0.0, 0.0)

    if is_on_floor():
        if velocity.y < 0.0:
            velocity.y = 0.0
        if Input.is_action_just_pressed("ui_accept") or _jump_requested:
            velocity.y = jump_velocity
    else:
        velocity.y -= _gravity * gravity_multiplier * delta
    _jump_requested = false
    move_and_slide()

    # Safety net: the CharacterBody origin is at the player's feet (the capsule
    # is offset upward by half its height). Do not wait until the body is half
    # underground before recovering. This also keeps the player above the
    # fallback cylinder if the original Unity heightmap cannot be read.
    if _terrain != null:
        var terrain_y := _terrain.get_surface_height_at(global_position.x, global_position.z)
        if global_position.y < terrain_y + 0.05:
            global_position.y = terrain_y + 0.05
            velocity.y = maxf(velocity.y, 0.0)
            velocity.x = 0.0
            velocity.z = 0.0

func _on_jump_requested() -> void:
    _jump_requested = true

func _on_interact_requested() -> void:
    if not _camera:
        return
    var from := _camera.global_position
    var to := from - _camera.global_basis.z * 3.5
    var query := PhysicsRayQueryParameters3D.create(from, to)
    query.collide_with_areas = true
    var hit := get_world_3d().direct_space_state.intersect_ray(query)
    if hit.is_empty() or not hit.has("collider"):
        return
    var pickup := hit["collider"] as CollectibleResource
    if not pickup:
        return
    var item_id: String = pickup.item_id
    var amount: int = pickup.amount
    var item_name: String = pickup.display_name
    var total: int = int(_inventory.get(item_id, 0)) + amount
    if pickup.try_collect():
        _inventory[item_id] = total
        if _mobile_controls:
            _mobile_controls.set_inventory_status(item_name, amount, total)

func _on_run_changed(is_running: bool) -> void:
    _is_running = is_running

func _on_crouch_changed(is_crouching: bool) -> void:
    _is_crouching = is_crouching
    if _camera_pivot:
        _camera_pivot.position.y = 1.05 if _is_crouching else 1.55
