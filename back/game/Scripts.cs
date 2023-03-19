using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLua;

using game.player;
using game.match;

namespace game.scripts
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class LuaCommand : Attribute {}

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

        private Player GetPlayer(int pid) {
            var player = _match.PlayerByID(pid);
            if (player is null) throw new Exception("No player with ID " + pid);
            return player;
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
        public void PlaceIntoZone(string cid, int pid, string zoneName) {
            var player = GetPlayer(pid);

            if (!player.Zones.ContainsKey(zoneName)) throw new Exception("No zone with name " + zoneName);
            var zone = player.Zones[zoneName];

            var card = _match.AllCards[cid];
            if (card is null) throw new Exception("No card with ID " + cid);

            zone.AddToBack(card); 

        }

        [LuaCommand]
        public void PlaceIntoDiscard(string cid, int pid) {
            PlaceIntoZone(cid, pid, Player.DISCARD_ZONE_NAME);
        }

        [LuaCommand]
        public void PlaceIntoPlay(string cid, int pid) {
            PlaceIntoZone(cid, pid, Player.IN_PLAY_ZONE_NAME);
        }

        [LuaCommand]
        public void TakeEnergy(int pid, int amount) {
            var player = GetPlayer(pid);
            
            player.Energy -= amount;
        }

        [LuaCommand]
        public void IncreaseMaxEnergy(int pid, int amount) {
            var player = GetPlayer(pid);

            player.MaxEnergy += amount;
            player.Energy += amount;
        }

        [LuaCommand]
        public LuaTable GetController(string cid) {
            foreach (var player in _match.Players)
                foreach (var zone in player.Zones.Values)
                    foreach (var card in zone.Cards)
                        if (card.ID == cid) return player.ToLuaTable(_match.LState);
            throw new Exception("Failed to find owner of card with ID:" + cid);
        }

        [LuaCommand]
        public int GainLife(int pid, int amount) {
            if (amount < 0) {
                System.Console.WriteLine("WARN: player with id " + pid + " tried to gain negative amount of life: " + amount);
                return 0;
            }
            var player = GetPlayer(pid);
            var life = player.Life;
            player.Life += amount;
            return player.Life - life;
        }

    }
}
