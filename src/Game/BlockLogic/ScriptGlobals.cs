using F.Game.Connections;
using F.Game.Tokens;

namespace F.Game.BlockLogic
{
    public class ScriptGlobals
    {
        public BaseBlock Block { get; set; }
        public ConnectionManager ConnectionManager { get; set; }
        public Token Token { get; set; }

        public ScriptGlobals(BaseBlock block, ConnectionManager connectionManager, Token token)
        {
            Block = block;
            ConnectionManager = connectionManager;
            Token = token;
        }
    }
} 