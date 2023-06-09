using NLua;

using game.util;
using game.core;
using game.cards;
using game.match;
using game.decks;
using game.recording;


namespace game.player {

    // A match partisipating player
    public class Player : IDamageable {
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
        public PlayerRecord Record { get; set; }

        public List<string> LastLogs { get; set; }

        public Deck OriginalDeck { get; }

        public Player(Match match, string name, Deck deck, PlayerController controller) {
            OriginalDeck = deck;

            Record = new();
            Record.DeckList = deck.ToText();

            LastLogs = new();
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
            Bond = deck.CreateBond(this, lState);
            Logger.Instance.Log("Player", "Creating deck");
            Deck = deck.ToDeckZone(this, lState);
            Deck.Shuffle(match.Rand);

            Hand = new(new());
            Discard = new(new());
            Treasures = new(new());
            Burned = new(new());

            Lanes = new UnitW[match.Config.LaneCount];

            Logger.Instance.Log("Player", "Finished creating player " + name);
        }

        public long ProcessDamage(Match match, long damage){
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
            var treasures = new List<object?>();
            foreach (var pair in cards) {
                if (pair.Value == Zones.DISCARD)
                    discard.Add(pair.Key.Info);
                if (pair.Value == Zones.UNITS)
                    units.Add(pair.Key.Info);
                if (pair.Value == Zones.TREASURES)
                    treasures.Add(pair.Key.Info);
            }

            result["discard"] = Utility.CreateTable(lState, discard);
            result["units"] = Utility.CreateTable(lState, units);
            result["treasures"] = Utility.CreateTable(lState, treasures);

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

            result["bond"] = Bond.Info;

            return result;
        }

        public string ShortStr() => Name + " (" + ID + ")";

        public Dictionary<CardW, string> GetAllCards() {
            var result = new Dictionary<CardW, string>();

            result.Add(Bond, Zones.BOND);
            foreach (var unit in Lanes) {
                if (unit is null) continue;
                result.Add(unit.Card, Zones.UNITS);
            }
            foreach (var treasure in Treasures.Cards)
                result.Add(treasure.Card, Zones.TREASURES);

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

        public List<HasMarkedDamage> HasMarkedDamageCards() {
            var result = new List<HasMarkedDamage>();
            foreach (var treasure in Treasures.Cards)
                result.Add(treasure);
            foreach (var unit in Lanes)
                if (unit is not null) result.Add(unit);
            return result;
        }
    }

    #region Player Controllers
    public abstract class PlayerController {
        public string PromptAction(Player controlledPlayer, Match match) {
            var result = ProcessPromptAction(controlledPlayer, match);
            match.RecordPlayerAction(controlledPlayer, result);
            return result;
        }

        public int PromptLane(string prompt, Player controlledPlayer, Match match, CardW? cursorCard=null) {
            var result = ProcessPromptLane(prompt, controlledPlayer, match, cursorCard);
            match.RecordPlayerAction(controlledPlayer, ""+result);
            return result;
        }

        public string Prompt(string type, string prompt, List<string> args, Player controlledPlayer, Match match, string sourceID) {
            var result = ProcessPrompt(type, prompt, args, controlledPlayer, match, sourceID);
            match.RecordPlayerAction(controlledPlayer, result);
            return result;
        }

        public string PickAttackTarget(Player controlledPlayer, Match match, CardW card) {
            var result = ProcessPickAttackTarget(controlledPlayer, match, card);
            match.RecordPlayerAction(controlledPlayer, result);
            return result;
        }

        abstract public string ProcessPromptAction(Player controlledPlayer, Match match);
        abstract public int ProcessPromptLane(string prompt, Player controlledPlayer, Match match, CardW? cursorCard=null);
        abstract public void Update(Player controlledPlayer, Match match);
        abstract public string ProcessPrompt(string type, string prompt, List<string> args, Player controlledPlayer, Match match, string sourceID);
        abstract public string ProcessPickAttackTarget(Player controlledPlayer, Match match, CardW card); // possible results: IF TREASURE - id of the card being attacked, IF PLAYER - "player"
        abstract public void InformMatchEnd(Player controlledPlayer, Match match, bool won);

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
        public override string ProcessPickAttackTarget(Player controlledPlayer, Match match, CardW card) {
            // TODO
            return "";
        }


        public override string ProcessPromptAction(Player player, Match match)
        {
            System.Console.WriteLine(ShortInfo(player));
            System.Console.Write("\tEnter action for " + player.Name + ": ");
            string? result = null;
            while (result is null)
                result = Console.ReadLine();
            System.Console.WriteLine();
            return result;
        }

        public override int ProcessPromptLane(string prompt, Player controlledPlayer, Match match, CardW? cursorCard=null)
        {
            string? result;
            do {
                System.Console.Write(prompt + ": ");
                result = Console.ReadLine();
            } while (result is null || result.Length == 0);
            
            return int.Parse(result);
        }

        public override string ProcessPrompt(string type, string prompt, List<string> args, Player controlledPlayer, Match match, string sourceID)
        {
            return "";
            // throw new NotImplementedException();
        }

        public override void Update(Player controlledPlayer, Match match)
        {
            // throw new NotImplementedException();
        }

        public override void InformMatchEnd(Player controlledPlayer, Match match, bool won) {
            // throw new NotImplementedException();
        }
    }

    
    #endregion
}