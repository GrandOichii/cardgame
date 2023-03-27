using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLua;

using game.match;
using game.cards;
using game.util;
using game.core;
using game.player;
using game.decks;

using Shared;

class Program {

    static private IPAddress ADDRESS = IPAddress.Parse("127.0.0.1");
    static private int PORT = 8080;

    static private TcpListener listener = new TcpListener(new IPEndPoint(ADDRESS, PORT));

    static void Main(string[] args)
    {
        #region Game Creation
        var g = new Game("../cards");

        #region Server Config
        listener.Start();
        #endregion

        #region Match Creation
        string configPath = "../match_configs/test_match.json";
        var m = g.MatchPool.NewMatch(MatchConfig.FromText(File.ReadAllText(configPath)));

        var deck1 = Deck.FromText(g.CardMaster, File.ReadAllText("../decks/test.deck"));
        var p1 = new Player(m, "Igor", deck1, new TCPPlayerController(listener));

        var deck2 = Deck.FromText(g.CardMaster, File.ReadAllText("../decks/test.deck"));
        var p2 = new Player(m, "Nastya", deck2, new TerminalPlayerController());
        g.CardMaster.LogContents();

        m.AddPlayer(p1);
        m.AddPlayer(p2);
        m.Start();
        
        deck1.UnloadCards(g.CardMaster);
        deck2.UnloadCards(g.CardMaster);
        
        #endregion

        bool clear = g.CardMaster.CheckEmpty();
        if (clear) return;

        g.CardMaster.LogContents();
        throw new Exception("Not all cards are unloaded from CardMaster");
        #endregion
    }
}

class TCPPlayerController : PlayerController
{
    byte[] buffer = new byte[1024];
    private TcpClient _handler;

    public TCPPlayerController(TcpListener listener) {
        Logger.Instance.Log("TCPPlayerController", "Waiting for connection");

        _handler = listener.AcceptTcpClient();
        
        Logger.Instance.Log("TCPPlayerController", "Connection established");
    }

    private void Write(string message) {
        var data = Encoding.UTF8.GetBytes(message);
        var handler = _handler.GetStream();
        NetUtil.Write(handler, message);
        // _handler.GetStream().Write(data);
        Logger.Instance.Log("TCPPlayerController", "Sent message " + message);
    }

    private string Read() {
        var stream = _handler.GetStream();
        // var result = "";
        // int received;
        // // while ((received = stream.Read(buffer)) != 0) {
        // //     result += Encoding.UTF8.GetString(buffer, 0, received);
        // // }
        // received = stream.Read(buffer);
        // result += Encoding.UTF8.GetString(buffer, 0, received);
        var result = NetUtil.Read(stream);
        return result;
    }

    public override string PromptAction(Player controlledPlayer, Match match)
    {
        Write("Enter command for " + controlledPlayer.Name + "\n" + ShortInfo(controlledPlayer) + "\n" + new MatchData(controlledPlayer, match).ToJson());
        return Read();
    }

    public override int PromptLane(string prompt, Player controlledPlayer, Match match)
    {
        Write(prompt);
        return int.Parse(Read());
    }
}

struct MatchData {

    // TODO add lane charms
    [JsonPropertyName("curPlayerI")]
    public int CurrentPlayerI { get; set; }
    [JsonPropertyName("players")]
    public PlayerData[] Players { get; set; }
    [JsonPropertyName("myData")]
    public MyData My { get; set; }


    public MatchData(Player player, Match match) {

        Players = new PlayerData[match.Players.Count];
        for (int i = 0; i < match.Players.Count; i++) {
            var pData = PlayerData.From(match.Players[i]);
            Players[i] = pData;
        }

        CurrentPlayerI = match.CurPlayerI;

        My = MyData.From(player);
    }

    public string ToJson() {
        var result = JsonSerializer.Serialize<MatchData>(this);
        return result;
    }

    static public MatchData From(string j) {
        var result = JsonSerializer.Deserialize<MatchData>(j);
        return result;
    }

}

struct MyData {
    [JsonPropertyName("hand")]
    public CardData[] Hand { get; set; }

    static public MyData From(Player player) {
        var result = new MyData();
        
        result.Hand = new CardData[player.Hand.Cards.Count];
        for (int i = 0; i < player.Hand.Cards.Count; i++) {
            var cData = CardData.From(player.Hand.Cards[i]);
            result.Hand[i] = cData;
        }
        
        return result;
    }
}

struct PlayerData {
    [JsonPropertyName("handCount")]
    public int HandCount { get; set; }
    [JsonPropertyName("deckCount")]
    public int DeckCount { get; set; }
    [JsonPropertyName("life")]
    public long Life { get; set; }
    [JsonPropertyName("energy")]
    public int Energy { get; set; }


    [JsonPropertyName("discard")]
    public CardData[] Discard { get; set; }
    [JsonPropertyName("burned")]
    public CardData[] Burned { get; set; }
    [JsonPropertyName("treasures")]
    public CardData[] Treasures { get; set; }
    [JsonPropertyName("units")]
    public UnitData?[] Units { get; set; }

    static public PlayerData From(Player player) {
        var result = new PlayerData();

        result.HandCount = player.Hand.Cards.Count;
        result.DeckCount = player.Deck.Cards.Count;
        result.Life = player.Life;
        result.Energy = player.Energy;
        
        result.Discard = new CardData[player.Discard.Cards.Count];
        for (int i = 0; i < player.Discard.Cards.Count; i++) {
            var cData = CardData.From(player.Discard.Cards[i]);
            result.Discard[i] = cData;
        }
        
        result.Burned = new CardData[player.Burned.Cards.Count];
        for (int i = 0; i < player.Burned.Cards.Count; i++) {
            var cData = CardData.From(player.Burned.Cards[i]);
            result.Burned[i] = cData;
        }

        result.Treasures = new CardData[player.Treasures.Cards.Count];
        for (int i = 0; i < player.Treasures.Cards.Count; i++) {
            var cData = CardData.From(player.Treasures.Cards[i].Card);
            result.Treasures[i] = cData;
        }

        result.Units = new UnitData?[player.Lanes.Length];
        for (int i = 0; i < player.Lanes.Length; i++) {
            var u = player.Lanes[i];
            if (u is null) continue;

            var uData = UnitData.From(u);
            result.Units[i] = uData;
        }

        return result;
    }
}

struct CardData {
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

    static public CardData From(CardW card) {
        var result = new CardData();

        result.Name = card.Original.Name;
        result.Type = card.Original.Type;
        result.Text = card.Original.Text;
        if (result.Type == "Unit")
            result.Power = Utility.GetLong(card.Info, "power");
        if (result.Type == "Unit" || result.Type == "Treasure")
            result.Life = Utility.GetLong(card.Info, "life");
        return result;
    }
}

struct UnitData {
    [JsonPropertyName("card")]
    public CardData Card { get; set; }
    [JsonPropertyName("attacksLeft")]
    public int AttackLeft { get; set; }

    static public UnitData From(UnitW card) {
        var result = new UnitData();

        result.AttackLeft = card.AvailableAttacks;
        result.Card = CardData.From(card.Card);

        return result;
    }
}