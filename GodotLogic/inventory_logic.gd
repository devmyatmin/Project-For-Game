extends RefCounted
class_name SD_InventoryLogic

var slot_limit: int
var max_stack: int
var slots: Array[Dictionary] = []

func _init(slot_count := 24, stack_limit := 99) -> void:
	slot_limit = maxi(slot_count, 1)
	max_stack = maxi(stack_limit, 1)
	for _i in range(slot_limit):
		slots.append({"item_id": "", "count": 0})

func count_item(item_id: String) -> int:
	var total := 0
	for slot in slots:
		if slot["item_id"] == item_id:
			total += int(slot["count"])
	return total

func can_add(item_id: String, amount: int) -> bool:
	if item_id.is_empty() or amount <= 0:
		return false
	var capacity := 0
	for slot in slots:
		if slot["item_id"] == item_id:
			capacity += max_stack - int(slot["count"])
		elif int(slot["count"]) == 0:
			capacity += max_stack
	return capacity >= amount

func add_item(item_id: String, amount: int) -> bool:
	if not can_add(item_id, amount):
		return false
	var remaining := amount
	for slot in slots:
		if slot["item_id"] == item_id and int(slot["count"]) < max_stack:
			var moved := mini(remaining, max_stack - int(slot["count"]))
			slot["count"] = int(slot["count"]) + moved
			remaining -= moved
			if remaining == 0:
				return true
	for slot in slots:
		if int(slot["count"]) == 0:
			var moved := mini(remaining, max_stack)
			slot["item_id"] = item_id
			slot["count"] = moved
			remaining -= moved
			if remaining == 0:
				return true
	return remaining == 0

func remove_item(item_id: String, amount: int) -> bool:
	if amount <= 0 or count_item(item_id) < amount:
		return false
	var remaining := amount
	for slot in slots:
		if slot["item_id"] != item_id:
			continue
		var removed := mini(remaining, int(slot["count"]))
		slot["count"] = int(slot["count"]) - removed
		remaining -= removed
		if int(slot["count"]) == 0:
			slot["item_id"] = ""
		if remaining == 0:
			return true
	return remaining == 0

func to_dict() -> Dictionary:
	return {"slot_limit": slot_limit, "max_stack": max_stack, "slots": slots.duplicate(true)}

func from_dict(data: Dictionary) -> void:
	slot_limit = maxi(int(data.get("slot_limit", 24)), 1)
	max_stack = maxi(int(data.get("max_stack", 99)), 1)
	slots = []
	for raw_slot in data.get("slots", []):
		if raw_slot is Dictionary:
			var item_id := str(raw_slot.get("item_id", ""))
			var count := clampi(int(raw_slot.get("count", 0)), 0, max_stack)
			if count == 0:
				item_id = ""
			slots.append({"item_id": item_id, "count": count})
	while slots.size() < slot_limit:
		slots.append({"item_id": "", "count": 0})
	if slots.size() > slot_limit:
		slots.resize(slot_limit)
