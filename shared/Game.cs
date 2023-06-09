namespace Shared;

using System.Text.Json;
using System.Text.Json.Serialization;


public struct MatchState {
    [JsonPropertyName("request")]
    public string Request { get; set; }
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }
    [JsonPropertyName("args")]
    public List<string> Args { get; set; }
    [JsonPropertyName("sourceID")]
    public string SourceID { get; set; }
    [JsonPropertyName("lastPlayed")]
    public LastPlayedState? LastPlayed { get; set; }
    [JsonPropertyName("newLogs")]
    public List<string> NewLogs { get; set; }
    [JsonPropertyName("cursorCard")]
    public CardState? CursorCard { get; set; }


    // TODO add lane charms
    [JsonPropertyName("curPlayerI")]
    public int CurrentPlayerI { get; set; }
    [JsonPropertyName("players")]
    public PlayerState[] Players { get; set; }
    [JsonPropertyName("myData")]
    public MyState My { get; set; }


    public string ToJson() {
        var result = JsonSerializer.Serialize<MatchState>(this);
        return result;
    }

    static public MatchState From(string j) {
        var result = JsonSerializer.Deserialize<MatchState>(j);
        return result;
    }


}

public struct LastPlayedState {
    [JsonPropertyName("card")]
    public CardState Card { get; set; }
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; }

}

public struct MyState {
    [JsonPropertyName("playerI")]
    public int PlayerI { get; set; }
    [JsonPropertyName("hand")]
    public CardState[] Hand { get; set; }
}

public struct PlayerState {
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("handCount")]
    public int HandCount { get; set; }
    [JsonPropertyName("deckCount")]
    public int DeckCount { get; set; }
    [JsonPropertyName("life")]
    public long Life { get; set; }
    [JsonPropertyName("energy")]
    public int Energy { get; set; }
    [JsonPropertyName("maxEnergy")]
    public int MaxEnergy { get; set; }


    [JsonPropertyName("bond")]
    public CardState Bond { get; set; }
    [JsonPropertyName("discard")]
    public CardState[] Discard { get; set; }
    [JsonPropertyName("burned")]
    public CardState[] Burned { get; set; }
    [JsonPropertyName("treasures")]
    public CardState[] Treasures { get; set; }
    [JsonPropertyName("units")]
    public UnitState?[] Units { get; set; }

}

public struct MutableState {
    [JsonPropertyName("current")]
    public long Current { get; set; }
    [JsonPropertyName("min")]
    public long Min { get; set; }
    [JsonPropertyName("max")]
    public long Max { get; set; }
}

public struct CardState {

    [JsonPropertyName("id")]
    public string ID { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("life")]
    public long Life { get; set; }
    [JsonPropertyName("power")]
    public long Power { get; set; }
    [JsonPropertyName("cost")]
    public long Cost { get; set; }
    [JsonPropertyName("mutable")]
    public Dictionary<string, MutableState> Mutable { get; set; }
}

public struct UnitState {
    [JsonPropertyName("card")]
    public CardState Card { get; set; }
    [JsonPropertyName("attacksLeft")]
    public int AttackLeft { get; set; }
}