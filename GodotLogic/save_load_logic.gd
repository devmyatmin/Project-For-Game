extends RefCounted
class_name SD_SaveLoadLogic

const FORMAT_VERSION := 1

static func encode_state(state: Dictionary) -> String:
	return JSON.stringify({"format_version": FORMAT_VERSION, "state": state}, "\t")

static func decode_state(json_text: String) -> Dictionary:
	var parsed = JSON.parse_string(json_text)
	if not (parsed is Dictionary):
		return {"ok": false, "error": "invalid_json"}
	if int(parsed.get("format_version", -1)) != FORMAT_VERSION:
		return {"ok": false, "error": "unsupported_version"}
	if not (parsed.get("state", null) is Dictionary):
		return {"ok": false, "error": "invalid_state"}
	return {"ok": true, "state": parsed["state"]}

static func save_to_path(path: String, state: Dictionary) -> bool:
	var file := FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		return false
	file.store_string(encode_state(state))
	file.flush()
	var error := file.get_error()
	file.close()
	return error == OK

static func load_from_path(path: String) -> Dictionary:
	if not FileAccess.file_exists(path):
		return {"ok": false, "error": "not_found"}
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		return {"ok": false, "error": "open_failed"}
	var content := file.get_as_text()
	file.close()
	return decode_state(content)
