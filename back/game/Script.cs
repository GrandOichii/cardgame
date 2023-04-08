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
        public long LoseLife(int pID, long amount) {
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
        public long GainLife(int pID, long amount) {
            var player = GetPlayer(pID);

            if (amount < 0) {
                Logger.Instance.Log("WARN", "Player " + player.ShortStr() + " tried to gain " + amount + " life, setting it to 0");
                amount = 0;
            }

            var prevLife = player.Life;
            player.Life += amount;
            var result = player.Life - prevLife;
            Logger.Instance.Log("ScriptMaster", "Player " + player.ShortStr() + " gained " + result + " life");
            _match.Emit("life_gain", new(){{"player", player.ToLuaTable(_match.LState)}, {"amount", amount}});
            return result;
        }


        [LuaCommand]
        public LuaTable GetController(string cID) {
            return _match.OwnerOf(cID).ToLuaTable(_match.LState);
        }


        [LuaCommand]
        public long DealDamage(string sourceID, string targetID, long amount) {
            // TODO? need source id
            var source = _match.AllCards[sourceID];
            var cards = _match.GetDamageableCards();
            var target = cards.Find(card => card.GetCardWrapper().ID == targetID);
            if (target is null) throw new Exception("Failed to get damageable card in play with id " + targetID);
            var result = target.ProcessDamage(_match, amount);
            Logger.Instance.Log("ScriptMaster", "Card " + target.GetCardWrapper().ShortStr() + " was dealt " + result + " damage");
            return result;
        }


        [LuaCommand]
        public int DrawCards(int pID, int amount) {
            var player = GetPlayer(pID);
            var original = player.Hand.Cards.Count;
            player.DrawCards(amount);
            return player.Hand.Cards.Count - original;
        }


        [LuaCommand]
        public void PlaceInUnits(string cID, int pID) {
            var player = GetPlayer(pID);
            var card = GetCard(cID);
            var cName = card.GetCardWrapper().Original.Name;
            if (card.GetCardWrapper().Original.Type != "Unit") throw new Exception("Player " + player.ShortStr() + " tried to place a non-unit card " + card.ShortStr() + " into a lane");
            var result = player.Controller.PromptLane("Choose where to place " + cName, player, _match);

            // TODO replace with simple re-request
            if (result >= _match.Config.LaneCount) throw new Exception("Player " + player.ShortStr() + " tried to place unit " + card.ShortStr() + " in lane " + result);

            // replace unit if present
            var lanes = player.Lanes;
            UnitW? replaced = lanes[result];
            if (replaced is not null) {
                player.PlaceIntoDiscard(replaced);
                replaced.Card.ExecFunc("LeavePlay", replaced.Card.Info, player.ToLuaTable(_match.LState));
            }

            lanes[result] = new UnitW(card);
            Logger.Instance.Log("ScriptMaster", "Player " + player.ShortStr() + " placed unit " + card.ShortStr() + " into lane " + result + (replaced is not null ? ", replacing unit " + replaced.Card.ShortStr() : ""));
            if (replaced is null) return;

            _match.Emit("unit_replaced", new(){{"unit", replaced.Card.Info}}); 
        }


        [LuaCommand]
        public void RemoveFromDiscard(string cID, int pID) {
            var card = GetCard(cID);
            var player = GetPlayer(pID);
            var removed = player.Discard.Cards.Remove(card);
            if (removed) return;

            throw new Exception("Tried to remove card " + card.ShortStr() + " from " + player.Name + "'s discard, which is not there");
        }


        [LuaCommand]
        public void PlaceIntoHand(string cID, int pID) {
            var card = GetCard(cID);
            var player = GetPlayer(pID);
            player.Hand.AddToBack(card);            
        }
    }
}
