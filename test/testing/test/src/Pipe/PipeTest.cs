namespace testing.Pipe;

using Godot;
using Chickensoft.GoDotTest;
using Shouldly;

public class PipeTest : TestClass
{
    private PipeNode _pipe = default!;
    private Area2D _testBlock = default!;

    public PipeTest(Node testScene) : base(testScene) { }

    [Setup]
    public void Setup()
    {
        // Create test block
        _testBlock = new Area2D()
        {
            CollisionLayer = 4,
            CollisionMask = 2
        };
        var collision = new CollisionPolygon2D()
        {
            Polygon = new Vector2[] {
                new(-20, -20),
                new(20, -20),
                new(20, 20),
                new(-20, 20)
            }
        };
        _testBlock.AddChild(collision);
        TestScene.AddChild(_testBlock);

        // Create PipeNode
        _pipe = new PipeNode();
        TestScene.AddChild(_pipe);
        _pipe.Initialize(new Vector2(100, 100), new Vector2(300, 100));
    }

    [Cleanup]
    public void Cleanup()
    {
        _testBlock?.QueueFree();
        _pipe?.QueueFree();
    }

    [Test]
    public void TestPipeStates()
    {
        GD.Print("Starting state transition test");

        // Test your state transitions here
        // The state changes should be visible in the output
    }

    [Test]
    public void TestPipeClick()
    {
        GD.Print("Starting click test");

        // Test your click handling here
    }
}
