using NLua;

using game.match;
using game.cards;
using game.util;
using game.cards.loaders;

namespace game.core {
    interface IDCreator {
        public string Next();
    }

    class BasicIDCreator : IDCreator
    {
        int _last = 0;
        public string Next()
        {
            _last++;
            return _last.ToString();
        }
    }

    interface IDamageable {
        public long ProcessDamage(Match match, long damage);
    }

    class Game {
        public MatchPool MatchPool { get; private set; }
        public CardMaster CardMaster { get; private set; }

        public Game(string collectionsPath) {
            MatchPool = new();
            // TODO replace with DB when implemented
            CardMaster = new(new FileCardLoader(collectionsPath));
        }
    }

    class Zone<T> where T : IHasCardW {

        public List<T> Cards { get; private set; }

        public T? this[string cardID]
        {
            get => Cards.Find(card => card.GetCardWrapper().ID == cardID);
        }

        public Zone(List<T> cards) {
            Cards = cards;
        }

        public void Shuffle() {
            Cards = Utility.Shuffled(Cards);
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
    
        public LuaTable ToLuaTable(Lua lState) {
            var result = Utility.CreateTable(lState);
            for (int i = 0; i < Cards.Count; i++)
                result[i+1] = Cards[i].GetCardWrapper().Info;
            return result;
        }
    }

    static class Zones {
        static public readonly string TREASURES = "treasures";
        static public readonly string BURNED = "burned";
        static public readonly string DISCARD = "discard";
        static public readonly string DECK = "deck";
        static public readonly string UNITS = "units";
        static public readonly string HAND = "hand";
        static public readonly string BOND = "bond";
    }

}