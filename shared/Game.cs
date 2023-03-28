namespace Shared;

using System.Text.Json;
using System.Text.Json.Serialization;

public struct MatchState {

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

public struct MyState {
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

public struct CardState {
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

}

public struct UnitState {
    [JsonPropertyName("card")]
    public CardState Card { get; set; }
    [JsonPropertyName("attacksLeft")]
    public int AttackLeft { get; set; }
}