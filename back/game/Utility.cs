using NLua;

using game.exceptions;

namespace game.util {
    static class Utility {
        static public LuaFunction GetGlobalF(Lua lState, string fName) {
            var f = lState[fName] as LuaFunction;
            if (f is null) throw new Exception("Failed to get function " + fName + " from glabal Lua state");
            return f;
        } 

        static public LuaFunction GetF(LuaTable table, string fName) {
            var f = table[fName] as LuaFunction;
            if (f is null) throw new GLuaTableException(table, "Failed to get function " + fName + " from Lua table ");
            return f;
        }

        static public long GetLong(LuaTable table, string name) {
            var f = table[name] as long?;
            if (f is null) throw new GLuaTableException(table, "Failed to get long " + name + " from Lua table ");
            // TODO bad cast?
            return (long)f;
        }

        static public LuaTable CreateTable(Lua lState) {
            lState.NewTable("_table");
            return lState.GetTable("_table");
        }

        static public LuaTable GetReturnAsTable(object[] returned, int index=0) {
            if (index >= returned.Length) throw new Exception("Can't access return value with index " + index + ": total amount of returned values is " + returned.Length);
            var result = returned[index] as LuaTable;
            if (result is null) throw new Exception("Return value in index " + index + " is not a table");
            return result;
        }
    }
    
    class Logger {
        static public Logger Instance { get; private set; }=new();
        
        private Logger() {

        }

        public void Log(string prefix, string message) {
            System.Console.WriteLine("[{0}]: {1}", prefix, message);
        }
    }
}