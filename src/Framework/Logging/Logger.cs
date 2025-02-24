using Chickensoft.Log;
using Chickensoft.Log.Godot;

namespace F.Framework.Logging;

public static class Logger
{
    private static readonly ILogWriter _writer = new GDWriter();

    private static readonly ILog _gameLogger = new Chickensoft.Log.Log(nameof(Game), _writer);
    private static readonly ILog _connectionLogger = new Chickensoft.Log.Log(nameof(Connection), _writer);
    private static readonly ILog _blockLogger = new Chickensoft.Log.Log(nameof(Block), _writer);
    private static readonly ILog _tokenLogger = new Chickensoft.Log.Log(nameof(Token), _writer);
    private static readonly ILog _uiLogger = new Chickensoft.Log.Log(nameof(UI), _writer);

    public static ILog Game => _gameLogger;
    public static ILog Connection => _connectionLogger;
    public static ILog Block => _blockLogger;
    public static ILog Token => _tokenLogger;
    public static ILog UI => _uiLogger;
}