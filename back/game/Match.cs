using NLua;
using System.Text.Json;
using System.Text.Json.Serialization;

using game.util;
using game.player;
using game.cards;
using game.scripts;
using game.core.phases;
using game.core.effects;

namespace game.match {
    class CardManager
    {
        public Dictionary<string, CardW> Cards { get; } = new();

        public CardW? this[string cID]
        {
            get => Cards.ContainsKey(cID) ? Cards[cID] : null;
        }


        public void Add(List<CardW> cards)
        {
            foreach (var card in cards)
                Cards[card.ID] = card;
        }
    }

    struct MatchConfig {
        [JsonPropertyName("base_source_per_turn")]  public int BaseSourcePerTurn { get; set; }
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
        static private List<GamePhase> _phases = new(){
            new TurnStart(),
            new MainPhase(),
            new TurnEnd()
        };

        public MatchConfig Config { get; private set; }
        public Lua LState { get; private set; }
        public Player? Winner { get; set; }
        public bool Active { get => Winner is null; }
        public List<Player> Players { get; private set; }
        public Player? PlayerByID(int pid) => Players.Find(player => player.ID == pid);
        private int _curPlayerI;

        public CardManager AllCards { get; private set; }

        public Match(MatchConfig config) {
            Config = config;

            // load scripts
            LState = new();
            new ScriptMaster(this);
            LState.DoFile("core.lua");

            AllCards = new();

            Players = new();
        }
        
        public bool AddPlayer(Player player) {
            if (Players.Count >= 2) return false;
            Logger.Instance.Log("Match", "Added player " + player.Name);
            Players.Add(player);
            return true;
        }

        public void Start() {
            if (Players.Count != 2) throw new Exception("Tried to launch match with " + Players.Count + " players");
            Logger.Instance.Log("Match", "Match started!");

            Setup();
            Turns();
            End();
        }

        private void Setup() {
            Logger.Instance.Log("Match", "Performing setup...");
            foreach (var player in Players) {
                // set starting energy
                player.Energy = Config.StartingEnergy;
                player.MaxEnergy = Config.StartingEnergy;
                
                // add cards in deck to card pool
                AllCards.Add(player.Deck.Cards);

                // set life total
                player.Life = Config.StartingLifeTotal;

                // fill hand
                player.Hand.AddToBack(player.Deck.PopTop(Config.StartingHandSize));
            }
            Logger.Instance.Log("Match", "Setup concluded");
        }

        private void Turns() {
            Logger.Instance.Log("Match", "Started turns loop");
            while(Active) {
                var cPlayer = Players[_curPlayerI];
                foreach (var phase in _phases) {
                    phase.Exec(this, cPlayer);

                    if (!Active) return;
                }
                _curPlayerI++;
                if (_curPlayerI >= Players.Count)
                    _curPlayerI = 0;
            }
           Logger.Instance.Log("Match", "Finished turns loop");
        }

        private void End() {
            Logger.Instance.Log("Match", "Performing clean up");
            // TODO
            Logger.Instance.Log("Match", "Clean up concluded");
        }

        public void Emit(string signal, Dictionary<string, object> args) {
            var logMessage = "Emitted signal " + signal + ", args: ";
            foreach (var pair in args) logMessage += pair.Key + ":" + pair.Value.ToString() + " ";
            Logger.Instance.Log("Match", logMessage);

            foreach (var player in Players) {
                var cards = player.GetAllCards();
                foreach (var pair in cards) {
                    var zone = pair.Value;
                    var card = pair.Key;
                    var triggers = Utility.TableGet<LuaTable>(card.Info, "triggers");
                    foreach (var triggerO in triggers.Values) {
                        var triggerRaw = triggerO as LuaTable;
                        if (triggerRaw is null) throw new Exception("Trigger of card " + card.ShortStr() + " is somehow value " + triggerO + " (not LuaTable)");
                        var z = Utility.TableGet<string>(triggerRaw, "zone");
                        if (z != zone) continue;
                        var on = Utility.TableGet<string>(triggerRaw, "on");
                        if (on != signal) continue;

                        var trigger = new Trigger(triggerRaw);
                        // TODO something else
                        Logger.Instance.Log("Match", "Card " + card.ShortStr() + " in zone " + zone + " of player " + player.ShortStr() + " has a potential trigger");

                        var triggered = trigger.ExecCheck(LState, player, args);
                        if (!triggered) {
                            Logger.Instance.Log("Match", "Card " + card.ShortStr() + " in zone " + zone + " of player " + player.ShortStr() + " failed to trigger");
                            return;
                        }

                        var payed = trigger.ExecCosts(LState, player, args);
                        if (!payed) {
                            Logger.Instance.Log("Match", "Player " + player.ShortStr() + " did not pay cost of triggered ability of card " + card.ShortStr() + " in zone " + zone);
                            return;
                        }

                        Logger.Instance.Log("Match", "Card " + card.ShortStr() + " in zone " + zone + " of player " + player.ShortStr() + " triggers");
                        trigger.ExecEffect(LState, player, args);
                    }
                }
            }

            Logger.Instance.Log("Match", "Finished emitting " + signal);
        }

        public Player OpponentOf(Player player) {
            if (Players[0] == player) return Players[1];
            return Players[0];
        }
    }

    class MatchPool {

        public List<Match> Matches { get; private set; }

        public MatchPool() {
            Matches = new();
        }

        public Match NewMatch(MatchConfig config) {
            Logger.Instance.Log("MatchPool", "Requested to create new match");
            return new Match(config);
        }

    }

}