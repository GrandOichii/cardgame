

using game.match;
using game.cards;
using game.cards.loaders;

namespace game.core {
    interface Damageable {
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
}