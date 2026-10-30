extends CanvasLayer
class_name MobileControls

signal jump_requested
signal interact_requested
signal crouch_changed(is_crouching: bool)
signal run_changed(is_running: bool)

@export var look_sensitivity: float = 0.0045
var movement_input := Vector2.ZERO
var look_delta := Vector2.ZERO
var crouch_toggled := false
var run_toggled := false

var _stick_base: Control
var _stick_knob: Control
var _look_area: Control
var _status_label: Label
var _inventory_label: Label
var _stick_active := false
var _stick_touch := -1
var _look_touch := -1
const STICK_RADIUS_BASE := 58.0
var _stick_radius := STICK_RADIUS_BASE

func _ready() -> void:
    var root := Control.new()
    root.name = "TouchHud"
    root.anchor_right = 1.0
    root.anchor_bottom = 1.0
    root.mouse_filter = Control.MOUSE_FILTER_IGNORE
    add_child(root)

    _look_area = Control.new()
    _look_area.name = "LookArea"
    _look_area.anchor_left = 0.38
    _look_area.anchor_right = 1.0
    _look_area.anchor_bottom = 1.0
    _look_area.mouse_filter = Control.MOUSE_FILTER_STOP
    root.add_child(_look_area)
    _look_area.gui_input.connect(_on_look_input)

    _stick_base = _make_circle("JoystickBase", Vector2(28, -164), Vector2(164, -28), Color(0.06, 0.09, 0.11, 0.48))
    _stick_base.anchor_top = 1.0
    _stick_base.anchor_bottom = 1.0
    root.add_child(_stick_base)
    _stick_base.gui_input.connect(_on_stick_input)

    _stick_knob = _make_circle("JoystickKnob", Vector2(39, 39), Vector2(97, 97), Color(0.78, 0.86, 0.88, 0.62))
    _stick_knob.mouse_filter = Control.MOUSE_FILTER_IGNORE
    _stick_base.add_child(_stick_knob)

    var interact := _make_button(root, "InteractButton", "USE", Vector2(-104, -178), Vector2(-24, -98))
    interact.pressed.connect(func(): interact_requested.emit())
    var run := _make_button(root, "RunButton", "RUN", Vector2(-194, -210), Vector2(-114, -138))
    run.add_theme_font_size_override("font_size", 15)
    run.pressed.connect(func():
        run_toggled = not run_toggled
        run_changed.emit(run_toggled)
    )
    var crouch := _make_button(root, "CrouchButton", "↓", Vector2(-184, -118), Vector2(-112, -46))
    crouch.pressed.connect(func():
        crouch_toggled = not crouch_toggled
        crouch_changed.emit(crouch_toggled)
    )
    var jump := _make_button(root, "JumpButton", "↑", Vector2(-100, -96), Vector2(-20, -16))
    jump.pressed.connect(func(): jump_requested.emit())

    var hint := Label.new()
    hint.name = "HudHint"
    hint.text = "SURVIVE"
    hint.offset_left = 24
    hint.offset_top = 20
    hint.offset_right = 180
    hint.offset_bottom = 48
    hint.mouse_filter = Control.MOUSE_FILTER_IGNORE
    hint.add_theme_color_override("font_color", Color(0.88, 0.94, 0.91, 0.88))
    hint.add_theme_font_size_override("font_size", 18)
    root.add_child(hint)

    _status_label = Label.new()
    _status_label.name = "SurvivalStatus"
    _status_label.text = "♥ 100     WATER 100     FOOD 100"
    _status_label.offset_left = 24
    _status_label.offset_top = 52
    _status_label.offset_right = 380
    _status_label.offset_bottom = 76
    _status_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
    _status_label.add_theme_color_override("font_color", Color(0.91, 0.94, 0.88, 0.8))
    _status_label.add_theme_font_size_override("font_size", 12)
    root.add_child(_status_label)

    _inventory_label = Label.new()
    _inventory_label.name = "InventoryStatus"
    _inventory_label.text = "ITEMS  0"
    _inventory_label.offset_left = 24
    _inventory_label.offset_top = 78
    _inventory_label.offset_right = 380
    _inventory_label.offset_bottom = 100
    _inventory_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
    _inventory_label.add_theme_color_override("font_color", Color(0.91, 0.94, 0.88, 0.8))
    _inventory_label.add_theme_font_size_override("font_size", 12)
    root.add_child(_inventory_label)

    # Reflow the touch HUD whenever the device orientation or viewport changes.
    get_viewport().size_changed.connect(_apply_responsive_layout)
    _apply_responsive_layout()

func _apply_responsive_layout() -> void:
    var viewport_size := get_viewport().get_visible_rect().size
    if viewport_size.x <= 0.0 or viewport_size.y <= 0.0:
        return

    # The layout is authored around a 900x500 landscape viewport. Clamp the
    # scale so controls stay usable on small phones without becoming enormous
    # on tablets and high-resolution screens.
    var ui_scale := clampf(minf(viewport_size.x / 900.0, viewport_size.y / 500.0), 0.72, 1.15)
    _stick_radius = STICK_RADIUS_BASE * ui_scale
    _stick_base.offset_left = 28.0 * ui_scale
    _stick_base.offset_top = -164.0 * ui_scale
    _stick_base.offset_right = 164.0 * ui_scale
    _stick_base.offset_bottom = -28.0 * ui_scale
    _stick_knob.offset_left = 39.0 * ui_scale
    _stick_knob.offset_top = 39.0 * ui_scale
    _stick_knob.offset_right = 97.0 * ui_scale
    _stick_knob.offset_bottom = 97.0 * ui_scale

    _set_button_layout("InteractButton", -104, -178, -24, -98, ui_scale)
    _set_button_layout("RunButton", -194, -210, -114, -138, ui_scale)
    _set_button_layout("CrouchButton", -184, -118, -112, -46, ui_scale)
    _set_button_layout("JumpButton", -100, -96, -20, -16, ui_scale)

    var hint := get_node_or_null("TouchHud/HudHint") as Label
    if hint:
        hint.offset_left = 24.0 * ui_scale
        hint.offset_top = 20.0 * ui_scale
        hint.offset_right = 180.0 * ui_scale
        hint.offset_bottom = 48.0 * ui_scale
        hint.add_theme_font_size_override("font_size", roundi(18.0 * ui_scale))

    if is_instance_valid(_status_label):
        _status_label.offset_left = 24.0 * ui_scale
        _status_label.offset_top = 52.0 * ui_scale
        _status_label.offset_right = 380.0 * ui_scale
        _status_label.offset_bottom = 76.0 * ui_scale
        _status_label.add_theme_font_size_override("font_size", roundi(12.0 * ui_scale))

    if is_instance_valid(_inventory_label):
        _inventory_label.offset_left = 24.0 * ui_scale
        _inventory_label.offset_top = 78.0 * ui_scale
        _inventory_label.offset_right = 380.0 * ui_scale
        _inventory_label.offset_bottom = 100.0 * ui_scale
        _inventory_label.add_theme_font_size_override("font_size", roundi(12.0 * ui_scale))

    _stick_knob.position = (_stick_base.size - _stick_knob.size) * 0.5

func _set_button_layout(node_name: String, left: float, top: float, right: float, bottom: float, ui_scale: float) -> void:
    var button := get_node_or_null("TouchHud/" + node_name) as Button
    if not button:
        return
    button.offset_left = left * ui_scale
    button.offset_top = top * ui_scale
    button.offset_right = right * ui_scale
    button.offset_bottom = bottom * ui_scale
    var base_font := 13.0 if node_name == "InteractButton" else 24.0
    if node_name == "RunButton":
        base_font = 15.0
    button.add_theme_font_size_override("font_size", maxi(10, roundi(base_font * ui_scale)))

func set_survival_status(health: float, water: float, food: float) -> void:
    if is_instance_valid(_status_label):
        _status_label.text = "♥ %d   WATER %d   FOOD %d" % [roundi(health), roundi(water), roundi(food)]

func set_inventory_status(item_name: String, amount_collected: int, total: int) -> void:
    if is_instance_valid(_inventory_label):
        _inventory_label.text = "PICKED UP: %s +%d  (TOTAL %d)" % [item_name, amount_collected, total]

func consume_look_delta() -> Vector2:
    var value := look_delta
    look_delta = Vector2.ZERO
    return value

func _make_circle(node_name: String, top_left: Vector2, bottom_right: Vector2, color: Color) -> Control:
    var panel := Panel.new()
    panel.name = node_name
    panel.offset_left = top_left.x
    panel.offset_top = top_left.y
    panel.offset_right = bottom_right.x
    panel.offset_bottom = bottom_right.y
    panel.mouse_filter = Control.MOUSE_FILTER_STOP
    var style := StyleBoxFlat.new()
    style.bg_color = color
    style.corner_radius_top_left = 100
    style.corner_radius_top_right = 100
    style.corner_radius_bottom_left = 100
    style.corner_radius_bottom_right = 100
    style.set_border_width_all(1)
    style.border_color = Color(0.86, 0.94, 0.94, 0.25)
    panel.add_theme_stylebox_override("panel", style)
    return panel

func _make_button(parent: Control, node_name: String, label_text: String, top_left: Vector2, bottom_right: Vector2) -> Button:
    var button := Button.new()
    button.name = node_name
    button.text = label_text
    button.anchor_left = 1.0
    button.anchor_right = 1.0
    button.anchor_top = 1.0
    button.anchor_bottom = 1.0
    button.offset_left = top_left.x
    button.offset_top = top_left.y
    button.offset_right = bottom_right.x
    button.offset_bottom = bottom_right.y
    button.focus_mode = Control.FOCUS_NONE
    var normal := StyleBoxFlat.new()
    normal.bg_color = Color(0.07, 0.12, 0.14, 0.64)
    normal.set_corner_radius_all(40)
    normal.set_border_width_all(1)
    normal.border_color = Color(0.85, 0.94, 0.92, 0.45)
    var pressed := normal.duplicate() as StyleBoxFlat
    pressed.bg_color = Color(0.22, 0.37, 0.35, 0.88)
    button.add_theme_stylebox_override("normal", normal)
    button.add_theme_stylebox_override("hover", normal)
    button.add_theme_stylebox_override("pressed", pressed)
    button.add_theme_color_override("font_color", Color(0.95, 0.98, 0.96, 1))
    button.add_theme_font_size_override("font_size", 13 if label_text == "USE" else 24)
    parent.add_child(button)
    return button

func _on_stick_input(event: InputEvent) -> void:
    if event is InputEventScreenTouch:
        if event.pressed and _stick_touch == -1:
            _stick_touch = event.index
            _stick_active = true
            _update_stick(event.position)
        elif not event.pressed and event.index == _stick_touch:
            _reset_stick()
    elif event is InputEventScreenDrag and event.index == _stick_touch:
        _update_stick(event.position)
    elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT:
        _stick_active = event.pressed
        if _stick_active:
            _update_stick(event.position)
        else:
            _reset_stick()
    elif event is InputEventMouseMotion and _stick_active and Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
        _update_stick(event.position)

func _update_stick(screen_position: Vector2) -> void:
    var local := _stick_base.get_global_transform_with_canvas().affine_inverse() * screen_position
    var center := _stick_base.size * 0.5
    var offset := local - center
    if offset.length() > _stick_radius:
        offset = offset.normalized() * _stick_radius
    movement_input = Vector2(offset.x / _stick_radius, offset.y / _stick_radius)
    _stick_knob.position = center + offset - (_stick_knob.size * 0.5)

func _reset_stick() -> void:
    _stick_active = false
    _stick_touch = -1
    movement_input = Vector2.ZERO
    _stick_knob.position = (_stick_base.size - _stick_knob.size) * 0.5

func _on_look_input(event: InputEvent) -> void:
    if event is InputEventScreenTouch:
        if event.pressed and _look_touch == -1:
            _look_touch = event.index
        elif not event.pressed and event.index == _look_touch:
            _look_touch = -1
    elif event is InputEventScreenDrag and event.index == _look_touch:
        look_delta += event.relative * look_sensitivity
    elif event is InputEventMouseMotion and Input.is_mouse_button_pressed(MOUSE_BUTTON_RIGHT):
        look_delta += event.relative * look_sensitivity
