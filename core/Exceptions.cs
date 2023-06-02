using NLua;

namespace game.exceptions {
    [System.Serializable]
    public class GLuaTableException : Exception
    {
        public GLuaTableException(LuaTable table, string message) : base(message) { }
    }
}