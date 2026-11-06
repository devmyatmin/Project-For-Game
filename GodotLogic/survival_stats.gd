extends RefCounted
class_name SD_SurvivalStats

signal health_changed(value: float)
signal oxygen_changed(value: float)
signal status_changed(effect_id: String, active: bool)

const VITAL_MAX := 700.0
const OXYGEN_MAX := 100.0
const OXYGEN_DRAIN_PER_SECOND := 3.5
const OXYGEN_REGAIN_PER_SECOND := 40.0
const DROWNING_GRACE_SECONDS := 15.0

var health := VITAL_MAX
var calories := VITAL_MAX
var fluids := VITAL_MAX
var sleep := VITAL_MAX
var stamina := 100.0
var oxygen := OXYGEN_MAX
var drowning_elapsed := 0.0
var drowning_active := false
var effects: Dictionary = {}

func _clamp_vitals() -> void:
	health = clampf(health, 0.0, VITAL_MAX)
	calories = clampf(calories, 0.0, VITAL_MAX)
	fluids = clampf(fluids, 0.0, VITAL_MAX)
	sleep = clampf(sleep, 0.0, VITAL_MAX)
	stamina = clampf(stamina, 0.0, 100.0)
	oxygen = clampf(oxygen, 0.0, OXYGEN_MAX)

func apply_damage(amount: float) -> float:
	if amount <= 0.0:
		return health
	health = clampf(health - amount, 0.0, VITAL_MAX)
	health_changed.emit(health)
	return health

func add_health(amount: float) -> float:
	health = clampf(health + amount, 0.0, VITAL_MAX)
	health_changed.emit(health)
	return health

func update_needs(hours: float, hard_mode := false, calorie_burn_factor := 1.0, fluid_burn_factor := 1.0) -> void:
	if hours <= 0.0:
		return
	var hunger_days := 1.0 if hard_mode else 2.0
	var thirst_days := 0.5 if hard_mode else 1.0
	calories -= (VITAL_MAX / (hunger_days * 24.0)) * hours * maxf(calorie_burn_factor, 0.0)
	fluids -= (VITAL_MAX / (thirst_days * 24.0)) * hours * maxf(fluid_burn_factor, 0.0)
	_clamp_vitals()
	if calories >= 500.0 and fluids >= 500.0 and not _has_health_debuff():
		add_health((VITAL_MAX / (7.0 * 24.0)) * hours)

func update_oxygen(seconds: float, underwater: bool) -> void:
	if seconds <= 0.0:
		return
	if underwater:
		oxygen = maxf(0.0, oxygen - OXYGEN_DRAIN_PER_SECOND * seconds)
		if oxygen <= 0.0:
			drowning_elapsed += seconds
			drowning_active = drowning_elapsed >= DROWNING_GRACE_SECONDS
	else:
		oxygen = minf(OXYGEN_MAX, oxygen + OXYGEN_REGAIN_PER_SECOND * seconds)
		if oxygen >= OXYGEN_MAX:
			drowning_elapsed = 0.0
			drowning_active = false
	oxygen_changed.emit(oxygen)

func add_status_effect(effect_id: String, duration_hours: float, health_per_hour := 0.0) -> void:
	if effect_id.is_empty():
		return
	effects[effect_id] = {"remaining_hours": maxf(duration_hours, 0.0), "health_per_hour": health_per_hour}
	status_changed.emit(effect_id, true)

func remove_status_effect(effect_id: String) -> void:
	if effects.erase(effect_id):
		status_changed.emit(effect_id, false)

func tick_status_effects(hours: float) -> void:
	if hours <= 0.0:
		return
	for effect_id in effects.keys():
		var effect: Dictionary = effects[effect_id]
		var rate := float(effect.get("health_per_hour", 0.0))
		if rate > 0.0:
			add_health(rate * hours)
		elif rate < 0.0:
			apply_damage(-rate * hours)
		effect["remaining_hours"] = float(effect.get("remaining_hours", 0.0)) - hours
		if float(effect["remaining_hours"]) <= 0.0:
			remove_status_effect(effect_id)
		else:
			effects[effect_id] = effect

func _has_health_debuff() -> bool:
	for effect in effects.values():
		if float(effect.get("health_per_hour", 0.0)) < 0.0:
			return true
	return false

func to_dict() -> Dictionary:
	return {"health": health, "calories": calories, "fluids": fluids, "sleep": sleep, "stamina": stamina, "oxygen": oxygen, "drowning_elapsed": drowning_elapsed, "effects": effects.duplicate(true)}

func from_dict(data: Dictionary) -> void:
	health = float(data.get("health", VITAL_MAX))
	calories = float(data.get("calories", VITAL_MAX))
	fluids = float(data.get("fluids", VITAL_MAX))
	sleep = float(data.get("sleep", VITAL_MAX))
	stamina = float(data.get("stamina", 100.0))
	oxygen = float(data.get("oxygen", OXYGEN_MAX))
	drowning_elapsed = float(data.get("drowning_elapsed", 0.0))
	effects = data.get("effects", {}).duplicate(true)
	drowning_active = oxygen <= 0.0 and drowning_elapsed >= DROWNING_GRACE_SECONDS
	_clamp_vitals()
