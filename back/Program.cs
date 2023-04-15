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

    static private IPAddress ADDRESS = IPAddress.Any;
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
        // string configPath = "../match_configs/test_match.json";
        string configPath = "../match_configs/normal.json";
        var config = MatchConfig.FromText(File.ReadAllText(configPath));
        var m = g.MatchPool.NewMatch(config);

        var deck1 = Deck.FromText(g.CardMaster, File.ReadAllText("../decks/test.deck"));
        var p1 = new Player(m, "Igor", deck1, new TCPPlayerController(listener, config));

        var deck2 = Deck.FromText(g.CardMaster, File.ReadAllText("../decks/test.deck"));
        // var p2 = new Player(m, "Nastya", deck2, new TerminalPlayerController());
        var p2 = new Player(m, "Nastya", deck2, new TCPPlayerController(listener, config));
        
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

    public TCPPlayerController(TcpListener listener, MatchConfig config) {
        Logger.Instance.Log("TCPPlayerController", "Waiting for connection");

        _handler = listener.AcceptTcpClient();
        
        Logger.Instance.Log("TCPPlayerController", "Connection established, sending match config");
        Write(config.ToJson());
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
        var result = NetUtil.Read(stream);
        System.Console.WriteLine("Read: " + result);
        // string? result = null;
        //     while (result is null)
        //         result = Console.ReadLine();
        //     System.Console.WriteLine();
        return result;
    }

    public override string PromptAction(Player controlledPlayer, Match match)
    {
        // Write("Enter command for " + controlledPlayer.Name + "\n" + ShortInfo(controlledPlayer));
        
        // TODO? args are available commands
        Write(CreateMState(controlledPlayer, match, "enter command", new()).ToJson());
        return Read();
    }

    public override int PromptLane(string prompt, Player controlledPlayer, Match match)
    {
        // Write(prompt);

        // TODO? args are available lanes
        Write(CreateMState(controlledPlayer, match, "pick lane", new()).ToJson());
        return int.Parse(Read());
    }

    public override string Prompt(string prompt, List<string> args, Player controlledPlayer, Match match) {
        Write(CreateMState(controlledPlayer, match, prompt, args).ToJson());
        return Read();        
    }


    #region Parsers

    static public MatchState CreateMState(Player player, Match match, string request, List<string> args) {
        var result = new MatchState();
        result.Players = new PlayerState[match.Players.Count];
        for (int i = 0; i < match.Players.Count; i++) {
            var pData = PlayerStateFrom(match.Players[i]);
            result.Players[i] = pData;
        }

        result.CurrentPlayerI = match.CurPlayerI;

        result.My = MyStateFrom(match, player);

        result.Request = request;

        result.Args = args;
        return result;
    }

    static public MyState MyStateFrom(Match match, Player player) {
        var result = new MyState();
        
        result.Hand = new CardState[player.Hand.Cards.Count];
        for (int i = 0; i < player.Hand.Cards.Count; i++) {
            var cData = CardStateFrom(player.Hand.Cards[i]);
            result.Hand[i] = cData;
        }
        
        result.PlayerI = match.Players.IndexOf(player);
        return result;
    }

    static public PlayerState PlayerStateFrom(Player player) {
        var result = new PlayerState();

        result.Name = player.Name;
        result.HandCount = player.Hand.Cards.Count;
        result.DeckCount = player.Deck.Cards.Count;
        result.Life = player.Life;
        result.Energy = player.Energy;

        result.Bond = CardStateFrom(player.Bond);
        
        result.Discard = new CardState[player.Discard.Cards.Count];
        for (int i = 0; i < player.Discard.Cards.Count; i++) {
            var cData = CardStateFrom(player.Discard.Cards[i]);
            result.Discard[i] = cData;
        }
        
        result.Burned = new CardState[player.Burned.Cards.Count];
        for (int i = 0; i < player.Burned.Cards.Count; i++) {
            var cData = CardStateFrom(player.Burned.Cards[i]);
            result.Burned[i] = cData;
        }

        result.Treasures = new CardState[player.Treasures.Cards.Count];
        for (int i = 0; i < player.Treasures.Cards.Count; i++) {
            var cData = CardStateFrom(player.Treasures.Cards[i].Card);
            result.Treasures[i] = cData;
        }

        result.Units = new UnitState?[player.Lanes.Length];
        for (int i = 0; i < player.Lanes.Length; i++) {
            var u = player.Lanes[i];
            if (u is null) continue;

            var uData = UnitStateFrom(u);
            result.Units[i] = uData;
        }

        return result;
    }

    static public CardState CardStateFrom(CardW card) {
        var result = new CardState();

        result.ID = card.ID;
        result.Name = card.Original.Name;
        result.Type = card.Original.Type;
        result.Text = card.Original.Text;
        result.Cost = card.GetCost();
        if (result.Type == "Unit")
            result.Power = Utility.GetLong(card.Info, "power");
        if (result.Type == "Unit" || result.Type == "Treasure")
            result.Life = Utility.GetLong(card.Info, "life");
        result.Mutable = new();
        var mutable = Utility.TableGet<LuaTable>(card.Info, "mutable");
        foreach (var key in mutable.Keys) {
            var sKey = key as string;
            if (sKey is null) throw new Exception("Key " + key + " is not string");
            result.Mutable.Add(sKey, MutableStateFrom(mutable[key]));
        }
        return result;
    }

    static public UnitState UnitStateFrom(UnitW card) {
        var result = new UnitState();

        result.AttackLeft = card.AvailableAttacks;
        result.Card = CardStateFrom(card.Card);

        return result;
    }

    static public MutableState MutableStateFrom(object o)
    {
        var table = o as LuaTable;
        if (table is null) throw new Exception("Object " + o + " is not a table");

        var result = new MutableState();
        result.Current = Utility.GetLong(table, "current");
        result.Min = Utility.GetLong(table, "min");
        result.Max= Utility.GetLong(table, "max");
        return result;
    }




    #endregion

    public override void Update(Player controlledPlayer, Match match)
    {
        Write(CreateMState(controlledPlayer, match, "update", new()).ToJson());
    }
}
