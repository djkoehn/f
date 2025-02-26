@tool
extends Node

# This script is used to test the integration between GDScript visual effects
# and C# logic. It should be attached to a node in the scene for testing purposes.

func _ready():
	print("Testing GDScript/C# integration...")
	
	# Find the toolbar in the scene
	var toolbar = get_node_or_null("/root/Main/Toolbar")
	if toolbar:
		print("Found toolbar: ", toolbar.name)
		
		# Check if ToolbarAnimator was attached
		var animator = toolbar.get_node_or_null("ToolbarAnimator")
		if animator:
			print("ToolbarAnimator found and attached!")
		else:
			push_error("ToolbarAnimator not found on toolbar")
	else:
		push_error("Main toolbar not found in scene")
	
	# Find some blocks to test visual effects
	var blocks = get_tree().get_nodes_in_group("blocks")
	if blocks.size() > 0:
		print("Found ", blocks.size(), " blocks for testing")
		
		# Check the first block
		var block = blocks[0]
		var effects = block.get_node_or_null("BlockVisualEffects")
		if effects:
			print("BlockVisualEffects found on block: ", block.name)
		else:
			push_error("BlockVisualEffects not found on block: " + block.name)
	else:
		push_warning("No blocks found for testing")
	
	print("Integration test completed")