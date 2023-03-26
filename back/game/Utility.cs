using NLua;

using game.exceptions;

namespace game.util {
    static class Utility {
        static public Random Rnd { get; }=new();
        static public List<T> Shuffled<T>(List<T> list) {
            return list.OrderBy(a => Rnd.Next()).ToList();
        }
        
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

        static public int GetInt(LuaTable table, string name) {
            var f = table[name] as int?;
            if (f is null) throw new GLuaTableException(table, "Failed to get int " + name + " from Lua table ");
            // TODO bad cast?
            return (int)f;
        }

        // static public T TableGet<T>(LuaTable table, string name) {
        //     var f = table[name] as T;
        //     if (f is null) throw new GLuaTableException(table, "Failed to get T " + name + " from Lua table ");
        //     // TODO bad cast?
        //     return (T)f;
        // }

        static public LuaTable CreateTable(Lua lState) {
            lState.NewTable("_table");
            return lState.GetTable("_table");
        }

        static void CheckIndex(object[] returned, int index) {
            if (index < returned.Length) return;

            throw new Exception("Can't access return value with index " + index + ": total amount of returned values is " + returned.Length);
        }

        static public bool GetReturnAsBool(object[] returned, int index=0) {
            CheckIndex(returned, index);
            var result = returned[index] as bool?;
            if (result is null) throw new Exception("Return value in index " + index + " is not a table");
            // TODO casting is bad
            return (bool)result;
        }

        static public LuaTable GetReturnAsTable(object[] returned, int index=0) {
            CheckIndex(returned, index);
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