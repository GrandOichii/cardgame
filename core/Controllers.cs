using NLua;
using game.player;
using game.match;
using game.cards;
using game.core;
using game.util;
using Shared;
using System.Net.Sockets;
using System.Text;

namespace game.controllers {

    public class LuaBotController : PlayerController
    {
        public Lua LState { get; }

        public LuaFunction PromptF { get; }
        public LuaFunction PromptActionF { get; }
        public LuaFunction PromptLaneF { get; }
        public LuaFunction UpdateF { get; }


        public LuaBotController(string scriptPath) {
            LState = new();

            LState.DoFile(scriptPath);
            // var script = File.ReadAllText(scriptPath);
            // LState.DoString(script);

            PromptF = Utility.GetGlobalF(LState, "_Prompt");
            PromptActionF = Utility.GetGlobalF(LState, "_PromptAction");
            PromptLaneF = Utility.GetGlobalF(LState, "_PromptLane");
            UpdateF = Utility.GetGlobalF(LState, "_Update");

        }


        public override string ProcessPrompt(string type, string prompt, List<string> args, Player controlledPlayer, Match match, string sourceID)
        {
            // TODO change to created luatable
            var result = PromptF.Call(MatchParsers.CreateMState(controlledPlayer, match, type, args).ToJson());
            return Utility.GetReturnAs<string>(result);
        }

        public override string ProcessPromptAction(Player controlledPlayer, Match match)
        {
            var result = PromptActionF.Call(MatchParsers.CreateMState(controlledPlayer, match, "prompt action", new()).ToJson());
            return Utility.GetReturnAs<string>(result);
        }

        public override int ProcessPromptLane(string prompt, Player controlledPlayer, Match match, CardW? cursorCard=null)
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

        public override string ProcessPickAttackTarget(Player controlledPlayer, Match match, CardW card) {
            // TODO
            return "player";
        }

    }

    public class TCPPlayerController : PlayerController
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

    public override string ProcessPromptAction(Player controlledPlayer, Match match)
    {
        // Write("Enter command for " + controlledPlayer.Name + "\n" + ShortInfo(controlledPlayer));
        
        // TODO? args are available commands
        Write(MatchParsers.CreateMState(controlledPlayer, match, "enter command", new()).ToJson());
        return Read();
    }

    public override int ProcessPromptLane(string prompt, Player controlledPlayer, Match match, CardW? cursorCard=null)
    {
        // Write(prompt);

        // TODO? args are available lanes
        Write(MatchParsers.CreateMState(controlledPlayer, match, "pick lane", new(), "", "", cursorCard).ToJson());
        return int.Parse(Read());
    }

    public override string ProcessPrompt(string type, string prompt, List<string> args, Player controlledPlayer, Match match, string sourceID) {
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

    public override string ProcessPickAttackTarget(Player controlledPlayer, Match match, CardW card) {
        // TODO replace with available attacks
        var opponent = match.OpponentOf(controlledPlayer);
        var targets = new List<string>();
        foreach (var treasure in opponent.Treasures.Cards)
            targets.Add(treasure.GetCardWrapper().ID);

        Write(MatchParsers.CreateMState(controlledPlayer, match, "pick attack target", targets, "", card.ID).ToJson());
        return Read();
    }


}
}
