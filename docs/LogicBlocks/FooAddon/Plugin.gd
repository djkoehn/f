@tool
extends EditorPlugin

const FooComponentScene = preload("res://addons/FooAddon/scenes/FooComponent.tscn")

func _enter_tree():
    # Register custom types
    add_custom_type(
        "FooComponent",
        "Node",
        preload("res://addons/FooAddon/scripts/FooComponent.gd"),
        null # You can add an icon here if needed
    )

func _exit_tree():
    # Clean up custom types
    remove_custom_type("FooComponent")
