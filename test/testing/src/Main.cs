namespace FTesting;

using Godot;

#if DEBUG
using System.Reflection;
using Chickensoft.GoDotTest;
#endif

// This entry-point file is responsible for determining if we should run tests.
//
// If you want to edit your game's main entry-point, please see Game.tscn and
// Game.cs instead.

public partial class Main : Node2D {
#if DEBUG
  public TestEnvironment Environment = default!;
#endif

  public override void _Ready() {
#if DEBUG
    // If this is a debug build, use GoDotTest to examine the
    // command line arguments and determine if we should run tests.
    Environment = TestEnvironment.From(OS.GetCmdlineArgs());
    if (Environment.ShouldRunTests) {
      // Add debug logging
      var assembly = Assembly.GetExecutingAssembly();
      GD.Print($"Looking for tests in assembly: {assembly.FullName}");
      foreach (var type in assembly.GetTypes()) {
        if (typeof(TestClass).IsAssignableFrom(type) && !type.IsAbstract) {
          GD.Print($"Found test class: {type.FullName}");
        }
      }
      CallDeferred("RunTests");
      return;
    }
#endif

    // If we don't need to run tests, we can just switch to the game scene.
    CallDeferred("RunScene");
  }

#if DEBUG
  private void RunTests()
    => _ = GoTest.RunTests(Assembly.GetExecutingAssembly(), this, Environment);
#endif

  private void RunScene()
    => GetTree().ChangeSceneToFile("res://src/Game.tscn");
}
