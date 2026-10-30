extends Node3D
class_name WorldBootstrap

const PALM_TREE_SCENE: PackedScene = preload("res://Scenes/Environment/PalmTree.tscn")

var _terrain: OriginalTerrain


func _ready() -> void:
    _terrain = get_node_or_null("../Ground") as OriginalTerrain
    call_deferred("_place_player_on_terrain")
    _spawn_palm_trees()
    _spawn_pickup("stick", "Stick", Vector3(-1.0, 0.12, -2.6), false)
    _spawn_pickup("rock", "Rock", Vector3(1.0, 0.16, -3.2), true)
    _spawn_pickup("stick", "Stick", Vector3(0.0, 0.12, -4.2), false)


func _place_player_on_terrain() -> void:
    var player := get_node_or_null("../Player") as CharacterBody3D
    if player == null:
        return
    # Raise the spawn point so the camera and capsule settle onto the terrain instead of starting partially embedded.
    player.position.y = _surface_y(player.position.x, player.position.z) + 0.85


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
    pickup.name = display_name + "Pickup"
    pickup.item_id = item_id
    pickup.display_name = display_name
    pickup.amount = 1
    var pickup_height: float = 0.28 if is_rock else 0.06
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
        var box_shape := BoxShape3D.new()
        box_shape.size = Vector3(0.18, 0.12, 0.65)
        shape.shape = box_shape
    pickup.add_child(shape)

    var mesh_instance := MeshInstance3D.new()
    if is_rock:
        var sphere_mesh := SphereMesh.new()
        sphere_mesh.radius = 0.28
        sphere_mesh.height = 0.56
        mesh_instance.mesh = sphere_mesh
        mesh_instance.position = Vector3(0, 0.05, 0)
    else:
        var box_mesh := BoxMesh.new()
        box_mesh.size = Vector3(0.18, 0.12, 0.65)
        mesh_instance.mesh = box_mesh
        mesh_instance.rotation = Vector3(0.08, 0.4, 0.18)
    var material := StandardMaterial3D.new()
    material.albedo_color = Color(0.38, 0.39, 0.35) if is_rock else Color(0.42, 0.28, 0.15)
    material.roughness = 0.95
    mesh_instance.material_override = material
    pickup.add_child(mesh_instance)
    add_child(pickup)
