

namespace game.match {

    class Match {

    }

    class MatchPool {

        public List<Match> Matches { get; private set; }

        public MatchPool() {
            Matches = new();
        }

        public Match NewMatch() {
            return new Match();
        }

    }

}