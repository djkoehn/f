@tool
extends EditorPlugin

const SCENE_PATH = "res://addons/ColorPaletteManager/Colors.tscn"
const PALETTE_PATH = "res://src/Config/ColorPalettes.json"

var _colors_instance: Control
var _color_hexes: Dictionary
var _colors: Dictionary
var _color_container: VBoxContainer

func _enter_tree() -> void:
	load_color_palettes()
	create_ui()

func load_color_palettes() -> void:
	var file = FileAccess.open(PALETTE_PATH, FileAccess.READ)
	var json = file.get_as_text()
	file.close()
	_color_hexes = JSON.parse_string(json)
	
	# Convert hex codes to Godot Colors
	_colors = {}
	for palette_name in _color_hexes:
		_colors[palette_name] = []
		for hex in _color_hexes[palette_name]:
			_colors[palette_name].append(Color(hex))

func create_ui() -> void:
	var scene = load(SCENE_PATH) as PackedScene
	_colors_instance = scene.instantiate()
	
	# Make sure the control is properly sized for a dock
	_colors_instance.custom_minimum_size = Vector2(200, 0) # Set a minimum width for dock
	_colors_instance.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_colors_instance.size_flags_vertical = Control.SIZE_EXPAND_FILL
	
	var container = _colors_instance.get_node(".")
	var scroll_container = container.get_node("ScrollContainer")
	_color_container = scroll_container.get_node("ColorContainer")
	
	# Add reload button at the top
	var reload_button = Button.new()
	reload_button.text = "Reload Palettes"
	reload_button.pressed.connect(_on_reload_pressed)
	_color_container.add_child(reload_button)
	
	_create_color_palettes()
	
	# Add as dock instead of bottom panel
	add_control_to_dock(DOCK_SLOT_RIGHT_UL, _colors_instance)

func _create_color_palettes() -> void:
	# Clear existing color palettes except the reload button
	for child in _color_container.get_children():
		if child is Button: # Skip the reload button
			continue
		child.queue_free()
	
	for color_name in _colors:
		var current_vbox = VBoxContainer.new()
		var current_color_name_label = Label.new()
		current_color_name_label.text = color_name.capitalize()
		current_color_name_label.add_theme_font_size_override("font_size", 18)
		current_vbox.add_child(current_color_name_label)
		
		var color_grid = GridContainer.new()
		color_grid.columns = 2 # Two columns: color button and hex code
		current_vbox.add_child(color_grid)
		_color_container.add_child(current_vbox)
		
		for i in range(_colors[color_name].size()):
			var color = _colors[color_name][i]
			var hex = _color_hexes[color_name][i]
			
			var button_container = HBoxContainer.new()
			color_grid.add_child(button_container)
			
			var new_color_button = Button.new()
			new_color_button.mouse_default_cursor_shape = Control.CURSOR_POINTING_HAND
			new_color_button.custom_minimum_size = Vector2(40, 25)
			new_color_button.tooltip_text = hex
			button_container.add_child(new_color_button)
			
			var new_style_box_flat = StyleBoxFlat.new()
			new_style_box_flat.corner_radius_top_left = 4
			new_style_box_flat.corner_radius_top_right = 4
			new_style_box_flat.corner_radius_bottom_left = 4
			new_style_box_flat.corner_radius_bottom_right = 4
			new_style_box_flat.bg_color = color
			
			var new_style_box_flat2 = StyleBoxFlat.new()
			new_style_box_flat2.corner_radius_top_left = 4
			new_style_box_flat2.corner_radius_top_right = 4
			new_style_box_flat2.corner_radius_bottom_left = 4
			new_style_box_flat2.corner_radius_bottom_right = 4
			new_style_box_flat2.bg_color = color
			new_style_box_flat2.border_color = Color.BLACK if (color.r * 0.299 + color.g * 0.587 + color.b * 0.114) > 0.5 else Color.WHITE
			new_style_box_flat2.set_border_width_all(1)
			
			new_color_button.add_theme_stylebox_override("normal", new_style_box_flat)
			new_color_button.add_theme_stylebox_override("hover", new_style_box_flat2)
			new_color_button.add_theme_stylebox_override("pressed", new_style_box_flat2)
			new_color_button.add_theme_stylebox_override("disabled", new_style_box_flat2)
			new_color_button.add_theme_stylebox_override("focus", new_style_box_flat2)
			
			var hex_label = Label.new()
			hex_label.text = hex
			color_grid.add_child(hex_label)
			
			new_color_button.pressed.connect(_copy_color.bind(hex))
		
		current_vbox.add_spacer(true)

func _on_reload_pressed() -> void:
	load_color_palettes()
	_create_color_palettes()
	print("Color palettes reloaded from:", PALETTE_PATH)

func _copy_color(hex_color: String) -> void:
	DisplayServer.clipboard_set(hex_color)
	print("Copied color:", hex_color)

func _exit_tree() -> void:
	if _colors_instance != null:
		remove_control_from_docks(_colors_instance)
		_colors_instance.queue_free()

func _has_main_screen() -> bool:
	return false

func _make_visible(visible: bool) -> void:
	if _colors_instance != null:
		_colors_instance.visible = visible

func _get_plugin_name() -> String:
	return "Colors"