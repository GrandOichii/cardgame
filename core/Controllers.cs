using NLua;
using game.player;
using game.match;
using game.cards;
using game.core;
using game.util;

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
}
