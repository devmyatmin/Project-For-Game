extends RefCounted
class_name SD_WildlifeLogic

enum State { IDLE, EXPLORE, SEARCH, CHASE, HIDE, ATTACK, DYING }

var state: State = State.IDLE
var detection_distance := 18.0
var attack_distance := 2.0
var health := 100.0
var last_known_player_position := Vector3.ZERO

func update_state(player_position: Vector3, animal_position: Vector3, can_see_player: bool, has_hiding_spot: bool) -> State:
	if state == State.DYING:
		return state
	var distance := animal_position.distance_to(player_position)
	if can_see_player:
		last_known_player_position = player_position
		if distance <= attack_distance:
			state = State.ATTACK
		elif distance <= detection_distance:
			state = State.CHASE
		else:
			state = State.SEARCH
	elif state == State.CHASE or state == State.ATTACK:
		state = State.SEARCH
	elif state == State.SEARCH and has_hiding_spot:
		state = State.HIDE
	elif state == State.IDLE:
		state = State.EXPLORE
	return state

func apply_damage(amount: float) -> State:
	if amount > 0.0:
		health = maxf(0.0, health - amount)
		if health <= 0.0:
			state = State.DYING
	return state

func set_state(next_state: State) -> void:
	if state != State.DYING:
		state = next_state
