extends RefCounted
class_name SD_CraftingLogic

var recipes: Dictionary = {}

func set_recipes(recipe_data: Dictionary) -> void:
	recipes = recipe_data.duplicate(true)

func can_craft(recipe_id: String, inventory) -> bool:
	if not recipes.has(recipe_id):
		return false
	var recipe: Dictionary = recipes[recipe_id]
	var ingredients: Dictionary = recipe.get("ingredients", {})
	for item_id in ingredients:
		if inventory.count_item(str(item_id)) < int(ingredients[item_id]):
			return false
	var output_id := str(recipe.get("output_id", ""))
	var output_count := int(recipe.get("output_count", 1))
	return inventory.can_add(output_id, output_count)

func craft(recipe_id: String, inventory) -> bool:
	if not can_craft(recipe_id, inventory):
		return false
	var recipe: Dictionary = recipes[recipe_id]
	var ingredients: Dictionary = recipe.get("ingredients", {})
	for item_id in ingredients:
		if not inventory.remove_item(str(item_id), int(ingredients[item_id])):
			return false
	return inventory.add_item(str(recipe.get("output_id", "")), int(recipe.get("output_count", 1)))
