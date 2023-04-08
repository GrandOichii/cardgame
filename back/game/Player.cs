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

        public string Name { get; private set; }

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

        public long Life { get; set; }

        #region Card Zones
        public CardW Bond { get; private set; }

        public Zone<CardW> Hand { get; private set; }
        public UnitW?[] Lanes { get; private set; }
        public Zone<CardW> Deck { get; private set; }
        public Zone<CardW> Discard { get; private set; }
        public Zone<TreasureW> Treasures { get; private set; }
        public Zone<CardW> Burned { get; private set; }

        private Match _match;

        #endregion

        public LuaTable Shared { get; private set; }


        // TODO replace with a lua function
        public int MaxSourcePerTurn { get; }
        public long SourceCount { 
            get => Utility.GetLong(Shared, "sourceCount");
            set => Shared["sourceCount"] = value;
        }


        public PlayerController Controller { get; set; }


        public Player(Match match, string name, Deck deck, PlayerController controller) {
            var lState = match.LState;
            MaxSourcePerTurn = match.Config.BaseSourcePerTurn;

            LastPid++;
            ID = LastPid;
            Shared = Utility.CreateTable(lState);

            _match = match;
            Logger.Instance.Log("Player", "Creating player " + name);

            Controller = controller;

            Name = name;

            Logger.Instance.Log("Player", "Creating bond");
            Bond = deck.CreateBond(lState);
            Logger.Instance.Log("Player", "Creating deck");
            Deck = deck.ToDeckZone(lState);
            Deck.Shuffle();

            Hand = new(new());
            Discard = new(new());
            Treasures = new(new());
            Burned = new(new());

            Lanes = new UnitW[match.Config.LaneCount];

            Logger.Instance.Log("Player", "Finished creating player " + name);
        }

        public long ProcessDamage(Match match, long damage)
        {
            Life -= damage;
            if (Life <= 0) {
                var opponent = match.OpponentOf(this);
                match.Winner = opponent;
            }
            return damage;
        }
    
        public void DrawCards(int amount) {
            // TODO replace with draw cards call to lua state
            var cards = Deck.PopTop(amount);

            _match.Emit("card_draw", new(){{"player", ToLuaTable(_match.LState)}, {"amount", amount}});

            Hand.AddToBack(cards);
        }

        public LuaTable ToLuaTable(Lua lState) {
            var result = Utility.CreateTable(lState);
            result["name"] = Name;
            result["id"] = ID;
            result["energy"] = Energy;
            result["hand"] = Hand.ToLuaTable(lState);
            result["shared"] = Shared;

            var cards = GetAllCards();
            var discard = new List<object?>();
            var units = new List<object?>();
            foreach (var pair in cards) {
                if (pair.Value == Zones.DISCARD)
                    discard.Add(pair.Key);
                if (pair.Value == Zones.UNITS)
                    units.Add(pair.Key);
            }

            result["discard"] = Utility.CreateTable(lState, discard);
            result["units"] = Utility.CreateTable(lState, units);

            var lanes = Utility.CreateTable(lState);
            for (int i = 0; i < Lanes.Length; i++) {
                var t = Utility.CreateTable(lState);
                var l = Lanes[i];
                t["isSet"] = false;
                t["unit"] = null;
                if (l is null) {
                    lanes[i+1] = t;
                    continue;
                }

                t["isSet"] = true;
                t["unit"] = l.Card.Info;

                lanes[i+1] = t;
            }

            result["lanes"] = lanes;

            return result;
        }

        public string ShortStr() => Name + " (" + ID + ")";

        public Dictionary<CardW, string> GetAllCards() {
            var result = new Dictionary<CardW, string>();

            result.Add(Bond, Zones.BOND);
            foreach (var unit in Lanes) {
                if (unit is null) continue;
                result.Add(unit.GetCardWrapper(), Zones.UNITS);
            }
            foreach (var treasure in Treasures.Cards)
                result.Add(treasure.GetCardWrapper(), Zones.TREASURES);

            foreach (var card in Hand.Cards)
                result.Add(card, Zones.HAND);
            foreach (var card in Deck.Cards)
                result.Add(card, Zones.DECK);
            foreach (var card in Discard.Cards)
                result.Add(card, Zones.DISCARD);
            foreach (var card in Burned.Cards)
                result.Add(card, Zones.BURNED);
                
            return result;
        }
    
        public void PlaceIntoDiscard(IHasCardW card) {
            var cw = card.GetCardWrapper();
            _match.Emit("placed_into_discard", new(){{"card", cw.Info}});

            Discard.AddToBack(cw);
        }
    }

    #region Player Controllers
    abstract class PlayerController {
        abstract public string PromptAction(Player controlledPlayer, Match match);
        abstract public int PromptLane(string prompt, Player controlledPlayer, Match match);
        abstract public void Update(Player controlledPlayer, Match match);
        protected string ShortInfo(Player player) {
            var result = "";
            result += "\tLife: " + player.Life + "\n";
            result += "\tEnergy: " + player.Energy + "\n";
            for (int i = 0; i < player.Lanes.Length; i++) {
                var unit = player.Lanes[i];
                var addMessage = "None";
                if (unit is not null) addMessage = unit.ToShortStr();
                result += "\tLane " + i + ": " + addMessage + "\n";
            }
            result += CardsInZoneToString(player.Treasures, "treasures");
            result += CardsInZoneToString(player.Hand, "hand");
            result += CardsInZoneToString(player.Discard, "discard");
            return result;
        }

        private string CardsInZoneToString<T>(Zone<T> zone, string zoneName) where T : IHasCardW {
            var result = "";
            result += "\t=== Cards in " + zoneName + " ===\n";
            foreach (var card in zone.Cards)
                result += "\t" + card.GetCardWrapper().ID + ": " + card.GetCardWrapper().Original.Name + "  (" + card.InfoStr() + ")\n";
            return result;
        }
    }


    class TerminalPlayerController : PlayerController {

        public override string PromptAction(Player player, Match match)
        {
            System.Console.WriteLine(ShortInfo(player));
            System.Console.Write("\tEnter action for " + player.Name + ": ");
            string? result = null;
            while (result is null)
                result = Console.ReadLine();
            System.Console.WriteLine();
            return result;
        }

        public override int PromptLane(string prompt, Player controlledPlayer, Match match)
        {
            string? result;
            do {
                System.Console.Write(prompt + ": ");
                result = Console.ReadLine();
            } while (result is null || result.Length == 0);
            
            return int.Parse(result);
        }

        public override void Update(Player controlledPlayer, Match match)
        {
            // throw new NotImplementedException();
        }
    }

    
    #endregion
}