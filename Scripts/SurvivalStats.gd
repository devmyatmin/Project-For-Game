extends Node
class_name SurvivalStats

@export var max_health: float = 500.0
@export var max_food: float = 100.0
@export var max_water: float = 100.0
@export var food_drain_per_second: float = 0.025
@export var water_drain_per_second: float = 0.045
@export var starvation_damage_per_second: float = 2.0

var health: float = 500.0
var food: float = 100.0
var water: float = 100.0
var _hud: MobileControls
var _refresh_timer := 0.0

func _ready() -> void:
    health = max_health
    food = max_food
    water = max_water
    _hud = get_tree().root.find_child("MobileControls", true, false) as MobileControls
    _refresh_hud()

func _process(delta: float) -> void:
    food = maxf(0.0, food - food_drain_per_second * delta)
    water = maxf(0.0, water - water_drain_per_second * delta)
    if food <= 0.0 or water <= 0.0:
        health = maxf(0.0, health - starvation_damage_per_second * delta)
    _refresh_timer += delta
    if _refresh_timer >= 0.25:
        _refresh_timer = 0.0
        _refresh_hud()

func restore_food(amount: float) -> void:
    food = clampf(food + amount, 0.0, max_food)

func restore_water(amount: float) -> void:
    water = clampf(water + amount, 0.0, max_water)

func restore_health(amount: float) -> void:
    health = clampf(health + amount, 0.0, max_health)

func _refresh_hud() -> void:
    if _hud:
        _hud.set_survival_status(health, water, food)
