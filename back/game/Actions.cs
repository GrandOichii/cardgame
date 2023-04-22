
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

    class AttackAction : GameAction
    {
        public override void Exec(Match match, Player player, string[] args)
        {
            if (args.Length < 2 || args.Length > 3) throw new Exception("Incorrect number of arguments for attack action");

            var lane = int.Parse(args[1]);
            var lanes = player.Lanes;
            var attacker = lanes[lane];

            // TODO ingore action
            if (attacker is null) throw new Exception("Player " + player.ShortStr() + " tried to attack in lane " + lane + ", where they don't have a unit");

            // TODO ignore action
            if (attacker.AvailableAttacks == 0) throw new Exception("Player " + player.ShortStr() + " tried to attack with " + attacker.Card.ShortStr() + ", which can't attack");
            attacker.AvailableAttacks--;
            var attackerPower = attacker.GetPower();

            var opponent = match.OpponentOf(player);
            IDamageable? target = opponent.Lanes[lane];
            var defender = opponent.Lanes[lane];
            long dealt;

            // TODO ignore action
            if (defender is not null) {
                if (args.Length == 3)
                    throw new Exception("Player " + player.ShortStr() + " tried to attack in lane " + lane + " treasure with id " + args[2] + ", while opponent has a unit in that lane");
                // deal damage to attacker
                target = defender;
                dealt = attacker.ProcessDamage(match, defender.GetPower());
                Logger.Instance.Log("Attack", "Unit " + defender.ToShortStr() + " dealt " + dealt + " damage to " + attacker.ToShortStr());
            }

            if (target is null) {
                target = opponent;
                if (args.Length == 3) {
                    var tID = args[2];
                    // attack treasure
                    foreach (var treasure in opponent.Treasures.Cards){
                        if (treasure.Card.ID == tID) {
                            target = treasure;
                            break;
                        }
                    }
                    // TODO ingore action
                    if (target == opponent) throw new Exception("Player tried to attack treasure with ID " + tID);
                }
            }

            // deal damage to defender/target
            dealt = target.ProcessDamage(match, attackerPower);
            Logger.Instance.Log("Attack", "Unit " + attacker.ToShortStr() + " dealt " + dealt + " damage");
        }
    }

}