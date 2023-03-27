using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLua;

using game.match;
using game.util;
using game.core;
using game.player;
using game.decks;


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
        _handler.GetStream().Write(data);
        Logger.Instance.Log("TCPPlayerController", "Sent message " + message);
    }

    private string Read() {
        var stream = _handler.GetStream();
        var result = "";
        int received;
        // while ((received = stream.Read(buffer)) != 0) {
        //     result += Encoding.UTF8.GetString(buffer, 0, received);
        // }
        received = stream.Read(buffer);
        result += Encoding.UTF8.GetString(buffer, 0, received);
        return result;
    }

    public override string PromptAction(Player controlledPlayer, Match match)
    {
        Write("Enter command for " + controlledPlayer.Name + "\n" + ShortInfo(controlledPlayer));
        return Read();
    }

    public override int PromptLane(string prompt, Player controlledPlayer, Match match)
    {
        Write(prompt);
        return int.Parse(Read());
    }
}

struct MatchData {

    static public void From(Player player, Match match) {

    }

    public string ToJson() {
        var result = JsonSerializer.Serialize<MatchData>(this);
        return result;
    }

}