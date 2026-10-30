extends Node3D
class_name WorldMapBuilder

## Godot-side reconstruction of Stranded Deep's 7x7 zone layout.
## Zone spacing/size follows StrandedWorld.cs (500 * 1.25 = 625 units).
## Terrain_Story is reserved for the starting island; other zones use deterministic
## low-resolution terrain because no importable Unity terrain meshes/map bundle were found.
const ZONE_COUNT := 7
const ZONE_SPACING := 625.0
const ZONE_SIZE := 250.0
const SEA_LEVEL := 0.0
const OTHER_RESOLUTION := 65
const GRASS_TEXTURE_PATH := "res://VisualAssets/ExportedProject/Assets/Texture2D/Terrain_Grass_Dirt_DIFF.png"
const SAND_TEXTURE_PATH := "res://VisualAssets/ExportedProject/Assets/Texture2D/Terrain_Sand_Clean_DIFF.png"
const TERRAIN_SHADER: Shader = preload("res://Shaders/OriginalTerrain.gdshader")

@export var world_seed: int = 2026
@export var enable_far_zone_collisions: bool = false

var _player: Node3D
var _terrain_material: ShaderMaterial


func _ready() -> void:
	_player = get_node_or_null("../Player") as Node3D
	_terrain_material = _make_terrain_material()
	_build_zones()


func _make_terrain_material() -> ShaderMaterial:
	var material := ShaderMaterial.new()
	material.shader = TERRAIN_SHADER
	var grass := load(GRASS_TEXTURE_PATH) as Texture2D
	var sand := load(SAND_TEXTURE_PATH) as Texture2D
	if grass != null:
		material.set_shader_parameter("grass_texture", grass)
	if sand != null:
		material.set_shader_parameter("sand_texture", sand)
	return material


func _build_zones() -> void:
	# Zone 0 is the original Terrain_Story island already present in Main.tscn.
	# Keep its transform at the origin so the player start and pickups stay valid.
	for row in range(ZONE_COUNT):
		for column in range(ZONE_COUNT):
			if row == ZONE_COUNT / 2 and column == ZONE_COUNT / 2:
				continue
			var zone_index := row * ZONE_COUNT + column
			var zone := StaticBody3D.new()
			zone.name = "Zone_%02d" % zone_index
			var jitter_rng := RandomNumberGenerator.new()
			jitter_rng.seed = int(world_seed + zone_index * 7919)
			var x := (column - 3) * ZONE_SPACING + jitter_rng.randf_range(-42.0, 42.0)
			var z := (row - 3) * ZONE_SPACING + jitter_rng.randf_range(-42.0, 42.0)
			zone.position = Vector3(x, 0.0, z)
			add_child(zone)
			var island_roll := int(posmod(zone_index * 37 + world_seed * 13, 100))
			var biome := "island" if island_roll < 43 else "sea"
			if zone_index in [10, 24, 38]:
				biome = "rock"
			_build_zone_mesh(zone, zone_index, biome)


func _build_zone_mesh(zone: StaticBody3D, zone_index: int, biome: String) -> void:
	var resolution := OTHER_RESOLUTION
	var sample_count := resolution * resolution
	var heights := PackedFloat32Array()
	heights.resize(sample_count)
	var noise := FastNoiseLite.new()
	noise.seed = int(world_seed + zone_index * 104729)
	noise.noise_type = FastNoiseLite.TYPE_SIMPLEX
	noise.frequency = 0.018
	noise.fractal_octaves = 4
	noise.fractal_lacunarity = 2.0
	noise.fractal_gain = 0.48

	var rng := RandomNumberGenerator.new()
	rng.seed = int(world_seed ^ (zone_index * 48611))
	var radius_x := rng.randf_range(65.0, 100.0)
	var radius_z := rng.randf_range(65.0, 103.0)
	var peak := rng.randf_range(24.0, 52.0)
	if biome == "rock":
		radius_x = rng.randf_range(48.0, 75.0)
		radius_z = rng.randf_range(48.0, 78.0)
		peak = rng.randf_range(42.0, 68.0)

	for z in range(resolution):
		for x in range(resolution):
			var local_x := -ZONE_SIZE * 0.5 + float(x) / float(resolution - 1) * ZONE_SIZE
			var local_z := -ZONE_SIZE * 0.5 + float(z) / float(resolution - 1) * ZONE_SIZE
			var noise_value := noise.get_noise_2d(local_x + zone.position.x, local_z + zone.position.z)
			var sea_floor := -15.0 + noise_value * 3.0
			var height := sea_floor
			if biome != "sea":
				var ellipse_distance := sqrt(pow(local_x / radius_x, 2.0) + pow(local_z / radius_z, 2.0))
				var land_mask := 1.0 - smoothstep(0.72, 1.12, ellipse_distance)
				var broad_shape := pow(maxf(land_mask, 0.0), 0.72)
				var detail := noise_value * (2.0 + broad_shape * 5.0)
				height = sea_floor + broad_shape * peak + detail
				if biome == "rock":
					height += pow(broad_shape, 1.7) * 12.0
			heights[z * resolution + x] = height

	var vertices := PackedVector3Array()
	var normals := PackedVector3Array()
	var uvs := PackedVector2Array()
	vertices.resize(sample_count)
	normals.resize(sample_count)
	uvs.resize(sample_count)
	var step := ZONE_SIZE / float(resolution - 1)
	for z in range(resolution):
		for x in range(resolution):
			var index := z * resolution + x
			var h := heights[index]
			vertices[index] = Vector3(-ZONE_SIZE * 0.5 + float(x) * step, h, -ZONE_SIZE * 0.5 + float(z) * step)
			uvs[index] = Vector2(float(x) * step / 8.0, float(z) * step / 8.0)
			var left_x := maxi(x - 1, 0)
			var right_x := mini(x + 1, resolution - 1)
			var back_z := maxi(z - 1, 0)
			var front_z := mini(z + 1, resolution - 1)
			var dx := float(right_x - left_x) * step
			var dz := float(front_z - back_z) * step
			var dhx := (heights[z * resolution + right_x] - heights[z * resolution + left_x]) / dx
			var dhz := (heights[front_z * resolution + x] - heights[back_z * resolution + x]) / dz
			normals[index] = Vector3(-dhx, 1.0, -dhz).normalized()

	var indices := PackedInt32Array()
	indices.resize((resolution - 1) * (resolution - 1) * 6)
	var cursor := 0
	for z in range(resolution - 1):
		for x in range(resolution - 1):
			var a := z * resolution + x
			var b := a + 1
			var c := a + resolution
			var d := c + 1
			indices[cursor] = a
			indices[cursor + 1] = c
			indices[cursor + 2] = d
			indices[cursor + 3] = a
			indices[cursor + 4] = d
			indices[cursor + 5] = b
			cursor += 6

	var arrays: Array = []
	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = vertices
	arrays[Mesh.ARRAY_NORMAL] = normals
	arrays[Mesh.ARRAY_TEX_UV] = uvs
	arrays[Mesh.ARRAY_INDEX] = indices
	var mesh := ArrayMesh.new()
	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)

	var instance := MeshInstance3D.new()
	instance.name = "TerrainMesh"
	instance.mesh = mesh
	instance.material_override = _terrain_material
	instance.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_ON if biome != "sea" else GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	zone.add_child(instance)

	# Sea-floor zones are visible below water but need no physics collision.
	# Island zones use matching mesh collision so the player can walk on them.
	if biome != "sea" or enable_far_zone_collisions:
		var collision := CollisionShape3D.new()
		collision.name = "TerrainCollision"
		collision.shape = mesh.create_trimesh_shape()
		zone.add_child(collision)
