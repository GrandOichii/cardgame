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

                var canCastFunc = card.Table[Card.CAN_CAST_FNAME] as LuaFunction;
                if (canCastFunc is null) throw new Exception("CardWrapper with id " + cID + "(card name: " + card.Card.Name + ") doesn't have a " + Card.CAN_CAST_FNAME + " function");
                var canCast = (bool)canCastFunc.Call(pTable)[0];
                
                // TODO throw exception?
                if (!canCast) {
                    System.Console.WriteLine("WARN: player " + player.Name + " (" + player.ID + ") " + "tried to cast an uncastable spell " + card.Card.Name + " (" + card.ID + ")");
                    return;
                }

                var castFunc = card.Table[Card.ON_CAST_FNAME] as LuaFunction;
                if (castFunc is null) throw new Exception("CardWrapper with id " + cID + "(card name: " + card.Card.Name + ") doesn't have a " + Card.ON_CAST_FNAME + " function");

                // TODO figure out the input and output args
                player.Hand.Cards.Remove(card);
                castFunc.Call(card.ToLuaTable(match.LState), pTable);
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
                // TODO replace with card draw per turn
                player.DrawCards(match.Config.TurnStartCardDraw);
            }
        }

        class MainPhase : GamePhase
        {
            private readonly string PASS_TURN_ACTION = "pass";
            private static readonly Dictionary<string, actions.GameAction> ACTION_MAP =
            new(){
                { "cast", new actions.CastCardAction() }
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
}