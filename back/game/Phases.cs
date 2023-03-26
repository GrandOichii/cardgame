using game.match;
using game.player;
using game.core.actions;

namespace game.core.phases {
    abstract class GamePhase {
        abstract public void Exec(Match match, Player player);
    }

    class TurnStart : GamePhase
    {
        public override void Exec(Match match, Player player)
        {
            // replenish source count
            player.SourceCount = player.MaxSourcePerTurn;
            System.Console.WriteLine(player.Name + "  " + player.SourceCount);

            // replenish energy
            player.Energy = player.MaxEnergy;

            // emit turn start effects
            match.Emit("turn_start", new(){ {"player", player.ToLuaTable(match.LState)} });

            // TODO
            // replenish all units' attacks
            // foreach (var card in player.InPlay.Cards)
            //     if (card.Table["availableAttacks"] is not null)
            //         // TODO replace if units will be able to attack multiple times
            //         card.Table["availableAttacks"] = 1;

            // draw for the turn
            player.DrawCards(match.Config.TurnStartCardDraw);
        }
    }

    class MainPhase : GamePhase
        {
            private readonly string PASS_TURN_ACTION = "pass";
            private static readonly Dictionary<string, actions.GameAction> ACTION_MAP =
            new(){
                { "play", new PlayCardAction() },
                // { "attack", new actions.AttackAction() }
            };

            public override void Exec(Match match, Player player)
            {
                string action;
                while (true)
                {
                    action = PromptAction(match, player);
                    var words = action.Split(" ");

                    var actionWord = words[0];
                    if (actionWord == PASS_TURN_ACTION) break;

                    // TODO remove
                    if (actionWord == "quit")
                    {
                        match.Winner = player;
                        return;
                    }
                    
                    if (!ACTION_MAP.ContainsKey(actionWord)) throw new Exception("Unknown action from player " + player.Name + ": " + actionWord);

                    ACTION_MAP[actionWord].Exec(match, player, words);
                    
                }
                // TODO
            }

            private string PromptAction(Match match, Player player)
            {
                // TODO get all available actions
                return player.Controller.PromptAction(player, match);
            } 
        }

        class TurnEnd : GamePhase
        {
            public override void Exec(Match match, Player player)
            {
                match.Emit("turn_end", new(){ {"player", player.ToLuaTable(match.LState)} });

                // TODO
                // discard to hand size
                // int discarded = player.PromptDiscard(match.Config.MaxHandSize - player.Hand.Cards.Count, true);
            }
        }
}