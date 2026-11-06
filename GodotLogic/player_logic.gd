extends RefCounted
class_name SD_PlayerLogic

const StatsScript = preload("res://GodotLogic/survival_stats.gd")
const InventoryScript = preload("res://GodotLogic/inventory_logic.gd")
const CraftingScript = preload("res://GodotLogic/crafting_logic.gd")

var statistics
var inventory
var crafting
var held_item_id := ""
var initialized := false

func initialize() -> void:
	if initialized:
		return
	statistics = StatsScript.new()
	inventory = InventoryScript.new()
	crafting = CraftingScript.new()
	initialized = true

func equip(item_id: String) -> bool:
	if not initialized or inventory.count_item(item_id) <= 0:
		return false
	held_item_id = item_id
	return true

func drop_held(amount := 1) -> bool:
	if held_item_id.is_empty() or not inventory.remove_item(held_item_id, amount):
		return false
	if inventory.count_item(held_item_id) == 0:
		held_item_id = ""
	return true

func save_state() -> Dictionary:
	if not initialized:
		return {}
	return {"statistics": statistics.to_dict(), "inventory": inventory.to_dict(), "held_item_id": held_item_id}

func load_state(data: Dictionary) -> void:
	initialize()
	statistics.from_dict(data.get("statistics", {}))
	inventory.from_dict(data.get("inventory", {}))
	held_item_id = str(data.get("held_item_id", ""))
	if not held_item_id.is_empty() and inventory.count_item(held_item_id) == 0:
		held_item_id = ""
