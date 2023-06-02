using System.Text.Json;
using System.Text.Json.Serialization;
using game.cards;
using game.match;
using game.player;

namespace game.recording {

    public class PlayerRecord {
        [JsonPropertyName("deckList")]
        public string DeckList { get; set; }="";

        [JsonPropertyName("responses")]
        public List<string> Responses { get; set;}=new();
    }

    public class MatchRecord {
        [JsonPropertyName("players")]
        public List<PlayerRecord> Players { get; set; }

        [JsonPropertyName("config")]
        public MatchConfig Config { get; set; }

        [JsonPropertyName("timestamp")]
        public string TimeStamp { get; set; }

        public MatchRecord(MatchConfig config) {
            Config = config;
            Players = new();
            TimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
        }

        public static MatchRecord FromJson(string text) {
            var result = JsonSerializer.Deserialize<MatchRecord>(text);
            if (result is null) throw new Exception("Failed to deserialize MatchRecord from text");
            return result;
        }
    }

    public class PlaybackPlayerController : PlayerController
    {
        public override void InformMatchEnd(Player controlledPlayer, Match match, bool won)
        {
        }

        public override void Update(Player controlledPlayer, Match match)
        {
        }

        public PlayerRecord Record { get; }
        public PlaybackPlayerController(PlayerRecord record) {
            Record = record;
        }

        public string PopLast() {
            var result = Record.Responses[0];
            Record.Responses.RemoveAt(0);
            return result;
        }

        public int PopLastInt() {
            var last = PopLast();
            return int.Parse(last);
        }

        public override string ProcessPickAttackTarget(Player controlledPlayer, Match match, CardW card)
        {
            return PopLast();
        }

        public override string ProcessPrompt(string type, string prompt, List<string> args, Player controlledPlayer, Match match, string sourceID)
        {
            return PopLast();
        }

        public override string ProcessPromptAction(Player controlledPlayer, Match match)
        {
            return PopLast();
        }

        public override int ProcessPromptLane(string prompt, Player controlledPlayer, Match match, CardW? cursorCard = null)
        {
            return PopLastInt();
        }

    }

}