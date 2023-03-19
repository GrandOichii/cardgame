using NLua;

using game.core;
using game.cards;
using game.match;
using game.deck;

namespace game.player {

    // A match partisipating player
    class Player : ILuaSerializable{
        static private int LastPid = 0;

        public int ID { get; }

        private string _name;
        public string Name { get => _name; }

        private int _maxEnergy = 0;
        public int MaxEnergy {
            get {
                // TODO
                return _maxEnergy;
            }
            set => _maxEnergy = value;
        }

        private int _energy = 0;
        public int Energy {
            get {
                // TODO
                return _energy;
            }
            set => _energy = value;
        }

        public int Life { get; set; }

        // cards
        public CardWrapper? Bond { get; }=null;

        public static readonly string ANYWHERE_ZONE = "anywhere";
        public static readonly string HAND_ZONE_NAME = "hand";
        public CardDeck Hand { get; }
        public static readonly string IN_PLAY_ZONE_NAME = "in_play";
        public CardDeck InPlay { get; }
        public static readonly string DECK_ZONE_NAME = "deck";
        public CardDeck Deck { get; }
        public static readonly string DISCARD_ZONE_NAME = "discard";
        public CardDeck Discard { get; }

        public Dictionary<string, CardDeck> Zones { get; }


        public PlayerController Controller { get; set; }
        public Player(Match match, string name, Deck deck, PlayerController controller) {
            Controller = controller;

            // base stats
            _name = name;

            // pid
            ID = LastPid;
            LastPid++;

            // bond setting
            if (deck.Bond is not null)
                Bond = new CardWrapper(match, deck.Bond);

            // deck setting
            Deck = CardDeck.From(match, deck);
            Deck.Shuffle();

            Hand = new(new());
            Discard = new(new());
            InPlay = new(new());

            Zones = new(){
                {HAND_ZONE_NAME, Hand},
                {DISCARD_ZONE_NAME, Discard},
                {DECK_ZONE_NAME, Deck},
                {IN_PLAY_ZONE_NAME, InPlay},
            };
        }

        public void DrawCards(int amount) {
            var cards = Deck.PopTop(amount);
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
            result["id"] = ID;
            result["energy"] = Energy;
            result["life"] = Life;
            return result;
        }
    }

    abstract class PlayerController {
        // TODO replace with PlayerInfo and MatchInfo
        abstract public string PromptAction(Player controlledPlayer);

    }

    class TerminalPlayerController : PlayerController {
        private void PrintCardsInZone(CardDeck zone, string zoneName) {
            System.Console.WriteLine("=== Cards in " + zoneName + " ===");
            foreach (var card in zone.Cards)
                System.Console.WriteLine(card.ID + ": " + card.Card.Name);

        }

        public override string PromptAction(Player player)
        {
            System.Console.WriteLine("Life: " + player.Life);
            System.Console.WriteLine("Energy: " + player.Energy);
            PrintCardsInZone(player.Hand, "hand");
            PrintCardsInZone(player.InPlay, "play");
            PrintCardsInZone(player.Discard, "discard");
            System.Console.Write("Enter action for " + player.Name + ": ");
            string? result = null;
            while (result is null)
                result = Console.ReadLine();
            System.Console.WriteLine();
            return result;
        }
    }
}