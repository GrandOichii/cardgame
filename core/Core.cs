using NLua;

using game.match;
using game.cards;
using game.util;
using game.cards.loaders;

namespace game.core {
    public interface IDCreator {
        public string Next();
    }

    public class BasicIDCreator : IDCreator
    {
        int _last = 0;
        public string Next()
        {
            _last++;
            return _last.ToString();
        }
    }

    public interface IDamageable {
        public long ProcessDamage(Match match, long damage);
        
    }

    public class Game {
        public MatchPool MatchPool { get; private set; }
        public CardMaster CardMaster { get; private set; }
        public CardLoader CardLoader { get; set; }

        public Game(string collectionsPath) {
            MatchPool = new();
            // TODO replace with DB when implemented
            CardLoader = new FileCardLoader(collectionsPath);
            CardMaster = new(CardLoader);
        }
    }

    public class Zone<T> where T : IHasCardW {

        public List<T> Cards { get; private set; }

        public T? this[string cardID]
        {
            get => Cards.Find(card => card.GetCardWrapper().ID == cardID);
        }

        public Zone(List<T> cards) {
            Cards = cards;
        }

        public void Shuffle(Random rnd) {
            Cards = Utility.Shuffled(Cards, rnd);
        }

        public List<T> PopTop(int amount) {
            if (amount > Cards.Count) amount = Cards.Count;
            var result = Cards.GetRange(0, amount);
            Cards.RemoveRange(0, amount);
            return result;
        }

        public void AddToBack(List<T> cards) {
            foreach (var card in cards)
                AddToBack(card);
        }

        public void AddToBack(T card) {
            Cards.Add(card);
        }

        public void AddToFront(T card) {
            Cards.Insert(0, card);
        }
    
        public LuaTable ToLuaTable(Lua lState) {
            var result = Utility.CreateTable(lState);
            for (int i = 0; i < Cards.Count; i++)
                result[i+1] = Cards[i].GetCardWrapper().Info;
            return result;
        }
    }

    static public class Zones {
        static public readonly string TREASURES = "treasures";
        static public readonly string BURNED = "burned";
        static public readonly string DISCARD = "discard";
        static public readonly string DECK = "deck";
        static public readonly string UNITS = "units";
        static public readonly string HAND = "hand";
        static public readonly string BOND = "bond";
    }

}