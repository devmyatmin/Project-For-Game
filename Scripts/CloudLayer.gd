extends MeshInstance3D

## Uses the original Unity MostlyCloud texture in a low-cost scrolling cloud sheet.
@export var cloud_height: float = 185.0
@export var cloud_size: float = 900.0
@export var follow_path: NodePath = NodePath("../Player")
@export_range(0.05, 0.8, 0.01) var opacity: float = 0.42
@export_range(0.5, 8.0, 0.1) var texture_scale: float = 3.0

const CLOUD_TEXTURE: Texture2D = preload("res://VisualAssets/ExportedProject/Assets/Texture2D/MostlyCloud.png")

var _follow_target: Node3D


func _ready() -> void:
	_follow_target = get_node_or_null(follow_path) as Node3D
	var plane := PlaneMesh.new()
	plane.size = Vector2(cloud_size, cloud_size)
	mesh = plane
	cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	material_override = _make_cloud_material()


func _process(_delta: float) -> void:
	if _follow_target != null:
		global_position = Vector3(_follow_target.global_position.x, cloud_height, _follow_target.global_position.z)
	else:
		global_position.y = cloud_height


func _make_cloud_material() -> ShaderMaterial:
	var shader := Shader.new()
	shader.code = """
shader_type spatial;
render_mode unshaded, blend_mix, cull_disabled, depth_draw_never;

uniform sampler2D cloud_texture : source_color, repeat_enable;
uniform vec4 cloud_tint : source_color = vec4(0.56, 0.83, 0.84, 1.0);
uniform float cloud_opacity = 0.42;
uniform float texture_scale = 3.0;
uniform float drift_speed = 0.0025;

void fragment() {
	vec2 cloud_uv = UV * texture_scale + vec2(TIME * drift_speed, TIME * drift_speed * 0.27);
	vec4 texel = texture(cloud_texture, cloud_uv);
	float luminance = dot(texel.rgb, vec3(0.299, 0.587, 0.114));
	float cloud_shape = smoothstep(0.36, 0.78, luminance);
	float cloud_alpha = max(texel.a * cloud_shape, cloud_shape * 0.28);
	ALBEDO = cloud_tint.rgb * mix(0.82, 1.08, luminance);
	ALPHA = cloud_alpha * cloud_opacity * cloud_tint.a;
}
"""
	var material := ShaderMaterial.new()
	material.shader = shader
	material.set_shader_parameter("cloud_texture", CLOUD_TEXTURE)
	material.set_shader_parameter("cloud_opacity", opacity)
	material.set_shader_parameter("texture_scale", texture_scale)
	return material
