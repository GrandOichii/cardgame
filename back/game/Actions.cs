
using game.match;
using game.player;

namespace game.core.actions {

    abstract class GameAction
    {
        abstract public void Exec(Match match, Player player, string[] args);
    }


    

}