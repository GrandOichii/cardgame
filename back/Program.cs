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


struct BotMessage {
    [JsonPropertyName("header")]
    public string Header { get; set; }
    [JsonPropertyName("payload")]
    public string Payload { get; set; }

    public BotMessage(string header, string payload) {
        Header = header;
        Payload = payload;
    }

    public string ToJson() => JsonSerializer.Serialize(this);

    static public BotMessage From(string text) => JsonSerializer.Deserialize<BotMessage>(text);
}

class Program {

    static private string DECK_PATH = "../decks/generated.deck";

    static private IPAddress ADDRESS = IPAddress.Any;
    static private int PORT = 8080;

    static private TcpListener listener = new TcpListener(new IPEndPoint(ADDRESS, PORT));

    static async void RunMatch(Match match) {
        match.Start();
    }

    static void RunMatchPool(string[] args) {
        var g = new Game("../cards");
        string configPath = "../match_configs/normal.json";
        var config = MatchConfig.FromText(File.ReadAllText(configPath));

        // TODO remove, fills matches with random matches
        Random random = new Random();
        for (int i = 0; i < 10; i++) {
            var m = g.MatchPool.NewMatch(g, config);

            var values = Enum.GetValues(typeof(EMatchState));
            m.State = (EMatchState)values.GetValue(random.Next(values.Length));
        }

        Dictionary<string, Action<NetworkStream, string[]>> commands = new(){
            {"list", (stream, args) => {
                var result = new Dictionary<string, string>();
                foreach (var pair in g.MatchPool.Matches) {
                    var mID = pair.Key;
                    string status = pair.Value.State.ToString();
                    result.Add(mID, status);
                }
                NetUtil.Write(stream, JsonSerializer.Serialize(result));
            }},
            {"new", (stream, args) => {
                // TODO required argument for whether the game has a bot or not
                var m = g.MatchPool.NewMatch(g, config);
                var mID = g.MatchPool.IDOf(m);

                // TODO remove
                var deck = Deck.FromText(g.CardMaster, File.ReadAllText(DECK_PATH));
                var p = new Player(m, "Nastya", deck, new LuaBotController("../bots/test_bot.lua"));
                m.AddPlayer(p);

                NetUtil.Write(stream, new BotMessage("success", mID).ToJson());
            }},
            {"connect", (stream, args) => {
                var mID = args[1];
                var username = args[2];
                if (!g.MatchPool.Matches.ContainsKey(mID)) {
                    NetUtil.Write(stream, new BotMessage("fail", "No match with id: " + mID).ToJson());
                    return;
                }
                var match = g.MatchPool.Matches[mID];
                if (match.State == EMatchState.Ended) {
                    NetUtil.Write(stream, new BotMessage("fail", "Failed to connect to match with id " + mID + ": the match already ended").ToJson());
                    return;
                }
                if (match.State == EMatchState.InProgress) {
                    NetUtil.Write(stream, new BotMessage("fail", "Failed to connect to match with id " + mID + ": the match is in progress").ToJson());
                    return;
                }

                NetUtil.Write(stream, new BotMessage("success", "Waiting for connection").ToJson());

                // TODO remove, adds bot
                var deck = Deck.FromText(g.CardMaster, File.ReadAllText(DECK_PATH));
                var p = new Player(match, "Igor", deck, new TCPPlayerController(listener, config));
                match.AddPlayer(p);

                if (match.Players.Count != 2) {
                    NetUtil.Write(stream, new BotMessage("match_pending", "").ToJson());
                    return;
                }

                NetUtil.Write(stream, new BotMessage("match_started", "").ToJson());
                RunMatch(match);
            }}
        };


        listener.Start();
        System.Console.WriteLine("Started");

        // connect to tg bot
        var tgBotConnection = listener.AcceptTcpClient();
        var tgBotStream = tgBotConnection.GetStream();
        while (true) {
            var messageRaw = NetUtil.Read(tgBotStream);
            var message = BotMessage.From(messageRaw);

            if (message.Header == "command") {
                var command = message.Payload;
                var cArgs = command.Split(' ');
                var commandF = commands[cArgs[0]];
                commandF.Invoke(tgBotStream, cArgs);
                continue;
            }
        }
    }

    static void Main(string[] args)
    {
        // RunMatchPool(args);
        // return;
        
        // listener.Start();
        // System.Console.WriteLine("Started server");
        // var handler = listener.AcceptTcpClient();
        // var stream = handler.GetStream();
        // NetUtil.Write(stream, "Hello");
        // NetUtil.Write(stream, ", world");
        // // byte[] buffer = Encoding.UTF8.GetBytes("Hello, ");
        // // stream.Write(buffer, 0, buffer.Length);
        // // buffer = Encoding.UTF8.GetBytes("world.");
        // // stream.Write(buffer, 0, buffer.Length);
        // handler.Close();
        // // handler

        // return;
        #region Game Creation
        var g = new Game("../cards");

        #region Server Config
        listener.Start();
        #endregion

        #region Match Creation
        // string configPath = "../match_configs/test_match.json";
        string configPath = "../match_configs/normal.json";
        var config = MatchConfig.FromText(File.ReadAllText(configPath));
        var m = g.MatchPool.NewMatch(g, config);

        var deck1 = Deck.FromText(g.CardMaster, File.ReadAllText(DECK_PATH));
        var p1 = new Player(m, "Igor", deck1, new TCPPlayerController(listener, config));
        // var p1 = new Player(m, "Igor", deck1, new LuaBotController("../bots/test_bot.lua"));

        var deck2 = Deck.FromText(g.CardMaster, File.ReadAllText(DECK_PATH));
        // var p2 = new Player(m, "Nastya", deck2, new TerminalPlayerController());
        // var p2 = new Player(m, "Nastya", deck2, new TCPPlayerController(listener, config));
        var p2 = new Player(m, "Nastya", deck2, new LuaBotController("../bots/test_bot.lua"));
        
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


static class MatchParsers {
    static public MatchState CreateMState(Player player, Match match, string request, List<string> args, string prompt="", string sourceID="") {
        var result = new MatchState();
        result.Players = new PlayerState[match.Players.Count];
        for (int i = 0; i < match.Players.Count; i++) {
            var pData = PlayerStateFrom(match.Players[i]);
            result.Players[i] = pData;
        }

        result.CurrentPlayerI = match.CurPlayerI;

        result.My = MyStateFrom(match, player);

        result.Request = request;

        result.Prompt = prompt;

        result.Args = args;

        result.SourceID = sourceID;

        result.LastPlayed = null;
        if (match.LastPlayed is object) {
            var s = new LastPlayedState();
            s.Card = CardStateFrom(match.LastPlayed);
            s.PlayerName = match.LastPlayedPName;
            result.LastPlayed = s;
        }
        
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
        result.MaxEnergy = player.MaxEnergy;

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
        result.Text = card.Original.Text + card.Info["appendText"];
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
        // Logger.Instance.Log("TCPPlayerController", "Sent message " + message);
    }

    private string Read() {
        var stream = _handler.GetStream();
        var result = NetUtil.Read(stream);
        // System.Console.WriteLine("Read: " + result);
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
        Write(MatchParsers.CreateMState(controlledPlayer, match, "enter command", new()).ToJson());
        return Read();
    }

    public override int PromptLane(string prompt, Player controlledPlayer, Match match)
    {
        // Write(prompt);

        // TODO? args are available lanes
        Write(MatchParsers.CreateMState(controlledPlayer, match, "pick lane", new()).ToJson());
        return int.Parse(Read());
    }

    public override string Prompt(string type, string prompt, List<string> args, Player controlledPlayer, Match match, string sourceID) {
        Write(MatchParsers.CreateMState(controlledPlayer, match, type, args, prompt, sourceID).ToJson());
        return Read();        
    }

    public override void Update(Player controlledPlayer, Match match)
    {
        Write(MatchParsers.CreateMState(controlledPlayer, match, "update", new()).ToJson());
    }

    public override void InformMatchEnd(Player controlledPlayer, Match match, bool won) {
        Write(MatchParsers.CreateMState(controlledPlayer, match, (won ? "won" : "lost"), new()).ToJson());
    }

    public override string PickAttackTarget(Player controlledPlayer, Match match, CardW card) {
        // TODO replace new() with available attacks
        var opponent = match.OpponentOf(controlledPlayer);
        var targets = new List<string>();
        foreach (var treasure in opponent.Treasures.Cards)
            targets.Add(treasure.GetCardWrapper().ID);
        Write(MatchParsers.CreateMState(controlledPlayer, match, "pick attack target", targets, "", card.ID).ToJson());
        return Read();
    }


}


// static class StateExtensions {
//     public static LuaTable ToLuaTable(this MatchState state, Lua lState) {
//         var result = Utility.CreateTable(lState);
//         foreach (var card )
//         return result;
//     }

//     public static LuaTable ToLuaTable(this CardState state, Lua lState) {
//         var result = Utility.CreateTable(lState);

//         return result;

//     }
// }


class LuaBotController : PlayerController
{
    public Lua LState { get; }

    public LuaFunction PromptF { get; }
    public LuaFunction PromptActionF { get; }
    public LuaFunction PromptLaneF { get; }
    public LuaFunction UpdateF { get; }


    public LuaBotController(string scriptPath) {
        LState = new();

        LState.DoFile(scriptPath);

        PromptF = Utility.GetGlobalF(LState, "_Prompt");
        PromptActionF = Utility.GetGlobalF(LState, "_PromptAction");
        PromptLaneF = Utility.GetGlobalF(LState, "_PromptLane");
        UpdateF = Utility.GetGlobalF(LState, "_Update");

    }


    public override string Prompt(string type, string prompt, List<string> args, Player controlledPlayer, Match match, string sourceID)
    {
        // TODO change to created luatable
        var result = PromptF.Call(MatchParsers.CreateMState(controlledPlayer, match, type, args).ToJson());
        return Utility.GetReturnAs<string>(result);
    }

    public override string PromptAction(Player controlledPlayer, Match match)
    {
        var result = PromptActionF.Call(MatchParsers.CreateMState(controlledPlayer, match, "prompt action", new()).ToJson());
        return Utility.GetReturnAs<string>(result);
    }

    public override int PromptLane(string prompt, Player controlledPlayer, Match match)
    {
        var result = PromptLaneF.Call(MatchParsers.CreateMState(controlledPlayer, match, prompt, new()).ToJson());
        var rInt = Utility.GetReturnAsLong(result);
        return (int)rInt;
    }

    public override void Update(Player controlledPlayer, Match match)
    {
        UpdateF.Call(MatchParsers.CreateMState(controlledPlayer, match, "update", new()).ToJson());
    }

    public override void InformMatchEnd(Player controlledPlayer, Match match, bool won) {
        // TODO
    }

    public override string PickAttackTarget(Player controlledPlayer, Match match, CardW card) {
        // TODO
        return "player";
    }

}