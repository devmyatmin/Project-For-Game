extends SceneTree

const Stats = preload("res://GodotLogic/survival_stats.gd")
const Inventory = preload("res://GodotLogic/inventory_logic.gd")
const Crafting = preload("res://GodotLogic/crafting_logic.gd")
const Construction = preload("res://GodotLogic/construction_logic.gd")
const Wildlife = preload("res://GodotLogic/wildlife_logic.gd")
const Vehicle = preload("res://GodotLogic/vehicle_logic.gd")
const SaveLoad = preload("res://GodotLogic/save_load_logic.gd")
const Player = preload("res://GodotLogic/player_logic.gd")

var failures := 0
var checks := 0

func _initialize() -> void:
	call_deferred("_run")

func _check(condition: bool, label: String) -> void:
	checks += 1
	if condition:
		print("PASS: ", label)
	else:
		failures += 1
		push_error("FAIL: " + label)

func _run() -> void:
	var stats = Stats.new()
	_check(stats.health == 700.0 and stats.calories == 700.0 and stats.fluids == 700.0, "survival defaults")
	stats.apply_damage(50.0)
	_check(stats.health == 650.0, "damage applies")
	stats.add_health(1000.0)
	_check(stats.health == 700.0, "health clamps to maximum")
	var needs = Stats.new()
	needs.update_needs(24.0, false)
	_check(is_equal_approx(needs.calories, 350.0), "normal hunger depletion")
	_check(is_equal_approx(needs.fluids, 0.0), "normal thirst depletion")
	var hard_stats = Stats.new()
	hard_stats.update_needs(24.0, true)
	_check(is_equal_approx(hard_stats.calories, 0.0), "hard hunger depletion")
	var oxygen_stats = Stats.new()
	oxygen_stats.update_oxygen(100.0, true)
	_check(oxygen_stats.oxygen == 0.0 and oxygen_stats.drowning_active, "oxygen drain and drowning grace")
	oxygen_stats.update_oxygen(3.0, false)
	_check(oxygen_stats.oxygen > 0.0 and not oxygen_stats.drowning_active, "oxygen recovery resets drowning")

	var inventory = Inventory.new(4, 10)
	_check(inventory.add_item("stick", 15), "inventory add and stack")
	_check(inventory.count_item("stick") == 15, "inventory count")
	_check(not inventory.add_item("stone", 26), "inventory capacity rejects overflow")
	var crafting = Crafting.new()
	crafting.set_recipes({"axe": {"ingredients": {"stick": 2, "stone": 1}, "output_id": "axe", "output_count": 1}})
	_check(not crafting.can_craft("axe", inventory), "crafting rejects missing materials")
	_check(inventory.add_item("stone", 1), "add crafting material")
	_check(crafting.craft("axe", inventory), "craft consumes materials and creates output")
	_check(inventory.count_item("stick") == 13 and inventory.count_item("axe") == 1, "crafting result is consistent")

	var construction = Construction.new()
	var placement: Dictionary = construction.validate_placement({"terrain_supported": true, "requires_connection": true, "connector_id": "A"}, [])
	_check(not placement["valid"] and placement["reason"] == "no_connection", "construction requires connector")
	placement = construction.validate_placement({"terrain_supported": true, "requires_connection": true, "connector_id": "A"}, [{"connector_id": "A"}])
	_check(placement["valid"], "construction connector validation")

	var animal = Wildlife.new()
	animal.update_state(Vector3(1, 0, 0), Vector3.ZERO, true, false)
	_check(animal.state == Wildlife.State.ATTACK, "wildlife attack state at close range")
	animal.apply_damage(100.0)
	_check(animal.state == Wildlife.State.DYING, "wildlife death state")

	var vehicle = Vehicle.new()
	vehicle.configure(10.0, 5.0)
	vehicle.board()
	vehicle.update_motion(1.0, 0.25, 1.0)
	_check(is_equal_approx(vehicle.speed, 5.0), "vehicle acceleration")
	_check(is_equal_approx(vehicle.steering, 0.25), "vehicle steering input")
	_check(vehicle.eject(), "vehicle eject")

	var encoded := SaveLoad.encode_state({"day": 3, "health": 450.0})
	var decoded: Dictionary = SaveLoad.decode_state(encoded)
	_check(decoded["ok"] and decoded["state"]["day"] == 3, "save data round trip")
	_check(not SaveLoad.decode_state("{broken")["ok"], "save data rejects invalid JSON")

	var player = Player.new()
	player.initialize()
	_check(player.initialized and not player.equip("missing"), "player initialization and equip validation")
	player.inventory.add_item("stone", 2)
	_check(player.equip("stone"), "player equips inventory item")
	var snapshot: Dictionary = player.save_state()
	var restored = Player.new()
	restored.load_state(snapshot)
	_check(restored.inventory.count_item("stone") == 2 and restored.held_item_id == "stone", "player state restoration")

	print("LOGIC TESTS: ", checks, " checks; ", failures, " failures")
	quit(0 if failures == 0 else 1)
