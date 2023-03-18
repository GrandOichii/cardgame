using NLua;
using System.Text.Json;
using System.Text.Json.Serialization;

using game.player;
using game.core.phases;
using game.collection;
using game.cards;
using game.deck;
using game.scripts;

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

    class CardManager
    {
        public Dictionary<string, CardWrapper> Cards { get; } = new();

        public CardWrapper? this[string cID]
        {
            get => Cards.ContainsKey(cID) ? Cards[cID] : null;
        }


        public void Add(List<CardWrapper> cards)
        {
            foreach (var card in cards)
                Cards[card.ID] = card;
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
        public Lua LState { get => _lState; }
        private ScriptMaster _scriptMaster;
        public CardManager AllCards { get; }

        // players
        private List<Player> _players;
        private List<Player> Players { get => _players; }

        // game vars
        private int _curPlayerI;

        public MatchConfig Config { get; }

        public Player? Winner { get; set; }
        public bool Active { get => Winner is null; }

        public Match(MatchConfig config) {
            Config = config;
            
            _lState = new();
            _scriptMaster = new(this);
            AllCards = new();
            LoadLuaScripts();
            _players = new();
        }

        private void LoadLuaScripts()
        {
            string[] scriptPaths = { "common.lua" };
            // string[] scriptPaths = { "C:\\Users\\ihawk\\code\\cardgame\\back\\common.lua" };

            foreach (var path in scriptPaths)
                LState.DoFile(path);
        }

        public bool AddPlayer(string playerName, PlayerController controller, Deck deck) {
        // TODO
            if (_players.Count >= 2) return false;

            var player = new Player(this, playerName, deck, controller);

            _players.Add(player);
            return true;
        }

        public Player? PlayerByID(int pid) => _players.Find(player => player.ID == pid);

        private void Setup() {
            foreach (var player in _players) {
                // add cards in deck to card pool
                AllCards.Add(player.Deck.Cards);

                // set life total
                player.Life = Config.StartingLifeTotal;

                // fill hand
                player.Hand.AddToBack(player.Deck.PopTop(Config.StartingHandSize));
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