@tool
extends EditorPlugin

func _enter_tree():
	# Register the addon
	print("ToolbarAnimator addon registered")

func _exit_tree():
	# Clean up the addon
	print("ToolbarAnimator addon unregistered")