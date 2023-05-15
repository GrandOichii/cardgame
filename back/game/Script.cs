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
            card.ExecFunc("OnEnter", card.Info, player.ToLuaTable(_match.LState));
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
            if (player.Life <= 0) {
                var opponent = _match.OpponentOf(player);
                _match.Winner = opponent;
            }
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
        public long DealDamageToPlayer(string sourceID, int targetID, long amount) {
            // TODO? need source id
            var source = _match.AllCards[sourceID];
            var cards = _match.GetDamageableCards();
            var target = GetPlayer(targetID);
            if (target is null) throw new Exception("Failed to get damageable card in play with id " + targetID);
            var result = target.ProcessDamage(_match, amount);
            Logger.Instance.Log("ScriptMaster", "Player " + target.ShortStr() + " was dealt " + result + " damage");
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
        public void RequestPlaceInUnits(string cID, int pID) {
            var player = GetPlayer(pID);
            var card = GetCard(cID);
            var cName = card.GetCardWrapper().Original.Name;
            if (card.GetCardWrapper().Original.Type != "Unit") throw new Exception("Player " + player.ShortStr() + " tried to place a non-unit card " + card.ShortStr() + " into a lane");
            var result = player.Controller.PromptLane("Choose where to place " + cName, player, _match);

            // TODO replace with simple re-request
            if (result >= _match.Config.LaneCount) throw new Exception("Player " + player.ShortStr() + " tried to place unit " + card.ShortStr() + " in lane " + result);

            // replace unit if present
            PlaceInUnits(cID, pID, result);
            
        }


        [LuaCommand]
        public void PlaceInUnits(string cID, int pID, int lane) {
            var player = GetPlayer(pID);
            var card = GetCard(cID);

            var lanes = player.Lanes;
            UnitW? replaced = lanes[lane];
            if (replaced is not null) {
                replaced.Card.ExecFunc("LeavePlay", replaced.Card.Info, player.ToLuaTable(_match.LState));
                player.PlaceIntoDiscard(replaced);
            }

            lanes[lane] = new UnitW(card);
            Logger.Instance.Log("ScriptMaster", "Player " + player.ShortStr() + " placed unit " + card.ShortStr() + " into lane " + lane + (replaced is not null ? ", replacing unit " + replaced.Card.ShortStr() : ""));
            card.ExecFunc("OnEnter", card.Info, player.ToLuaTable(_match.LState));
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
    
    
        [LuaCommand]
        public int PickLane(int pID) {
            var player = GetPlayer(pID);
            var laneI = player.Controller.PromptLane("pick", player, _match);
            return laneI;
        }


        [LuaCommand]
        public LuaTable GetPlayers() {
            var result = new List<object?>();
            foreach (var player in _match.Players)
                result.Add(player.ToLuaTable(_match.LState));
            return Utility.CreateTable(_match.LState, result);
        }


        [LuaCommand]
        public LuaTable PlayerByID(int pID) {
            return GetPlayer(pID).ToLuaTable(_match.LState);
        }


        [LuaCommand]
        public string PromptPlayer(int pID, string prompt, string type, LuaTable args, string sourceID="") {
            var player = GetPlayer(pID);
            List<string> aArgs = new();
            foreach (var a in args.Values) {
                var n = a as string;
                // System.Console.WriteLine(a);
                if (n is null) throw new Exception("Cannot convert " + a + " to string");
                aArgs.Add(n);
            }
            return player.Controller.Prompt(type, prompt, aArgs, player, _match, sourceID);
        }


        // TODO not tested
        [LuaCommand]
        public void Emit(string signal, LuaTable table) {
            var aArgs = new Dictionary<string, object>();
            foreach (var key in table.Keys) {
                var sKey = key as string;
                if (sKey is null) throw new Exception("Emit argument key is null");
                aArgs.Add(sKey, table[key]);
            }
            _match.Emit(signal, aArgs);
        }


        [LuaCommand]
        public void Destroy(string cID) {
            var cardDict = _match.HasMarkedDamageDict();
            foreach (var pair in cardDict) {
                var player = pair.Key;
                foreach (var card in pair.Value) {
                    if (card.Card.ID != cID) continue;
                    card.Destroy(_match);
                    return;
                }                
            }
            // TODO? remove
            throw new Exception("Failed to destroy card with id " + cID + ": it's not in play");
        }


        [LuaCommand]
        public LuaTable SummonCard(string colName, string cName) {
            var template = _match.Game.CardMaster.Get(cName, colName);
            var t = template.ConstructWrapper(_match.LState, true);
            _match.AllCards.Add(t);
            return t.Info;
        }


        [LuaCommand]
        public void PlaceOnTopOfDeck(int pID, string cID) {
            var player = GetPlayer(pID);
            var card = GetCard(cID);

            player.Deck.AddToFront(card);
        }


        [LuaCommand]
        public void ShuffleDeck(int pID) {
            var player = GetPlayer(pID);
            player.Deck.Shuffle();
        }


        [LuaCommand]
        public void RemoveFromHand(string cID, int pID) {
            var player = GetPlayer(pID);
            var card = GetCard(cID);
            player.Hand.Cards.Remove(card);
        }
    }
}
