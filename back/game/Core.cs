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

    namespace phases {

        abstract class GamePhase {
            abstract public void Action(Match match, Player player);
        }

        class TurnStart : GamePhase
        {
            public override void Action(Match match, Player player)
            {
                // TODO
                throw new NotImplementedException();
            }
        }

        
        class MainPhase : GamePhase
        {
            public override void Action(Match match, Player player)
            {
                // TODO
                throw new NotImplementedException();
            }
        }

        class TurnEnd : GamePhase
        {
            public override void Action(Match match, Player player)
            {
                // TODO
                throw new NotImplementedException();
            }
        }
    }

    class CardDeck {
        public List<CardWrapper> _cards;
        public List<CardWrapper> Cards { get => _cards; }

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

        static public CardDeck From(Deck deck) {
            var cards = new List<CardWrapper>();
            foreach (var pair in deck.Cards)
                for (int i = 0; i < pair.Value; i++)
                    cards.Add(new CardWrapper(pair.Key));
            return new CardDeck(cards);
        }

        public void AddToBack(List<CardWrapper> cards) {
            foreach (var card in cards)
                _cards.Add(card);
        }
    }
}