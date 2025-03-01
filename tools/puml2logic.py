
import re
import sys
from pathlib import Path
import xml.etree.ElementTree as ET

def parse_puml(puml_content):
    # Remove comments and empty lines
    lines = [line.strip() for line in puml_content.split('\n')
            if line.strip() and not line.strip().startswith("'")]

    # Remove @startuml, @enduml, and hide empty description
    lines = [line for line in lines
            if not any(x in line for x in ['@startuml', '@enduml', 'hide empty description'])]

    states = set()
    transitions = []
    initial_state = None
    compound_states = {}

    # First pass: identify compound states and their substates
    current_compound = None

    for line in lines:
        if line.startswith('state'):
            # Check for compound state start
            if '{' in line:
                current_compound = line.split()[1]
                compound_states[current_compound] = set()
        elif line == '}':
            current_compound = None
        elif current_compound:
            # Parse transitions within compound state
            match = re.match(r'([^\-]+)\s*-->\s*([^:]+)(?:\s*:\s*(.+))?', line)
            if match:
                from_state = match.group(1).strip()
                to_state = match.group(2).strip()

                # Only add states that are actually within this compound state
                # Skip transitions that reference states outside the compound state
                if from_state != '[*]' and from_state != current_compound:
                    compound_states[current_compound].add(from_state)
                if to_state != '[*]' and to_state != current_compound:
                    compound_states[current_compound].add(to_state)

    # Second pass: parse transitions
    for line in lines:
        # Parse transitions
        match = re.match(r'([^\-]+)\s*-->\s*([^:]+)(?:\s*:\s*(.+))?', line)
        if match:
            from_state = match.group(1).strip()
            to_state = match.group(2).strip()
            action = match.group(3).strip() if match.group(3) else None

            # Handle initial state
            if from_state == '[*]':
                if initial_state is None:
                    initial_state = to_state
                continue

            # Add states and transitions
            states.add(from_state)
            states.add(to_state)
            transitions.append((from_state, to_state, action))

    # Clean up compound states - remove any cross-references
    for compound_state, substates in compound_states.items():
        # Remove any states that are themselves compound states
        substates.difference_update(compound_states.keys())

    print(f"Compound states: {compound_states}")  # Debug print
    print(f"Initial state: {initial_state}")  # Debug print
    print(f"States: {states}")  # Debug print
    print(f"Transitions: {transitions}")  # Debug print

    return initial_state, states, transitions, compound_states

def generate_csharp(namespace, class_name, initial_state, states, transitions):
    # Create transition lookup
    state_transitions = {state: [] for state in states}
    for from_state, to_state, action in transitions:
        state_transitions[from_state].append((to_state, action))

    code = f"""namespace {namespace};

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class {class_name} : LogicBlock<{class_name}.State> {{
    public override Transition GetInitialState() => To<{initial_state}>();

    public abstract partial record State : StateLogic<State> {{
        {generate_base_actions(transitions)}
    }}

"""

    # Generate state records
    for state in states:
        code += generate_state_record(state, state_transitions[state])

    code += "}\n"
    return code

def generate_base_actions(transitions):
    # We don't need virtual methods in base state anymore
    return "// States implement IGet<Input.X> for their actions"

def generate_state_record(state, transitions):
    if not transitions:
        return f"""    public sealed partial record {state} : State {{
    }}

"""
    # Collect all actions this state needs to handle
    actions = [action for _, action in transitions if action]
    iget_interfaces = ", ".join([f"IGet<Input.{action}>" for action in actions if action])

    # Start building the state with interfaces
    state_def = f"""    public sealed partial record {state} : State{f", {iget_interfaces}" if iget_interfaces else ""} {{
"""

    # Add transitions
    for to_state, action in transitions:
        if action:
            state_def += f"""        public Transition On(in Input.{action} input) => To<{to_state}>();

"""
        else:
            state_def += f"""        public Transition OnEnter() => To<{to_state}>();

"""

    state_def += "    }\n\n"
    return state_def

def get_relative_namespace(file_path):
    # Convert to absolute path if it's not already
    file_path = file_path.absolute()

    # Find the src directory by searching up the directory tree
    current = file_path.parent
    while current.name != 'src' and current != current.parent:
        current = current.parent

    if current.name != 'src':
        # If we're already in src/tools, use current directory
        if 'src' in file_path.parts and 'tools' in file_path.parts:
            return 'tools'
        raise Exception("Could not find 'src' directory in parent path")

    # Get path relative to src directory
    rel_path = file_path.parent.relative_to(current)
    return str(rel_path).replace('/', '.')

def generate_main_file(namespace, class_name, initial_state):
    return f"""namespace {namespace};

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class {class_name} : LogicBlock<{class_name}.State> {{
  public override Transition GetInitialState() => To<State.{initial_state}>();
}}
"""

def generate_state_base_file(namespace, class_name):
    return f"""namespace {namespace};

using Chickensoft.LogicBlocks;

public partial class {class_name} {{
  public abstract partial record State : StateLogic<State>;
}}
"""

def generate_input_file(namespace, class_name, transitions):
    # Extract unique actions from transitions
    actions = set()
    for _, _, action in transitions:
        if action:
            actions.add(action)

    # Generate input records for each action
    input_records = "\n        ".join(
        f"public readonly record struct {action};" for action in sorted(actions)
    )

    if not input_records:
        input_records = "// No input records needed"

    return f"""namespace {namespace};

using Chickensoft.LogicBlocks;

public partial class {class_name} {{
    public static class Input {{
        {input_records}
    }}
}}
"""

def generate_output_file(namespace, class_name):
    return f"""namespace {namespace};

using Chickensoft.LogicBlocks;

public partial class {class_name} {{
    public static class Output {{
    }}
}}
"""

def generate_state_file(namespace, class_name, state, transitions, compound_states):
    # Find if this state is a substate of any compound state
    parent_state = None
    for compound, substates in compound_states.items():
        if state in substates:
            parent_state = compound
            break

    # Check if this state is a compound state
    is_compound = state in compound_states

    # Determine base state - if it's a nested state, inherit from compound state
    base_state = parent_state if parent_state else "State"

    # Only add sealed if it's not a compound state
    state_modifier = "" if is_compound else "sealed "

    # Collect all actions this state needs to handle
    actions = [action for _, action in transitions if action]
    iget_interfaces = ", ".join([f"IGet<Input.{action}>" for action in actions if action])

    if not transitions and not is_compound:
        return f"""namespace {namespace};

public partial class {class_name} {{
  public partial record State {{
    public {state_modifier}partial record {state} : {base_state} {{
    }}
  }}
}}
"""

    transition_code = []
    for to_state, action in transitions:
        if action:
            transition_code.append(
                f"      public Transition On(in Input.{action} input) => To<{to_state}>();\n"
            )
        else:
            transition_code.append(
                f"      public Transition OnEnter() => To<{to_state}>();\n"
            )

    return f"""namespace {namespace};

public partial class {class_name} {{
  public partial record State {{
    public {state_modifier}partial record {state} : {base_state}{f", {iget_interfaces}" if iget_interfaces else ""} {{
{(''.join(transition_code)).rstrip()}
    }}
  }}
}}
"""

def main():
    if len(sys.argv) != 3:
        print("Usage: python puml2logic.py <base_namespace> <puml_file>")
        sys.exit(1)

    base_namespace = sys.argv[1]
    puml_path = Path(sys.argv[2])
    if not puml_path.exists():
        print(f"File not found: {puml_path}")
        sys.exit(1)

    # Get the relative namespace and combine with base namespace
    rel_namespace = get_relative_namespace(puml_path)
    namespace = f"{base_namespace}.{rel_namespace}" if rel_namespace else base_namespace

    # Read PUML file
    puml_content = puml_path.read_text()

    # Parse PUML with compound states
    initial_state, states, transitions, compound_states = parse_puml(puml_content)

    # Create transition lookup
    state_transitions = {state: [] for state in states}
    for from_state, to_state, action in transitions:
        if from_state != '[*]':  # Skip initial transitions
            state_transitions[from_state].append((to_state, action))

    # Generate main logic block file
    class_name = puml_path.stem
    code = generate_main_file(namespace, class_name, initial_state)
    output_path = puml_path.with_suffix('.cs')
    output_path.write_text(code)
    print(f"Generated LogicBlock code: {output_path}")

    # Generate base State file
    state_code = generate_state_base_file(namespace, class_name)
    state_base_path = puml_path.parent / f"{class_name}.State.cs"
    state_base_path.write_text(state_code)
    print(f"Generated base State file: {state_base_path}")

    # Generate state files
    states_dir = puml_path.parent / "states"
    states_dir.mkdir(exist_ok=True)

    for state in states:
        if state != '[*]':  # Skip generating [*] state file
            state_code = generate_state_file(namespace, class_name, state,
                                           state_transitions[state], compound_states)
            state_path = states_dir / f"{class_name}.State.{state}.cs"
            state_path.write_text(state_code)
            print(f"Generated state file: {state_path}")

    # Generate Input file
    input_code = generate_input_file(namespace, class_name, transitions)
    input_path = puml_path.parent / f"{class_name}.Input.cs"
    input_path.write_text(input_code)
    print(f"Generated Input file: {input_path}")

    # Generate Output file
    output_code = generate_output_file(namespace, class_name)
    output_path = puml_path.parent / f"{class_name}.Output.cs"
    output_path.write_text(output_code)
    print(f"Generated Output file: {output_path}")

if __name__ == "__main__":
    main()
