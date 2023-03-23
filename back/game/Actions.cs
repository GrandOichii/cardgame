
using game.match;
using game.util;
using game.player;
using game.cards;

namespace game.core.actions {

    abstract class GameAction
    {
        abstract public void Exec(Match match, Player player, string[] args);
    }


    class PlayCardAction : GameAction
    {
        public override void Exec(Match match, Player player, string[] args)
        {
            if (args.Length != 2) throw new Exception("Incorrect number of arguments for PlayCardAction");
            
            var pTable = player.ToLuaTable(match.LState);

            var cID = args[1];
            var card = player.Hand[cID];

            // TODO can't play a card with id that's not in your hand, suspect cheating
            if (card is null) throw new Exception("Player " + player.Name + "failed to play card with ID " + cID + ": it's not in their hand");

            var canPlay = card.ExecCheckerFunc(CardW.CAN_PLAY_FNAME, card.Info, pTable);
            if (!canPlay) {
                Logger.Instance.Log("WARN", "Player " + player.Name + " tried to play a card they can't play: " + card.ShortStr());
                return;
            }

            var payed = card.ExecCheckerFunc(CardW.PAY_COSTS_FNAME, card.Info, pTable);
            if (!payed) {
                Logger.Instance.Log("WARN", "Player " + player.Name + " decided not to cast card " + card.ShortStr());
                return;
            }
            // TODO figure out order of removing card in have, executing effect and putting in discard
            player.Hand.Cards.Remove(card);
            card.ExecFunc(CardW.PLAY_FNAME, card.Info, pTable);
        }
    }

}