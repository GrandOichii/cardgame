using NLua;

using game.collection;
using game.player;
using game.deck;
using game.match;
using game.cards;

namespace game.core {

    static class Utility {

        static public Random Rnd { get; }=new();
        static public List<T> Shuffled<T>(List<T> list) {
            return list.OrderBy(a => Rnd.Next()).ToList();
        }

        static public LuaFunction GetLuaF(LuaTable t, string fName) {
            var result = t[fName]as LuaFunction;
            if (result is null) throw new Exception("Failed to get function name " + fName + " from lua table");
            return result;
        }
    }
    
    interface IDamageable {
        public int ProcessDamage(int damage);
    }

    interface ILuaSerializable {
        abstract public LuaTable ToLuaTable(Lua lState);
    }

    interface ITextSerializable {
        abstract public string ToText();
    }

    interface IIDCreator {
        abstract public string CreateID();
    }

    class IncrementIDCreator : IIDCreator
    {
        private int _last = 0;
        public string CreateID()
        {
            _last++;
            return _last.ToString();
        }
    }

    class Game {
        public CollectionPool Collections { get; }
        public MatchPool Matches { get; }

        public Game(string collectionsPath) {
            Matches = new();

            Collections = CollectionPool.Load(collectionsPath);
        }
    }

    namespace actions
    {
        abstract class GameAction
        {
            abstract public void Exec(Match match, Player player, string[] args);
        }

        class CastCardAction : GameAction
        {
            public override void Exec(Match match, Player player, string[] args)
            {
                if (args.Length != 2) throw new Exception("Incorrect number of arguments for CastCardAction");

                var pTable = player.ToLuaTable(match.LState);

                var cID = args[1];
                var card = player.Hand[cID];

                // TODO can't cast a card with id that's not in your hand, suspect cheating
                if (card is null) throw new Exception("Player " + player.Name + " cast card with ID " + cID + ": it's not in their hand");
                // var cTable = card.Table;

                var canCastFunc = card.Table[Card.CAN_CAST_FNAME] as LuaFunction;
                if (canCastFunc is null) throw new Exception("CardWrapper with id " + cID + "(card name: " + card.Card.Name + ") doesn't have a " + Card.CAN_CAST_FNAME + " function");
                var canCast = (bool)canCastFunc.Call(pTable)[0];
                
                // TODO throw exception?
                if (!canCast) {
                    System.Console.WriteLine("WARN: player " + player.Name + " (" + player.ID + ") " + "tried to cast an uncastable catd " + card.Card.Name + " (" + card.ID + ")");
                    return;
                }

                var costFunc = card.Table[Card.CAST_COST_FNAME] as LuaFunction;
                if (costFunc is null) throw new Exception("CardWrapper with id " + cID + "(card name: " + card.Card.Name + ") doesn't have a " + Card.CAST_COST_FNAME + " function");

                // remove card from player's hand
                player.Hand.Cards.Remove(card);
                // TODO figure out the input and output args
                // var payed = (bool)costFunc.Call(cTable, pTable)[0];
                costFunc.Call(pTable);
                // TODO throw an exception?
                // if (!payed) {
                //     System.Console.WriteLine("WARN: player " + player.Name + " (" + player.ID + ") tried to cast " + card.Card.Name + " (" + card.ID + "), but FAILED for some reason");
                //     return;
                // }

                var castFunc = card.Table[Card.ON_CAST_FNAME] as LuaFunction;
                if (castFunc is null) throw new Exception("CardWrapper with id " + cID + "(card name: " + card.Card.Name + ") doesn't have a " + Card.ON_CAST_FNAME + " function");
                // TODO figure out input and output args
                castFunc.Call(pTable);
            }
        }

        class AttackAction : GameAction
        {
            public override void Exec(Match match, Player player, string[] args)
            {
                // TODO? move to structure of damage promise
                if (args.Length == 1 || args.Length > 3) throw new Exception("Incorrect number of arguments for AttackAction");

                // TODO get available attacks
                // TODO check if can attack target
                var attackerID = args[1];
                CardWrapper? attacker = null;
                foreach (var card in player.InPlay.Cards) {
                    if (card.ID != attackerID) continue;
                    attacker = card;
                    break;
                }
                if (attacker is null) 
                    throw new Exception("Player " + player.Name + " tried to attack with creature with id " + attackerID + ", which is not in play under their control");
                var availableAttacksO = attacker.Table["availableAttacks"];
                if (availableAttacksO == null) throw new Exception("Player " + player.Name + " tried to attack with a card that can't attack: " + attacker.Card.Name);
                
                long availableAttacks = (long)availableAttacksO;
                if (availableAttacks == 0) {
                    System.Console.WriteLine("WARN: Player " + player.Name + " tried to attack with " + attacker.Card.Name + " (" + attacker.ID + "), which has to available attacks");
                    return;
                }
                Player opponent = match.OpponentOf(player);
                IDamageable target = opponent;
                if (args.Length == 3) {
                    // is attacking a card in play
                    var attackedID = args[2];
                    bool set = false;
                    foreach (var card in opponent.InPlay.Cards) {
                        if (card.ID == attackedID) {
                            set = true;
                            target = card;
                            break;
                        }
                    }
                    if (!set) throw new Exception("Player " + player.Name + " tried to attack a card with ID " + attackedID + ", which is not in play");
                }

                // TODO change when implementing damage types
                long attack = (long)attacker.Table["attack"];
                // if (attack is null) throw new Exception("Player " + player.Name + " tried to attack with a  card with ID " + attackerID + ", which has no attack value");    
                
                target.ProcessDamage((int)attack);
            }
        }

    }

    namespace phases {

        abstract class GamePhase {
            abstract public void Exec(Match match, Player player);
        }

        class TurnStart : GamePhase
        {
            public override void Exec(Match match, Player player)
            {
                // replenish energy
                player.Energy = player.MaxEnergy;
                // emit turn start effects
                match.Emit("turn_start", new(){ {"player", player} });

                // replenish creatures' attacks
                foreach (var card in player.InPlay.Cards)
                    if (card.Table["availableAttacks"] is not null)
                        // TODO replace if creatures will be able to attack multiple times
                        card.Table["availableAttacks"] = 1;

                // draw for the turn
                player.DrawCards(match.Config.TurnStartCardDraw);
            }
        }

        class MainPhase : GamePhase
        {
            private readonly string PASS_TURN_ACTION = "pass";
            private static readonly Dictionary<string, actions.GameAction> ACTION_MAP =
            new(){
                { "cast", new actions.CastCardAction() },
                { "attack", new actions.AttackAction() }
            };

            public override void Exec(Match match, Player player)
            {
                string action;
                while (true)
                {
                    action = PromptAction(match, player);
                    var words = action.Split(" ");

                    var actionWord = words[0];
                    if (actionWord == PASS_TURN_ACTION) break;

                    // TODO remove
                    if (actionWord == "quit")
                    {
                        match.Winner = player;
                        return;
                    }
                    
                    if (!ACTION_MAP.ContainsKey(actionWord)) throw new Exception("Unknown action from player " + player.Name + ": " + actionWord);

                    ACTION_MAP[actionWord].Exec(match, player, words);
                    
                }
                // TODO
            }

            private string PromptAction(Match match, Player player)
            {
                // TODO get all available actions
                return player.PromptAction();
            } 
        }

        class TurnEnd : GamePhase
        {
            public override void Exec(Match match, Player player)
            {
                match.Emit("turn_end", new(){ {"player", player} });

                // discard to hand size
                int discarded = player.PromptDiscard(match.Config.MaxHandSize - player.Hand.Cards.Count, true);
            }
        }
    }

    class CardDeck {
        public List<CardWrapper> _cards;
        public List<CardWrapper> Cards { get => _cards; }

        public CardWrapper? this[string cardID]
        {
            get => _cards.Find(card => card.ID == cardID);
        }

        public CardDeck(List<CardWrapper> cards) {
            _cards = cards;
        }

        public void Shuffle() {
            _cards = Utility.Shuffled(Cards);
        }

        public List<CardWrapper> PopTop(int amount) {
            if (amount > _cards.Count) amount = _cards.Count;
            var result = Cards.GetRange(0, amount);
            Cards.RemoveRange(0, amount);
            return result;
        }

        static public CardDeck From(Match match, Deck deck) {
            var cards = new List<CardWrapper>();
            foreach (var pair in deck.Cards)
                for (int i = 0; i < pair.Value; i++)
                    cards.Add(new CardWrapper(match, pair.Key));
            return new CardDeck(cards);
        }

        public void AddToBack(List<CardWrapper> cards) {
            foreach (var card in cards)
                AddToBack(card);
        }

        public void AddToBack(CardWrapper card) {
            _cards.Add(card);
        }
    }

    class Trigger {
        
        public bool IsSilent { get; }
        public string Zone { get; }
        public string On { get; }
        public LuaFunction? CheckF { get; }
        public LuaFunction EffectF { get; }
        
        public Trigger(bool isSilent, string zone, string on, LuaFunction? checkF, LuaFunction effectF) {
            IsSilent = isSilent;
            Zone = zone;
            CheckF = checkF;
            EffectF = effectF;
            On = on;
        }

        public static Trigger FromLua(LuaTable table) {
            var isSilent = (bool)table["isSilent"];
            var effect = table["effect"] as LuaFunction;
            if (effect is null) throw new Exception("Failed to parse trigger: effect is nil");
            var zone = table["zone"] as string;
            if (zone is null) throw new Exception("Failed to parse trigger: zone is nil");
            var on = table["on"] as string;
            if (on is null) throw new Exception("Failed to parse trigger: on is nil");
            return new Trigger(isSilent, zone, on, table["check"] as LuaFunction, effect);
        }
    }
}