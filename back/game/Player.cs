using NLua;

using game.util;
using game.core;
using game.cards;
using game.match;
using game.decks;


namespace game.player {

    // A match partisipating player
    class Player : IDamageable {
        public static readonly string ANYWHERE_ZONE = "anywhere";
        public static readonly string HAND_ZONE_NAME = "hand";
        public static readonly string TREASURES_ZONE_NAME = "treasures";
        public static readonly string DECK_ZONE_NAME = "deck";
        public static readonly string DISCARD_ZONE_NAME = "discard";


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

        #region Card Zones
        public CardW Bond { get; }

        public Zone<CardW> Hand { get; }
        public UnitW[] Lanes { get; private set; }
        public Zone<CardW> Deck { get; }
        public Zone<CardW> Discard { get; }
        public Zone<TreasureW> Treasures { get; set; }

        private Match _match;

        #endregion


        public PlayerController Controller { get; set; }


        public Player(Match match, string name, Deck deck, PlayerController controller) {
            LastPid++;
            ID = LastPid;

            _match = match;
            Logger.Instance.Log("Player", "Creating player " + name);

            Controller = controller;

            _name = name;

            Logger.Instance.Log("Player", "Creating bond");
            Bond = deck.CreateBond(match.LState);
            Logger.Instance.Log("Player", "Creating deck");
            Deck = deck.ToDeckZone(match.LState);
            Deck.Shuffle();

            Hand = new(new());
            Discard = new(new());
            Treasures = new(new());

            Lanes = new UnitW[match.Config.LaneCount];

            Logger.Instance.Log("Player", "Finished creating player " + name);
        }

        public long ProcessDamage(long damage)
        {
            // TODO
            throw new NotImplementedException();
        }
    
        public void DrawCards(int amount) {
            // TODO replace with draw cards call to lua state
            var cards = Deck.PopTop(amount);

            _match.Emit("card_draw", new(){{"player", ToLuaTable(_match.LState)}});

            Hand.AddToBack(cards);
        }

        public LuaTable ToLuaTable(Lua lState) {
            var result = Utility.CreateTable(lState);
            result["name"] = Name;
            result["id"] = ID;
            return result;
        }

        public string ShortStr() => Name + " (" + ID + ")";
    }

    #region Player Controllers
    abstract class PlayerController {
        abstract public string PromptAction(Player controlledPlayer, Match match);
    }


    class TerminalPlayerController : PlayerController {
        private void PrintCardsInZone<T>(Zone<T> zone, string zoneName) where T : IHasCardW {
            System.Console.WriteLine("=== Cards in " + zoneName + " ===");
            foreach (var card in zone.Cards)
                System.Console.WriteLine(card.GetCardWrapper().ID + ": " + card.GetCardWrapper().Original.Name);

        }

        public override string PromptAction(Player player, Match match)
        {
            System.Console.WriteLine("Life: " + player.Life);
            System.Console.WriteLine("Energy: " + player.Energy);
            for (int i = 0; i < player.Lanes.Length; i++) {
                var unit = player.Lanes[i];
                var addMessage = "None";
                if (unit is not null) addMessage = unit.ToShortStr();
                System.Console.WriteLine("Lane " + i + ": " + addMessage);
            }
            PrintCardsInZone(player.Treasures, "treasures");
            PrintCardsInZone(player.Hand, "hand");
            PrintCardsInZone(player.Discard, "discard");
            System.Console.Write("Enter action for " + player.Name + ": ");
            string? result = null;
            while (result is null)
                result = Console.ReadLine();
            System.Console.WriteLine();
            return result;
        }
    }
    #endregion
}