extends StaticBody3D
class_name CollectibleResource

@export var item_id: String = "stick"
@export var display_name: String = "Stick"
@export var amount: int = 1

func try_collect() -> bool:
    if amount <= 0 or not is_inside_tree():
        return false
    queue_free()
    return true
