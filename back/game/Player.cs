using NLua;

using game.core;
using game.cards;
using game.match;
using game.deck;

namespace game.player {

    // A match partisipating player
    class Player : ILuaSerializable{
        static private int LastPid = 0;

        public int PID { get; }

        private string _name;
        public string Name { get => _name; }

        private int _maxEnergy = 0;
        public int MaxEnergy {
            get {
                // TODO
                return _maxEnergy;
            }
        }

        private int _energy = 0;
        public int Energy {
            get {
                // TODO
                return _energy;
            }
        }

        public int Life { get; set; }

        // cards
        public CardWrapper? Bond { get; }=null;

        public CardDeck Hand { get; }
        public CardDeck Deck { get; }
        public CardDeck Discard { get; }


        public PlayerController Controller { get; set; }
        public Player(Match match, string name, Deck deck, PlayerController controller) {
            Controller = controller;

            // base stats
            _name = name;

            // pid
            PID = LastPid;
            LastPid++;

            // TODO
            // deck setting
            if (deck.Bond is not null)
                Bond = new CardWrapper(deck.Bond);

            Deck = CardDeck.From(deck);
            Deck.Shuffle();

            Hand = new(new());
            Discard = new(new());
        }

        public void DrawCards(int amount) {
            var cards = Deck.PopTop(1);
            Hand.AddToBack(cards);
        }

        public string PromptAction() {
            return Controller.PromptAction(this);
        }

        public int PromptDiscard(int amount, bool force) {
            // TODO
            return 0;
        }

        public void LoadDeckTemplate(Deck template) {
            // TODO
        }

        public LuaTable ToLuaTable(Lua lState) {
            // TODO
            lState.NewTable("result");
            var result = lState.GetTable("result");
            result["name"] = _name;
            result["id"] = PID;
            return result;
        }
    }

    abstract class PlayerController {
        // TODO replace with PlayerInfo and MatchInfo
        abstract public string PromptAction(Player controlledPlayer);

    }

    class TerminalPlayerController : PlayerController {
        public override string PromptAction(Player controlledPlayer)
        {
            System.Console.WriteLine("Cards in hand:");
            foreach (var card in controlledPlayer.Hand.Cards)
                System.Console.WriteLine(card.ID + ": " + card.Card.Name);
            System.Console.Write("Enter action for " + controlledPlayer.Name + ": ");
            string? result = null;
            while (result is null)
                result = Console.ReadLine();

            return result;
        }
    }
}