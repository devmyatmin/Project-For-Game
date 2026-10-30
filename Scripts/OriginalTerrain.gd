extends StaticBody3D
class_name OriginalTerrain

const HEIGHTMAP_RESOLUTION := 257
const HEIGHTMAP_SAMPLE_COUNT := HEIGHTMAP_RESOLUTION * HEIGHTMAP_RESOLUTION
const TERRAIN_WIDTH := 125.0
const TERRAIN_HEIGHT := 150.0
const TERRAIN_DEPTH := 500.0
const HEIGHTMAP_ASSET_PATH := "res://VisualAssets/ExportedProject/Assets/TerrainData/Terrain_Story.asset"
const GRASS_TEXTURE_PATH := "res://VisualAssets/ExportedProject/Assets/Texture2D/Terrain_Grass_Dirt_DIFF.png"
const SAND_TEXTURE_PATH := "res://VisualAssets/ExportedProject/Assets/Texture2D/Terrain_Sand_Clean_DIFF.png"
const TERRAIN_SHADER: Shader = preload("res://Shaders/OriginalTerrain.gdshader")

var _heights := PackedFloat32Array()


func _ready() -> void:
    if not _load_original_heightmap():
        push_warning("Original Terrain_Story heightmap could not be loaded; keeping the fallback ground mesh.")
        return
    _build_terrain_mesh()


func get_surface_height_at(world_x: float, world_z: float) -> float:
    if _heights.is_empty():
        # The fallback cylinder's top is 0.3 units above the Ground origin.
        return global_position.y + 0.3

    var local_x: float = clampf(world_x - global_position.x + TERRAIN_WIDTH * 0.5, 0.0, TERRAIN_WIDTH)
    var local_z: float = clampf(world_z - global_position.z + TERRAIN_DEPTH * 0.5, 0.0, TERRAIN_DEPTH)
    var sample_x: float = local_x / TERRAIN_WIDTH * float(HEIGHTMAP_RESOLUTION - 1)
    var sample_z: float = local_z / TERRAIN_DEPTH * float(HEIGHTMAP_RESOLUTION - 1)
    var x0: int = int(floor(sample_x))
    var z0: int = int(floor(sample_z))
    var x1: int = mini(x0 + 1, HEIGHTMAP_RESOLUTION - 1)
    var z1: int = mini(z0 + 1, HEIGHTMAP_RESOLUTION - 1)
    var tx: float = sample_x - float(x0)
    var tz: float = sample_z - float(z0)

    var h00: float = _heights[z0 * HEIGHTMAP_RESOLUTION + x0]
    var h10: float = _heights[z0 * HEIGHTMAP_RESOLUTION + x1]
    var h01: float = _heights[z1 * HEIGHTMAP_RESOLUTION + x0]
    var h11: float = _heights[z1 * HEIGHTMAP_RESOLUTION + x1]
    var top: float = lerpf(h00, h10, tx)
    var bottom: float = lerpf(h01, h11, tx)
    return global_position.y + lerpf(top, bottom, tz)


func _load_original_heightmap() -> bool:
    var source: String = FileAccess.get_file_as_string(HEIGHTMAP_ASSET_PATH)
    if source.is_empty():
        push_error("Unable to read " + HEIGHTMAP_ASSET_PATH)
        return false

    # Unity serializes TerrainData's 16-bit height samples as a hex byte string.
    # World.ImportHeightmapFromRawFile reads the same 257x257 little-endian format.
    var marker := "m_Heights: "
    var marker_index: int = source.find(marker)
    if marker_index < 0:
        push_error("Terrain_Story.asset does not contain m_Heights.")
        return false

    var data_start: int = marker_index + marker.length()
    var data_end: int = source.find("\n", data_start)
    if data_end < 0:
        data_end = source.length()
    var hex_data: String = source.substr(data_start, data_end - data_start).strip_edges()
    if hex_data.length() != HEIGHTMAP_SAMPLE_COUNT * 4:
        push_error("Unexpected Terrain_Story heightmap hex length: " + str(hex_data.length()))
        return false

    var raw_bytes: PackedByteArray = hex_data.hex_decode()
    if raw_bytes.size() != HEIGHTMAP_SAMPLE_COUNT * 2:
        push_error("Unexpected decoded Terrain_Story heightmap byte count: " + str(raw_bytes.size()))
        return false

    _heights.resize(HEIGHTMAP_SAMPLE_COUNT)
    for source_z in range(HEIGHTMAP_RESOLUTION):
        # Match the row flip in World.ImportHeightmapFromRawFile().
        var destination_z: int = HEIGHTMAP_RESOLUTION - 1 - source_z
        for x in range(HEIGHTMAP_RESOLUTION):
            var byte_index: int = (source_z * HEIGHTMAP_RESOLUTION + x) * 2
            var sample: int = int(raw_bytes[byte_index]) | (int(raw_bytes[byte_index + 1]) << 8)
            _heights[destination_z * HEIGHTMAP_RESOLUTION + x] = float(sample) / 65535.0 * TERRAIN_HEIGHT

    return true


func _build_terrain_mesh() -> void:
    var vertices := PackedVector3Array()
    var normals := PackedVector3Array()
    var uvs := PackedVector2Array()
    vertices.resize(HEIGHTMAP_SAMPLE_COUNT)
    normals.resize(HEIGHTMAP_SAMPLE_COUNT)
    uvs.resize(HEIGHTMAP_SAMPLE_COUNT)

    var step_x: float = TERRAIN_WIDTH / float(HEIGHTMAP_RESOLUTION - 1)
    var step_z: float = TERRAIN_DEPTH / float(HEIGHTMAP_RESOLUTION - 1)

    for z in range(HEIGHTMAP_RESOLUTION):
        for x in range(HEIGHTMAP_RESOLUTION):
            var index: int = z * HEIGHTMAP_RESOLUTION + x
            var height: float = _heights[index]
            vertices[index] = Vector3(
                -TERRAIN_WIDTH * 0.5 + float(x) * step_x,
                height,
                -TERRAIN_DEPTH * 0.5 + float(z) * step_z
            )
            # Repeat the original tiling textures approximately every 8 world units.
            uvs[index] = Vector2(float(x) * step_x / 8.0, float(z) * step_z / 8.0)

            var left_x: int = maxi(x - 1, 0)
            var right_x: int = mini(x + 1, HEIGHTMAP_RESOLUTION - 1)
            var back_z: int = maxi(z - 1, 0)
            var front_z: int = mini(z + 1, HEIGHTMAP_RESOLUTION - 1)
            var dx: float = float(right_x - left_x) * step_x
            var dz: float = float(front_z - back_z) * step_z
            var derivative_x: float = (
                _heights[z * HEIGHTMAP_RESOLUTION + right_x]
                - _heights[z * HEIGHTMAP_RESOLUTION + left_x]
            ) / dx
            var derivative_z: float = (
                _heights[front_z * HEIGHTMAP_RESOLUTION + x]
                - _heights[back_z * HEIGHTMAP_RESOLUTION + x]
            ) / dz
            normals[index] = Vector3(-derivative_x, 1.0, -derivative_z).normalized()

    var index_count: int = (HEIGHTMAP_RESOLUTION - 1) * (HEIGHTMAP_RESOLUTION - 1) * 6
    var indices := PackedInt32Array()
    indices.resize(index_count)
    var cursor := 0
    for z in range(HEIGHTMAP_RESOLUTION - 1):
        for x in range(HEIGHTMAP_RESOLUTION - 1):
            var a: int = z * HEIGHTMAP_RESOLUTION + x
            var b: int = a + 1
            var c: int = a + HEIGHTMAP_RESOLUTION
            var d: int = c + 1
            # Clockwise winding as viewed from above, for Godot's front-face convention.
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

    var terrain_mesh := ArrayMesh.new()
    terrain_mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)

    var material := ShaderMaterial.new()
    material.shader = TERRAIN_SHADER
    material.set_shader_parameter("grass_texture", load(GRASS_TEXTURE_PATH))
    material.set_shader_parameter("sand_texture", load(SAND_TEXTURE_PATH))

    var mesh_instance := get_node_or_null("GroundMesh") as MeshInstance3D
    if mesh_instance != null:
        mesh_instance.mesh = terrain_mesh
        mesh_instance.material_override = material

    var collision := get_node_or_null("GroundCollision") as CollisionShape3D
    if collision != null:
        collision.shape = terrain_mesh.create_trimesh_shape()
