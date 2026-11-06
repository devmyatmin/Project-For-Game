extends RefCounted
class_name SD_ConstructionLogic

func validate_placement(candidate: Dictionary, existing: Array) -> Dictionary:
	if not bool(candidate.get("terrain_supported", false)):
		return {"valid": false, "reason": "unsupported"}
	if bool(candidate.get("underwater_blocked", false)):
		return {"valid": false, "reason": "underwater"}
	var max_distance := float(candidate.get("max_link_distance", 0.0))
	var position: Vector3 = candidate.get("position", Vector3.ZERO)
	var connector_id := str(candidate.get("connector_id", ""))
	var linked := false
	for structure in existing:
		if connector_id != "" and str(structure.get("connector_id", "")) == connector_id:
			linked = true
			break
		if max_distance > 0.0:
			var other_position: Vector3 = structure.get("position", Vector3.ZERO)
			if position.distance_to(other_position) <= max_distance:
				linked = true
				break
	if bool(candidate.get("requires_connection", false)) and not linked:
		return {"valid": false, "reason": "no_connection"}
	return {"valid": true, "reason": "ok"}

func connect_structures(a: Dictionary, b: Dictionary) -> Dictionary:
	var links: Array = a.get("connections", []).duplicate()
	var id := str(b.get("id", ""))
	if id.is_empty():
		return {"success": false, "connections": links}
	if not links.has(id):
		links.append(id)
	return {"success": true, "connections": links}
