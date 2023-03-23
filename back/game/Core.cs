

using game.match;

namespace game.core {
    interface Damageable {
        public long ProcessDamage(long damage);
    }

    class Game {
        public MatchPool MatchPool { get; private set; }
        public CardMaster CardMaster { get; private set; }

        public Game(string collectionsPath) {
            MatchPool = new();
            CardMaster = new(collectionsPath);
        }
    }
}