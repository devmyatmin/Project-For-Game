extends Node

## A lightweight Stranded Deep-style day/night lighting controller for Godot Mobile.
## One normalized clock drives the sun, sky, and ambient fill so exposure stays stable.
@export_range(120.0, 3600.0, 30.0) var day_length_seconds: float = 1200.0
@export_range(0.0, 1.0, 0.01) var start_time_normalized: float = 0.32
@export var sun_path: NodePath = NodePath("../DirectionalLight3D")
@export var world_environment_path: NodePath = NodePath("../WorldEnvironment")

const DAY_SUN_COLOR := Color(1.0, 0.94, 0.82, 1.0)
const SUNSET_COLOR := Color(1.0, 0.53, 0.28, 1.0)
const NIGHT_SUN_COLOR := Color(0.30, 0.39, 0.58, 1.0)
const DAY_AMBIENT_COLOR := Color(0.62, 0.72, 0.82, 1.0)
const NIGHT_AMBIENT_COLOR := Color(0.12, 0.17, 0.28, 1.0)
const DAY_SKY_TOP := Color(0.08, 0.28, 0.58, 1.0)
const NIGHT_SKY_TOP := Color(0.008, 0.018, 0.055, 1.0)
const DAY_SKY_HORIZON := Color(0.62, 0.79, 0.90, 1.0)
const NIGHT_SKY_HORIZON := Color(0.055, 0.085, 0.14, 1.0)

var _sun: DirectionalLight3D
var _world_environment: WorldEnvironment
var _sky_material: ProceduralSkyMaterial
var _elapsed_seconds := 0.0


func _ready() -> void:
	_sun = get_node_or_null(sun_path) as DirectionalLight3D
	_world_environment = get_node_or_null(world_environment_path) as WorldEnvironment
	if _world_environment != null and _world_environment.environment != null:
		var sky := _world_environment.environment.sky
		if sky != null:
			_sky_material = sky.sky_material as ProceduralSkyMaterial
	_apply_time(start_time_normalized)


func _process(delta: float) -> void:
	if day_length_seconds <= 0.0:
		return
	_elapsed_seconds = fposmod(_elapsed_seconds + delta, day_length_seconds)
	var time_value := fposmod(start_time_normalized + _elapsed_seconds / day_length_seconds, 1.0)
	_apply_time(time_value)


func _apply_time(time_value: float) -> void:
	# At 0.25 the sun is highest; at 0.75 it is below the horizon.
	var sun_angle := time_value * TAU
	var sun_height := sin(sun_angle)
	var day_blend := clampf((sun_height + 0.16) / 0.42, 0.0, 1.0)
	day_blend = day_blend * day_blend * (3.0 - 2.0 * day_blend)
	var horizon_warmth := clampf(1.0 - absf(sun_height) * 3.2, 0.0, 1.0)

	if _sun != null:
		_sun.rotation.x = sun_angle
		_sun.rotation.y = -0.5
		_sun.light_energy = lerpf(0.025, 1.25, day_blend)
		var daylight_color := DAY_SUN_COLOR.lerp(SUNSET_COLOR, horizon_warmth * 0.72)
		_sun.light_color = NIGHT_SUN_COLOR.lerp(daylight_color, day_blend)
		_sun.shadow_enabled = day_blend > 0.08

	if _world_environment != null and _world_environment.environment != null:
		var environment := _world_environment.environment
		environment.ambient_light_color = NIGHT_AMBIENT_COLOR.lerp(DAY_AMBIENT_COLOR, day_blend)
		environment.ambient_light_energy = lerpf(0.22, 0.85, day_blend)
		if _sky_material != null:
			_sky_material.sky_top_color = NIGHT_SKY_TOP.lerp(DAY_SKY_TOP, day_blend)
			var horizon_day := DAY_SKY_HORIZON.lerp(SUNSET_COLOR, horizon_warmth * 0.38)
			_sky_material.sky_horizon_color = NIGHT_SKY_HORIZON.lerp(horizon_day, day_blend)
			_sky_material.ground_bottom_color = Color(0.008, 0.012, 0.025, 1.0).lerp(Color(0.08, 0.11, 0.12, 1.0), day_blend)
			_sky_material.ground_horizon_color = NIGHT_SKY_HORIZON.lerp(Color(0.48, 0.62, 0.66, 1.0), day_blend)
