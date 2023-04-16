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
    
        public string ToJson() => JsonSerializer.Serialize<MatchConfig>(this);
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
        public int CurPlayerI { get; private set; }

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
                // player.Hand.AddToBack(player.Deck.PopTop(Config.StartingHandSize));

                // TODO remove, for testing without redraws only
                while (true) {
                    player.Hand.AddToBack(player.Deck.PopTop(Config.StartingHandSize));
                    var count = 0;
                    foreach (var card in player.Hand.Cards)
                        if (card.Original.Name == "Source")
                            count++;
                    if (count >= 2) break;
                    player.Deck.AddToBack(player.Hand.PopTop(Config.StartingHandSize));
                    player.Deck.Shuffle();
                }
            }
            Logger.Instance.Log("Match", "Setup concluded");
        }

        private void Turns() {
            Logger.Instance.Log("Match", "Started turns loop");
            while(Active) {
                var cPlayer = Players[CurPlayerI];
                foreach (var phase in _phases) {
                    phase.Exec(this, cPlayer);

                    if (!Active) break;
                }
                CurPlayerI++;
                if (CurPlayerI >= Players.Count)
                    CurPlayerI = 0;
            }

            // TODO should always be true
            if (Winner is not null) Logger.Instance.Log("Match", "Winner is " + Winner.ShortStr());
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
                            continue;
                        }

                        var payed = trigger.ExecCosts(LState, player, args);
                        if (!payed) {
                            Logger.Instance.Log("Match", "Player " + player.ShortStr() + " did not pay cost of triggered ability of card " + card.ShortStr() + " in zone " + zone);
                            continue;
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
    
        public List<HasMarkedDamage> GetDamageableCards() {
            var result = new List<HasMarkedDamage>();
            foreach (var player in Players) {
                foreach (var unit in player.Lanes) {
                    if (unit is null) continue;
                    result.Add(unit);
                }
                foreach (var treasure in player.Treasures.Cards)
                    result.Add(treasure);
            }
            return result;
        }

        public Player OwnerOf(string cID) {
            foreach (var player in Players) {
                var cards = player.GetAllCards();
                var result = cards.Keys.ToList().Find(card => card.ID == cID);
                if (result is null) continue;
                return player;
            }
            throw new Exception("Failed to find owner of card with ID:" + cID);
        }
    
        public void UpdateOpponent() {
            var op = OpponentOf(Players[CurPlayerI]);
            op.Controller.Update(op, this);
        }

        public Dictionary<Player, List<HasMarkedDamage>> HasMarkedDamageDict() {
            var result = new Dictionary<Player, List<HasMarkedDamage>>();
            foreach (var player in Players)
                result.Add(player, player.HasMarkedDamageCards());
            return result;
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