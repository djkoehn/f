@tool
extends EditorPlugin

const SETTINGS_PREFIX = "editor_ui_hider/"
const SETTING_HIDE_RUN = "hide_run_buttons"
const SETTING_HIDE_SCREENS = "hide_screen_selector"
const SETTING_HIDE_TOOLBAR = "hide_canvas_toolbar"

var _hidden_controls = []

func _enter_tree():
	# Register editor settings
	setup_editor_settings()
	
	# Initial hiding based on saved settings
	apply_hiding_settings()
	
	# Connect to editor interface signals to reapply hiding when needed
	get_tree().connect("node_added", _on_node_added)
	get_editor_interface().get_editor_settings().settings_changed.connect(_on_settings_changed)

func _exit_tree():
	# Show all hidden controls before exiting
	show_all_controls()
	
	# Disconnect signals
	if get_tree():
		get_tree().disconnect("node_added", _on_node_added)
	
	var settings = get_editor_interface().get_editor_settings()
	if settings and settings.settings_changed.is_connected(_on_settings_changed):
		settings.settings_changed.disconnect(_on_settings_changed)

func setup_editor_settings():
	var editor_settings = get_editor_interface().get_editor_settings()
	
	# Register our settings if they don't exist
	if not editor_settings.has_setting(SETTINGS_PREFIX + SETTING_HIDE_RUN):
		editor_settings.set_setting(SETTINGS_PREFIX + SETTING_HIDE_RUN, false)
		
	if not editor_settings.has_setting(SETTINGS_PREFIX + SETTING_HIDE_SCREENS):
		editor_settings.set_setting(SETTINGS_PREFIX + SETTING_HIDE_SCREENS, false)
		
	if not editor_settings.has_setting(SETTINGS_PREFIX + SETTING_HIDE_TOOLBAR):
		editor_settings.set_setting(SETTINGS_PREFIX + SETTING_HIDE_TOOLBAR, false)
	
	# Set initial values and metadata
	editor_settings.set_initial_value(SETTINGS_PREFIX + SETTING_HIDE_RUN, false, false)
	editor_settings.set_initial_value(SETTINGS_PREFIX + SETTING_HIDE_SCREENS, false, false)
	editor_settings.set_initial_value(SETTINGS_PREFIX + SETTING_HIDE_TOOLBAR, false, false)
	
	# Add to Editor Settings with proper metadata
	editor_settings.add_property_info({
		"name": SETTINGS_PREFIX + SETTING_HIDE_RUN,
		"type": TYPE_BOOL,
		"hint": PROPERTY_HINT_NONE,
		"hint_string": "",
		"usage": PROPERTY_USAGE_DEFAULT | PROPERTY_USAGE_EDITOR
	})
	editor_settings.add_property_info({
		"name": SETTINGS_PREFIX + SETTING_HIDE_SCREENS,
		"type": TYPE_BOOL,
		"hint": PROPERTY_HINT_NONE,
		"hint_string": "",
		"usage": PROPERTY_USAGE_DEFAULT | PROPERTY_USAGE_EDITOR
	})
	editor_settings.add_property_info({
		"name": SETTINGS_PREFIX + SETTING_HIDE_TOOLBAR,
		"type": TYPE_BOOL,
		"hint": PROPERTY_HINT_NONE,
		"hint_string": "",
		"usage": PROPERTY_USAGE_DEFAULT | PROPERTY_USAGE_EDITOR
	})

func _on_node_added(node: Node):
	# When new nodes are added to the editor, reapply our hiding settings
	if node is Control:
		apply_hiding_settings()

func _on_settings_changed():
	# When settings change, reapply hiding
	apply_hiding_settings()

func apply_hiding_settings():
	# Clear previously hidden controls
	show_all_controls()
	
	var editor_settings = get_editor_interface().get_editor_settings()
	var editor = get_editor_interface()
	var base_control = editor.get_base_control()
	
	# Hide run buttons
	if editor_settings.get_setting(SETTINGS_PREFIX + SETTING_HIDE_RUN):
		var play_buttons = find_node_by_class(base_control, "EditorRunBar")
		if play_buttons:
			play_buttons.visible = false
			_hidden_controls.append(play_buttons)
	
	# Hide main screen selector and engine selector
	if editor_settings.get_setting(SETTINGS_PREFIX + SETTING_HIDE_SCREENS):
		# Try to find the main screen selector container
		var main_container = find_node_by_class(base_control, "PanelContainer")
		if main_container:
			var hbox = find_node_by_class(main_container, "HBoxContainer")
			if hbox:
				for child in hbox.get_children():
					if child is Button or child is HBoxContainer:
						child.visible = false
						_hidden_controls.append(child)
		
		# Try to find the engine selector
		var top_bar = find_node_by_class(base_control, "EditorTitleBar")
		if top_bar:
			for child in top_bar.get_children():
				if child is HBoxContainer:
					for button in child.get_children():
						if button is Button:
							button.visible = false
							_hidden_controls.append(button)
	
	# Hide canvas toolbar
	if editor_settings.get_setting(SETTINGS_PREFIX + SETTING_HIDE_TOOLBAR):
		var main_screen = editor.get_editor_main_screen()
		if main_screen:
			var canvas_editor = find_node_by_class(main_screen, "CanvasItemEditor")
			if canvas_editor:
				var toolbar = find_node_by_class(canvas_editor, "HBoxContainer")
				if toolbar:
					toolbar.visible = false
					_hidden_controls.append(toolbar)
			
			var spatial_editor = find_node_by_class(main_screen, "Node3DEditor")
			if spatial_editor:
				var toolbar = find_node_by_class(spatial_editor, "HBoxContainer")
				if toolbar:
					toolbar.visible = false
					_hidden_controls.append(toolbar)

func show_all_controls():
	for control in _hidden_controls:
		if control and is_instance_valid(control):
			control.visible = true
	
	_hidden_controls.clear()

func find_node_by_class(node: Node, className: String) -> Node:
	if node.get_class() == className:
		return node
	
	for child in node.get_children():
		var found = find_node_by_class(child, className)
		if found:
			return found
	
	return null