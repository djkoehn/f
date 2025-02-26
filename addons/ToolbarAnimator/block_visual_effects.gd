@tool
class_name BlockVisualEffects
extends Node

# Visual effect properties
@export var hover_scale: Vector2 = Vector2(1.1, 1.1)
@export var normal_scale: Vector2 = Vector2(1.0, 1.0)
@export var hover_animation_speed: float = 0.15
@export var click_animation_speed: float = 0.1
@export var highlight_color: Color = Color(1.0, 1.0, 1.0, 0.3)

# Private variables
var _block: Node
var _tween: Tween
var _is_hovering: bool = false
var _highlight: ColorRect

func _init(block: Node):
	_block = block
	name = "BlockVisualEffects"
	
	# Create highlight effect
	_highlight = ColorRect.new()
	_highlight.name = "Highlight"
	_highlight.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_highlight.color = highlight_color
	_highlight.visible = false
	
	# We'll add the highlight in _ready after we can access the block's size

func _ready():
	if not _block:
		push_error("Block reference is null")
		return
		
	# Size the highlight to match the block
	if _block is Control:
		_highlight.size = _block.size
		_block.add_child(_highlight)
		_highlight.show_behind_parent = true
		
		# Set up mouse interaction if this is a Control
		if not _block.mouse_entered.is_connected(_on_mouse_entered):
			_block.mouse_entered.connect(_on_mouse_entered)
		if not _block.mouse_exited.is_connected(_on_mouse_exited):
			_block.mouse_exited.connect(_on_mouse_exited)
		if not _block.gui_input.is_connected(_on_gui_input):
			_block.gui_input.connect(_on_gui_input)

func _on_mouse_entered() -> void:
	_is_hovering = true
	
	# Cancel any existing animation
	if _tween and _tween.is_valid():
		_tween.kill()
	
	# Create new hover animation
	_tween = create_tween().set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_CUBIC)
	_tween.tween_property(_block, "scale", hover_scale, hover_animation_speed)
	
	# Show highlight
	if _highlight:
		_highlight.visible = true

func _on_mouse_exited() -> void:
	_is_hovering = false
	
	# Cancel any existing animation
	if _tween and _tween.is_valid():
		_tween.kill()
	
	# Create new normal animation
	_tween = create_tween().set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_CUBIC)
	_tween.tween_property(_block, "scale", normal_scale, hover_animation_speed)
	
	# Hide highlight
	if _highlight:
		_highlight.visible = false

func _on_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		# Just add visual feedback for click - logic is handled by C#
		if _tween and _tween.is_valid():
			_tween.kill()
			
		_tween = create_tween().set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_ELASTIC)
		var target_scale = hover_scale if _is_hovering else normal_scale
		_tween.tween_property(_block, "scale", target_scale * 0.9, click_animation_speed)
		_tween.tween_property(_block, "scale", target_scale, click_animation_speed)

# This method can be used by C# code to trigger visual effects
func play_effect(effect_name: String, params: Dictionary = {}) -> void:
	match effect_name:
		"highlight":
			if _highlight:
				_highlight.visible = params.get("visible", true)
				if params.has("color"):
					_highlight.color = params.get("color")
		
		"pulse":
			var intensity = params.get("intensity", 1.2)
			var duration = params.get("duration", 0.3)
			
			if _tween and _tween.is_valid():
				_tween.kill()
				
			_tween = create_tween().set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_ELASTIC)
			_tween.tween_property(_block, "scale", normal_scale * intensity, duration * 0.5)
			_tween.tween_property(_block, "scale", normal_scale, duration * 0.5)
		
		"shake":
			var intensity = params.get("intensity", 5.0)
			var duration = params.get("duration", 0.3)
			
			if _tween and _tween.is_valid():
				_tween.kill()
				
			var original_position = _block.position
			_tween = create_tween().set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_SINE)
			
			for i in range(5):
				var offset = Vector2(randf_range(- intensity, intensity), randf_range(- intensity, intensity))
				_tween.tween_property(_block, "position", original_position + offset, duration / 10)
			
			_tween.tween_property(_block, "position", original_position, duration / 10)