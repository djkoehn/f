@tool
class_name ToolbarAnimator
extends Node

# Animation properties
@export var animation_speed: float = 0.3
@export var closed_position: float = -120.0
@export var opened_position: float = 0.0
@export var hover_margin: float = 10.0

# Private variables
var _is_open: bool = false
var _tween: Tween
var _hover_timer: Timer

# Nodes
var _toolbar: Node
var _hover_area: Control

# Signals
signal toolbar_opened
signal toolbar_closed

func _init(toolbar: Node):
	_toolbar = toolbar
	name = "ToolbarAnimator"
	
	# Create hover area
	_hover_area = Control.new()
	_hover_area.name = "HoverArea"
	_hover_area.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(_hover_area)
	
	# Create and configure hover timer
	_hover_timer = Timer.new()
	_hover_timer.name = "HoverTimer"
	_hover_timer.one_shot = true
	_hover_timer.wait_time = 0.2
	add_child(_hover_timer)
	
	# Connect signals
	_hover_area.mouse_entered.connect(_on_hover_area_entered)
	_hover_area.mouse_exited.connect(_on_hover_area_exited)
	_hover_timer.timeout.connect(_on_hover_timer_timeout)

func _ready():
	# Position the toolbar initially
	if _toolbar is Control:
		_toolbar.position.y = closed_position
	
	# Size and position the hover area
	call_deferred("_setup_hover_area")

func _setup_hover_area():
	if _toolbar and _toolbar is Control:
		var toolbar_rect = _toolbar.get_rect()
		_hover_area.position = Vector2(0, toolbar_rect.size.y)
		_hover_area.size = Vector2(toolbar_rect.size.x, hover_margin)

func _on_hover_area_entered() -> void:
	_hover_timer.start()

func _on_hover_area_exited() -> void:
	_hover_timer.stop()
	if _is_open:
		close_toolbar()

func _on_hover_timer_timeout() -> void:
	open_toolbar()

func open_toolbar() -> void:
	if _is_open or not _toolbar or not _toolbar is Control:
		return
		
	# Cancel any existing animations
	if _tween and _tween.is_valid():
		_tween.kill()
	
	# Create new animation
	_tween = create_tween().set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_CUBIC)
	_tween.tween_property(_toolbar, "position:y", opened_position, animation_speed)
	_is_open = true
	
	emit_signal("toolbar_opened")

func close_toolbar() -> void:
	if not _is_open or not _toolbar or not _toolbar is Control:
		return
		
	# Cancel any existing animations
	if _tween and _tween.is_valid():
		_tween.kill()
	
	# Create new animation
	_tween = create_tween().set_ease(Tween.EASE_IN).set_trans(Tween.TRANS_CUBIC)
	_tween.tween_property(_toolbar, "position:y", closed_position, animation_speed)
	_is_open = false
	
	emit_signal("toolbar_closed")

func toggle_toolbar() -> void:
	if _is_open:
		close_toolbar()
	else:
		open_toolbar()

# This function can be called from C# to update animator when the C# toolbar changes
func update_toolbar_visuals(state_data: Dictionary = {}) -> void:
	if not state_data.is_empty():
		# Handle state-specific visual changes here
		# Example: Different appearance based on block count or state
		pass