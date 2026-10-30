extends Node3D
class_name WorldBootstrap

const PALM_TREE_SCENE: PackedScene = preload("res://Scenes/Environment/PalmTree.tscn")
const PLAYER_SPAWN_X := 0.0
const PLAYER_SPAWN_Z := 0.0
const PLAYER_SPAWN_CLEARANCE := 2.0

var _terrain: OriginalTerrain


func _ready() -> void:
    _terrain = get_node_or_null("../Ground") as OriginalTerrain
    call_deferred("_initialize_world")


func _initialize_world() -> void:
    # Wait for terrain generation and the physics server to register its collision mesh.
    var placed := false
    for attempt in range(4):
        await get_tree().physics_frame
        if _place_player_on_terrain():
            placed = true
            break
    if not placed:
        push_warning("Player spawn ray did not hit terrain; using the sampled height as a fallback.")
        var player := get_node_or_null("../Player") as CharacterBody3D
        if player != null:
            player.global_position = Vector3(
                PLAYER_SPAWN_X,
                _surface_y(PLAYER_SPAWN_X, PLAYER_SPAWN_Z) + PLAYER_SPAWN_CLEARANCE,
                PLAYER_SPAWN_Z
            )

    _spawn_palm_trees()
    _spawn_pickup("stick", "Dry Stick", Vector3(-1.0, 0.0, -2.6), false)
    _spawn_pickup("rock", "Rock", Vector3(1.0, 0.0, -3.2), true)
    _spawn_pickup("stick", "Dry Stick", Vector3(0.0, 0.0, -4.2), false)
    _spawn_pickup("stick", "Dry Stick", Vector3(-2.2, 0.0, -1.2), false)
    _spawn_pickup("stick", "Dry Stick", Vector3(2.0, 0.0, -0.8), false)
    _spawn_pickup("stick", "Dry Stick", Vector3(1.7, 0.0, 2.0), false)


func _place_player_on_terrain() -> bool:
    var player := get_node_or_null("../Player") as CharacterBody3D
    if player == null:
        return false

    # Raycast against the actual generated collision, instead of trusting a
    # heightmap lookup alone. Start well above the island to avoid spawning
    # inside the ground if the heightmap/collision orientation differs.
    var origin := Vector3(PLAYER_SPAWN_X, 200.0, PLAYER_SPAWN_Z)
    var destination := Vector3(PLAYER_SPAWN_X, -100.0, PLAYER_SPAWN_Z)
    var query := PhysicsRayQueryParameters3D.create(origin, destination)
    query.exclude = [player.get_rid()]
    query.collide_with_areas = false
    var hit := get_world_3d().direct_space_state.intersect_ray(query)
    if hit.is_empty() or not hit.has("position"):
        return false

    player.global_position = hit["position"] + Vector3(0.0, PLAYER_SPAWN_CLEARANCE, 0.0)
    player.velocity = Vector3.ZERO
    player.floor_snap_length = 0.35
    player.safe_margin = 0.02
    return true


func _surface_y(x: float, z: float) -> float:
    if _terrain != null:
        return _terrain.get_surface_height_at(x, z)
    return 0.0


func _spawn_palm_trees() -> void:
    # Keep trees away from the initial pickup cluster and leave a clear spawn area.
    var tree_positions: Array[Vector3] = [
        Vector3(-15.0, 0.0, -13.0),
        Vector3(14.0, 0.0, -15.0),
        Vector3(-19.0, 0.0, 4.0),
        Vector3(19.0, 0.0, 5.0),
        Vector3(-10.0, 0.0, 18.0),
        Vector3(9.0, 0.0, 19.0)
    ]
    for index in range(tree_positions.size()):
        var tree := PALM_TREE_SCENE.instantiate() as Node3D
        var tree_position: Vector3 = tree_positions[index]
        tree.position = Vector3(
            tree_position.x,
            _surface_y(tree_position.x, tree_position.z),
            tree_position.z
        )
        tree.rotation.y = float(index) * 0.91
        add_child(tree)


func _spawn_pickup(item_id: String, display_name: String, item_position: Vector3, is_rock: bool) -> void:
    var pickup := CollectibleResource.new()
    pickup.name = display_name.replace(" ", "") + "Pickup" + str(get_child_count())
    pickup.item_id = item_id
    pickup.display_name = display_name
    pickup.amount = 1
    var pickup_height: float = 0.28 if is_rock else 0.04
    pickup.position = Vector3(
        item_position.x,
        _surface_y(item_position.x, item_position.z) + pickup_height,
        item_position.z
    )

    var shape := CollisionShape3D.new()
    if is_rock:
        var sphere_shape := SphereShape3D.new()
        sphere_shape.radius = 0.28
        shape.shape = sphere_shape
    else:
        # Broad, low pickup collider covers the small pile of dry branches.
        var box_shape := BoxShape3D.new()
        box_shape.size = Vector3(0.62, 0.14, 0.62)
        shape.shape = box_shape
    pickup.add_child(shape)

    if is_rock:
        var mesh_instance := MeshInstance3D.new()
        var sphere_mesh := SphereMesh.new()
        sphere_mesh.radius = 0.28
        sphere_mesh.height = 0.56
        mesh_instance.mesh = sphere_mesh
        mesh_instance.position = Vector3(0.0, 0.05, 0.0)
        var rock_material := StandardMaterial3D.new()
        rock_material.albedo_color = Color(0.38, 0.39, 0.35)
        rock_material.roughness = 0.95
        mesh_instance.material_override = rock_material
        pickup.add_child(mesh_instance)
    else:
        _build_dry_stick_pile(pickup)

    add_child(pickup)


func _build_dry_stick_pile(pickup: CollectibleResource) -> void:
    # The original export has no standalone portable-stick mesh, so build a
    # small branch bundle from tapered-looking low-poly cylinders rather than
    # using the old rectangular box placeholder.
    var stick_specs: Array[Dictionary] = [
        {"position": Vector3(-0.12, 0.045, -0.08), "rotation": Vector3(0.08, 0.12, 1.18), "length": 0.68, "radius": 0.035},
        {"position": Vector3(0.08, 0.055, 0.06), "rotation": Vector3(-0.12, 0.0, -0.78), "length": 0.74, "radius": 0.04},
        {"position": Vector3(0.03, 0.09, -0.02), "rotation": Vector3(0.18, 0.22, 0.28), "length": 0.56, "radius": 0.03},
        {"position": Vector3(-0.04, 0.12, 0.10), "rotation": Vector3(-0.16, 0.15, 1.52), "length": 0.43, "radius": 0.026}
    ]
    var wood_colors: Array[Color] = [
        Color(0.34, 0.22, 0.12),
        Color(0.43, 0.29, 0.16),
        Color(0.29, 0.19, 0.11),
        Color(0.48, 0.34, 0.20)
    ]

    for index in range(stick_specs.size()):
        var spec: Dictionary = stick_specs[index]
        var twig := MeshInstance3D.new()
        twig.name = "DryTwig" + str(index + 1)
        var twig_mesh := CylinderMesh.new()
        twig_mesh.top_radius = float(spec["radius"]) * 0.72
        twig_mesh.bottom_radius = float(spec["radius"])
        twig_mesh.height = float(spec["length"])
        twig_mesh.radial_segments = 6
        twig_mesh.rings = 1
        twig.mesh = twig_mesh
        twig.position = spec["position"]
        twig.rotation = spec["rotation"]
        var wood_material := StandardMaterial3D.new()
        wood_material.albedo_color = wood_colors[index]
        wood_material.roughness = 1.0
        wood_material.metallic = 0.0
        twig.material_override = wood_material
        pickup.add_child(twig)
