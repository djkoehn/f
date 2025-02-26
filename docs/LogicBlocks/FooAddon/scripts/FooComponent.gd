@tool
extends Node

# Expose properties in the editor
@export var auto_start: bool = true
@export var debug_mode: bool = false
@export var initial_value: float = 1.0

# Signal forwarding from LogicMachine
signal state_changed(state_name: String)
signal value_processed(id: String, value: float)

# Reference to C# LogicMachine
var logic_machine

func _ready():
    if Engine.is_editor_hint():
        return
        
    # Find or create the LogicMachine
    logic_machine = get_node_or_null("FooLogicMachine")
    
    if logic_machine == null:
        # Create the LogicMachine at runtime if it doesn't exist
        logic_machine = load("res://src/LogicBlocks/Foo/State/FooLogicMachine.cs").new()
        logic_machine.name = "FooLogicMachine"
        add_child(logic_machine)
    
    # Connect to signals
    logic_machine.state_changed.connect(_on_state_changed)
    logic_machine.data_processed.connect(_on_data_processed)
    
    if auto_start and not Engine.is_editor_hint():
        # Auto-start with initial input if configured
        call_deferred("trigger_some_input")

func _on_state_changed(state_name: String):
    if debug_mode:
        print("State changed to: ", state_name)
    
    # Forward the signal
    state_changed.emit(state_name)
    
func _on_data_processed(data: String):
    if debug_mode:
        print("Data processed: ", data)
    
    # Forward the signal with the correct signature
    value_processed.emit(data, 0.0)
    
func trigger_some_input():
    if logic_machine:
        logic_machine.SendSomeInput()
    
func trigger_another_input(data: String):
    if logic_machine:
        logic_machine.SendAnotherInput(data)

func _process(_delta):
    if Engine.is_editor_hint():
        return

func next_state() -> void:
    if logic_machine:
        pass

func previous_state() -> void:
    if logic_machine:
        pass

func process_value(value: float) -> void:
    if logic_machine:
        pass

func save_state() -> String:
    if logic_machine:
        return logic_machine.call("SerializeState")
    return ""

func load_state(state: String) -> void:
    if logic_machine:
        logic_machine.call("DeserializeState", state)
