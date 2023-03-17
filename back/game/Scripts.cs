using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLua;

using game.match;

namespace game.scripts
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class LuaCommand : Attribute
    {
    }

    class ScriptMaster
    {
        private Match _match;
        public ScriptMaster(Match parent) {

            _match = parent;

            var type = typeof(ScriptMaster);
            foreach (var method in type.GetMethods())
            {
                if (method.GetCustomAttribute(typeof(LuaCommand)) is not null)
                {
                    _match.LState[method.Name] = method.CreateDelegate(Expression.GetDelegateType(
                        (from parameter in method.GetParameters() select parameter.ParameterType)
                        .Concat(new[] { method.ReturnType })
                    .ToArray()), this);
                }
            }

        }

        [LuaCommand]
        public void TestCommand()
        {
            Console.WriteLine("TESTING");
        }

        [LuaCommand]
        public object? PlayerByID(int pid) {
            var player = _match.PlayerByID(pid);
            if (player is null) {
                System.Console.WriteLine("WARN: tried to get player with invalid id: " + pid);
                return null;
            }
            return player.ToLuaTable(_match.LState);
        }

        [LuaCommand]
        public void PlaceIntoDiscard(string cid, int pid) {
            var player = _match.PlayerByID(pid);
            if (player is null) throw new Exception("No player with ID " + pid);

            var card = _match.AllCards[cid];
            if (card is null) throw new Exception("No card with ID " + cid);

            player.Discard.AddToBack(card); 
        }

    }
}
