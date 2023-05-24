using System.Text.Json.Serialization;
using game.match;

namespace game.recording {

    class PlayerRecord {
        [JsonPropertyName("deckList")]
        public string DeckList { get; set; }="";

        [JsonPropertyName("responses")]
        public List<string> Responses { get; set;}=new();
    }

    class MatchRecord {
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
    }

}