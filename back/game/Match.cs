using NLua;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace game.match {
    struct MatchConfig {
        [JsonPropertyName("lane_count")]            public int LaneCount { get; set; }
        [JsonPropertyName("starting_energy")]       public int StartingEnergy { get; set; }
        [JsonPropertyName("starting_life")]         public int StartingLifeTotal { get; set; }
        [JsonPropertyName("turn_start_card_draw")]  public int TurnStartCardDraw { get; set; }
        [JsonPropertyName("starting_hand_size")]    public int StartingHandSize { get; set; }
        [JsonPropertyName("max_hand_size")]         public int MaxHandSize { get; set; }

        static public MatchConfig FromText(string text) {
            var result = JsonSerializer.Deserialize<MatchConfig>(text);
            return result;
        }

        public LuaTable ToLuaTable(Lua lState)
        {
            lState.NewTable("result");
            var result = lState.GetTable("result");
            result["lane_count"] = LaneCount;
            result["starting_life"] = StartingLifeTotal;
            result["turn_start_card_draw"] = TurnStartCardDraw;
            result["starting_hand_size"] = StartingHandSize;
            result["max_hand_size"] = MaxHandSize;
            result["starting_energy"] = StartingEnergy;
            return result;
        }
    }

    class Match {
        public MatchConfig Config { get; private set; }
        public Lua LState { get; private set; }
        public Match(MatchConfig config) {
            Config = config;

            // load scripts
            LState = new();

        }
        
        public void Start() {
            // TODO
        }
    }

    class MatchPool {

        public List<Match> Matches { get; private set; }

        public MatchPool() {
            Matches = new();
        }

        public Match NewMatch(MatchConfig config) {
            return new Match(config);
        }

    }

}