using NLua;

using game.util;
using game.player;

namespace game.core.effects {
    class ActivatedEffect {
        
        public string Zone { get; }
        public LuaFunction CheckF { get; }
        public LuaFunction CostF { get; }
        public LuaFunction EffectF { get; }

        public ActivatedEffect(LuaTable table) {
            Zone = Utility.TableGet<string>(table, "zone");
            CheckF = Utility.TableGet<LuaFunction>(table, "checkF");
            CostF = Utility.TableGet<LuaFunction>(table, "costF");
            EffectF = Utility.TableGet<LuaFunction>(table, "effectF");
        }

        private bool CheckFunction(LuaFunction f, Lua lState, Player player, Dictionary<string, object> args) {
            var tArgs = Utility.CreateTable(lState, args);
            var returned = f.Call(player.ToLuaTable(lState), tArgs);
            return Utility.GetReturnAsBool(returned);

        }

        public bool ExecCheck(Lua lState, Player player, Dictionary<string, object> args) => CheckFunction(CheckF, lState, player, args);
        public bool ExecCosts(Lua lState, Player player, Dictionary<string, object> args) => CheckFunction(CostF, lState, player, args);
        public void ExecEffect(Lua lState, Player player, Dictionary<string, object> args) => EffectF.Call(player.ToLuaTable(lState), Utility.CreateTable(lState, args));
    }


    class Trigger : ActivatedEffect {        
        public string On { get; }
        public bool IsSilent { get; }

        public Trigger(LuaTable table) : base(table) {
            On = Utility.TableGet<string>(table, "on");
            IsSilent = Utility.GetBool(table, "isSilent");
        }

        
    }
}