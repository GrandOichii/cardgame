

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
        public long ProcessDamage(long damage);
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
        public List<T> _cards;
        public List<T> Cards { get => _cards; }

        public T? this[string cardID]
        {
            get => _cards.Find(card => card.GetCardWrapper().ID == cardID);
        }

        public Zone(List<T> cards) {
            _cards = cards;
        }

        public void Shuffle() {
            _cards = Utility.Shuffled(Cards);
        }

        public List<T> PopTop(int amount) {
            if (amount > _cards.Count) amount = _cards.Count;
            var result = Cards.GetRange(0, amount);
            Cards.RemoveRange(0, amount);
            return result;
        }

        public void AddToBack(List<T> cards) {
            foreach (var card in cards)
                AddToBack(card);
        }

        public void AddToBack(T card) {
            _cards.Add(card);
        }
    }

}