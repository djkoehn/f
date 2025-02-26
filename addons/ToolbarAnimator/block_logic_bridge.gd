@tool
extends Node

## This script demonstrates how to use the C# LogicBlock bridge from GDScript.
## Attach this script to a node that has a BlockLogicMachine as a child or parent.

# The BlockLogicMachine node to interact with
var logic_machine: Node
# The bridge dictionary containing methods to interact with the LogicBlock
var logic_bridge: Dictionary

# Signal that will be emitted when the state changes
signal state_changed(state_name: String, state_data: Dictionary)

func _ready() -> void:
	# Find the BlockLogicMachine node
	logic_machine = find_logic_machine()
	if not logic_machine:
		push_error("BlockLogicMachine not found")
		return
	
	# Connect to the state changed signal
	logic_machine.connect("StateChanged", _on_state_changed)
	
	# Create the bridge
	var bridge_service = Engine.get_singleton("CSharpGDScriptBridge")
	if not bridge_service:
		push_error("CSharpGDScriptBridge singleton not found")
		return
	
	# Get the bridge dictionary
	logic_bridge = bridge_service.call("BridgeLogicBlock", logic_machine, "BlockLogic")
	
	# Log the current state
	print("Current state: ", get_state_name())

# Find the BlockLogicMachine node in the scene
func find_logic_machine() -> Node:
	# First check if we have a direct child
	for child in get_children():
		if child.get_class() == "BlockLogicMachine":
			return child
	
	# Then check if we have a parent
	var parent = get_parent()
	if parent and parent.get_class() == "BlockLogicMachine":
		return parent
	
	# Then check siblings
	if parent:
		for sibling in parent.get_children():
			if sibling.get_class() == "BlockLogicMachine":
				return sibling
	
	# Finally, try to find by name
	return get_node_or_null("../BlockLogicMachine")

# Get the current state name
func get_state_name() -> String:
	if logic_bridge.has("get_state_name"):
		return logic_bridge["get_state_name"].call()
	return "Unknown"

# Get the current state data
func get_state() -> Dictionary:
	if logic_bridge.has("get_state"):
		return logic_bridge["get_state"].call()
	return {}

# Send an input to the LogicBlock
func send_input(input_name: String) -> void:
	if not logic_machine:
		push_error("BlockLogicMachine not found")
		return
	
	# Call the appropriate method on the logic machine
	match input_name:
		"StartDragging":
			logic_machine.StartDragging()
		"StopDragging":
			logic_machine.StopDragging()
		"Connect":
			logic_machine.Connect()
		"Disconnect":
			logic_machine.Disconnect()
		"Interact":
			logic_machine.Interact()
		"ReturnBlock":
			logic_machine.ReturnBlock()
		"HoveredOverPipe":
			logic_machine.HoveredOverPipe()
		_:
			push_error("Unknown input: " + input_name)

# Handle state changes
func _on_state_changed(state_name: String) -> void:
	var state_data = get_state()
	print("State changed to: ", state_name, " with data: ", state_data)
	emit_signal("state_changed", state_name, state_data)

# Convenience methods for common inputs
func start_dragging() -> void:
	send_input("StartDragging")

func stop_dragging() -> void:
	send_input("StopDragging")

func connect_block() -> void:
	send_input("Connect")

func disconnect_block() -> void:
	send_input("Disconnect")

func interact() -> void:
	send_input("Interact")

func return_block() -> void:
	send_input("ReturnBlock")

func hovered_over_pipe() -> void:
	send_input("HoveredOverPipe")

# Check if the block is in a specific state
func is_in_toolbar() -> bool:
	var state = get_state()
	return state.get("is_in_toolbar", false)

func is_dragging() -> bool:
	var state = get_state()
	return state.get("is_dragging", false)

func has_connection() -> bool:
	var state = get_state()
	return state.get("is_connected", false)