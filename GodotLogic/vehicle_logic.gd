extends RefCounted
class_name SD_VehicleLogic

var speed := 0.0
var max_speed := 0.0
var acceleration := 0.0
var steering := 0.0
var fuel := -1.0
var occupied := false

func configure(maximum_speed: float, acceleration_rate: float) -> void:
	max_speed = maxf(maximum_speed, 0.0)
	acceleration = maxf(acceleration_rate, 0.0)

func update_motion(throttle: float, steering_input: float, delta: float) -> float:
	if delta <= 0.0:
		return speed
	if fuel == 0.0:
		throttle = 0.0
	speed = move_toward(speed, clampf(throttle, -1.0, 1.0) * max_speed, acceleration * delta)
	steering = clampf(steering_input, -1.0, 1.0)
	if fuel > 0.0 and absf(throttle) > 0.01:
		fuel = maxf(0.0, fuel - absf(throttle) * delta)
	return speed

func board() -> bool:
	if occupied:
		return false
	occupied = true
	return true

func eject() -> bool:
	if not occupied:
		return false
	occupied = false
	return true
