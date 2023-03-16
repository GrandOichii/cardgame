using NLua;
using System.Text.Json;
using System.Text.Json.Serialization;

using game.player;
using game.core.phases;
using game.collection;
using game.cards;
using game.deck;

namespace game.match {

    struct MatchConfig {
        [JsonPropertyName("starting_life")]
        public int StartingLifeTotal { get; set; }
        [JsonPropertyName("starting_hand_size")]
        public int StartingHandSize { get; set; }
        [JsonPropertyName("max_hand_size")]
        public int MaxHandSize { get; set; }
        static public MatchConfig FromText(string text) {
            var result = JsonSerializer.Deserialize<MatchConfig>(text);
            return result;
        }
    }
    
    class Match {
        static private List<GamePhase> _phases = new(){
            new TurnStart(),
            new MainPhase(),
            new TurnEnd()
        };
        
        // lua state
        private Lua _lState;
        public Lua LStata { get => _lState; }

        // players
        private List<Player> _players;
        private List<Player> Players {get => _players; }

        // game vars
        private int _curPlayerI;

        private MatchConfig _config;

        private Player? _winner;
        public bool Active { get => _winner is null; }

        public Match(MatchConfig config) {
            _config = config;
            
            _lState = new();
            _players = new();

            /*
            var type = typeof(TiledGame);
            foreach (var method in type.GetMethods())
            {
                if (method.GetCustomAttribute(typeof(LuaCommand)) is object)
                {
                    LState[method.Name] = method.CreateDelegate(Expression.GetDelegateType(
                        (from parameter in method.GetParameters() select parameter.ParameterType)
                        .Concat(new[] { method.ReturnType })
                        .ToArray())
                    , this);
                }
            }
            */
        }

        public bool AddPlayer(string playerName, PlayerController controller, Deck deck) {
            // TODO
            if (_players.Count >= 2) return false;

            var player = new Player(playerName, deck, controller);

            _players.Add(player);
            return true;
        }

        private void Setup() {
            foreach (var player in _players) {
                // set life total
                player.Life = _config.StartingLifeTotal;
                // fill hand
                player.Hand.AddToBack(player.Deck.PopTop(_config.StartingHandSize));
            }
        }

        private void Turns() {
            while(Active) {
                var cPlayer = _players[_curPlayerI];
                foreach (var phase in _phases) {
                    phase.Action(this, cPlayer);

                    if (!Active) return;
                }
                _curPlayerI++;
                if (_curPlayerI >= _players.Count)
                    _curPlayerI = 0;
            }
        }

        private void End() {
            // TODO
        }

        public void Start() {
            // setup game
            Setup();

            // turns
            Turns();

            // end game
            End();
        }
    }


    class MatchPool {
        public List<Match> Matches { get; }=new();

        public MatchPool() {

        }

        public Match CreateMatch(MatchConfig config) {
            return new Match(config);
        }
    }
}