using System.Linq.Expressions;
using System.Reflection;

using game.match;
using game.player;
using game.cards;
using game.util;

namespace game.scripts
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class LuaCommand : Attribute {}

    class ScriptMaster
    {
        private Match _match;
        public ScriptMaster(Match parent) {

            _match = parent;

            var type = typeof(ScriptMaster);
            foreach (var method in type.GetMethods())
            {
                if (method.GetCustomAttribute(typeof(LuaCommand)) is not null)
                {
                    _match.LState[method.Name] = method.CreateDelegate(Expression.GetDelegateType(
                        (from parameter in method.GetParameters() select parameter.ParameterType)
                        .Concat(new[] { method.ReturnType })
                    .ToArray()), this);
                }
            }

        }

        private Player GetPlayer(int pID) {
            var player = _match.PlayerByID(pID);
            if (player is null) throw new Exception("No player with ID " + pID);
            return player;
        }


        private CardW GetCard(string cID) {
            var card = _match.AllCards[cID];
            if (card is null) throw new Exception("No card with ID " + cID);
            return card;
        }


        [LuaCommand]
        public void Log(string message) {
            Logger.Instance.Log("LSTATE", message);
        }


        [LuaCommand]
        public void PlaceIntoDiscard(string cID, int pID) {
            var card = GetCard(cID);
            var player = GetPlayer(pID);
            Logger.Instance.Log("ScriptMaster", "Card " + card.ShortStr() + " is put into " + player.ShortStr() + "'s discard");

            player.Discard.AddToBack(card);
        }

    }
}
