using System.Linq.Expressions;
using System.Reflection;
using NLua;

using game.match;
using game.player;
using game.cards;
using game.util;

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

        private Player GetPlayer(int pID) {
            var player = _match.PlayerByID(pID);
            if (player is null) throw new Exception("No player with ID " + pID);
            return player;
        }


        private CardW GetCard(string cID) {
            var card = _match.AllCards[cID];
            if (card is null) throw new Exception("No card with ID " + cID);
            return card;
        }


        [LuaCommand]
        public void Log(string message) {
            Logger.Instance.Log("LSTATE", message);
        }


        [LuaCommand]
        public void PlaceIntoDiscard(string cID, int pID) {
            var card = GetCard(cID);
            var player = GetPlayer(pID);
            Logger.Instance.Log("ScriptMaster", "Card " + card.ShortStr() + " is put into " + player.ShortStr() + "'s discard");

            player.Discard.AddToBack(card);
        }


        [LuaCommand]
        public void PlaceInTreasures(string cID, int pID) {
            var card = GetCard(cID);
            var player = GetPlayer(pID);
            if (card.Original.Type != "Treasure") throw new Exception("Player" + player.ShortStr() + "tried to place a non-treasure card " + card.ShortStr() + " into treasure zone");
            
            Logger.Instance.Log("ScriptMaster", "Card " + card.ShortStr() + " is put into " + player.ShortStr() + "'s treasure zone");
            player.Treasures.AddToBack(new TreasureW(card));
        }

        
        [LuaCommand]
        public void IncreaseMaxEnergy(int pID, int amount) {
            var player = GetPlayer(pID);
            player.MaxEnergy += amount;
            player.Energy += amount;
            Logger.Instance.Log("ScriptMaster", "Increased max amount of energy of player " + player.ShortStr() + " by " + amount);
        }


        [LuaCommand]
        public void PayEnergy(int pID, int amount) {
            var player = GetPlayer(pID);
            player.Energy -= amount;
        }


        [LuaCommand]
        public LuaTable OpponentOf(int pID) {
            var player = GetPlayer(pID);
            return _match.OpponentOf(player).ToLuaTable(_match.LState);
        }


        [LuaCommand]
        public int LoseLife(int pID, int amount) {
            // !!! NOT DAMAGE, just life loss
            
            var player = GetPlayer(pID);

            if (amount < 0) {
                Logger.Instance.Log("WARN", "Player " + player.ShortStr() + " tried to lose " + amount + " life, setting it to 0");
                amount = 0;
            }

            var prevLife = player.Life;
            player.Life -= amount;
            var result = prevLife - player.Life;
            Logger.Instance.Log("ScriptMaster", "Player " + player.ShortStr() + " lost " + result + " life");
            return result;
        }


        [LuaCommand]
        public int GainLife(int pID, int amount) {
            var player = GetPlayer(pID);

            if (amount < 0) {
                Logger.Instance.Log("WARN", "Player " + player.ShortStr() + " tried to gain " + amount + " life, setting it to 0");
                amount = 0;
            }

            var prevLife = player.Life;
            player.Life += amount;
            var result = player.Life - prevLife;
            Logger.Instance.Log("ScriptMaster", "Player " + player.ShortStr() + " gained " + result + " life");
            return result;
        }


        [LuaCommand]
        public LuaTable GetController(string cID) {
            foreach (var player in _match.Players) {
                var cards = player.GetAllCards();
                var result = cards.Keys.ToList().Find(card => card.ID == cID);
                if (result is null) continue;
                return player.ToLuaTable(_match.LState);
            }
            throw new Exception("Failed to find owner of card with ID:" + cID);
        }
    }
}
