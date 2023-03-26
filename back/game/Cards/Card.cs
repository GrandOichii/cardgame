using NLua;

using game.core;
using game.util;
using game.match;
using game.cards.loaders;

namespace game.cards {

    

    class CardMaster {
        private CardLoader _loader;
        private Dictionary<string, Card> _cardIndex;
        private Dictionary<string, int> _refCount;
        public void Load(string cName, string colName) {
            var name = FmtCard(cName, colName);
            Logger.Instance.Log("CardMaster", "Requested to load card " + name);
            if (!_cardIndex.ContainsKey(name)) {
                Logger.Instance.Log("CardMaster", "Card not present, loading...");
                
                // TODO load card
                var card = _loader.Load(cName, colName);
                _cardIndex.Add(name, card);
                _refCount.Add(name, 0);
            }

            _refCount[name]++;
            Logger.Instance.Log("CardMaster", "Increasing reference counter of card " + name + " to " + _refCount[name]);
        }

        public void Unload(string cName, string colName) {
            var name = FmtCard(cName, colName);
            Logger.Instance.Log("CardMaster", "Requested to unload card " + name);
            if (!_cardIndex.ContainsKey(name)) throw new Exception("Tried to unload a card that is not loaded: " + name);
            _refCount[name]--;
            if (_refCount[name] > 0) return;
            Logger.Instance.Log("CardMaster", "Reference counter of  " + name + " reached 0, removing it");

            // remove card from memory
            _cardIndex.Remove(name);
            _refCount.Remove(name);

            Logger.Instance.Log("CardMaster", "Card  " + name + " was removed from CardMaster");
        }

        public Card Get(string cName, string colName) {
            var name = FmtCard(cName, colName);
            Logger.Instance.Log("CardMaster", "Requested to get card  " + name);
            if (!_cardIndex.ContainsKey(name)) throw new Exception("Tried to get a card that is not loaded in Card Master: " + name);
            return _cardIndex[name]; 
        }

        private string FmtCard(string cName, string colName) => colName + ":" + cName;

        public CardMaster(CardLoader loader) {
            _loader = loader;
            _cardIndex = new();
            _refCount = new();
        }

        public bool CheckEmpty() => _cardIndex.Count == 0;

        public void LogContents() {
            Logger.Instance.Log("CardMaster", "Logging content of CardMaster...");
            foreach (var cName in _cardIndex.Keys) {
                var card = _cardIndex[cName];
                var refC = _refCount[cName];
                Logger.Instance.Log("CardMaster", cName + " --- " + "refCount: " + refC);
            }
            Logger.Instance.Log("CardMaster", "Finished");
        }
    }

    // card template class
    class Card {
        static public readonly string WRAPPER_CREATION_FNAME = "_CreateCard";
        
        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Text { get; private set; }
        public string Collection { get; private set; }
        public string ScriptPath { get; private set; }

        public Card(string name, string type, string text, string collection, string scriptPath) {
            Name = name;
            Type = type;
            Text = text;
            Collection = collection;
            ScriptPath = scriptPath;
        }

        public LuaTable GetProps(Lua lState) {
            var result = Utility.CreateTable(lState);
            result["name"] = Name;
            result["type"] = Type;
            return result;
        }

        public CardW ConstructWrapper(Lua lState) {
            lState.DoFile(ScriptPath);
            var creationF = Utility.GetGlobalF(lState, WRAPPER_CREATION_FNAME);
            var props = GetProps(lState);
            var returned = creationF.Call(props);
            var table = Utility.GetReturnAsTable(returned);
            return new CardW(this, table);
        }
    }


    interface IHasCardW {
        public CardW GetCardWrapper();

        public string InfoStr();
    }

    // card wrapper object
    class CardW  : IHasCardW {
        static public string CAN_PLAY_FNAME = "CanPlay";
        static public string PLAY_FNAME = "Play";
        static public string PAY_COSTS_FNAME = "PayCosts";

        static public IDCreator IDCreator = new BasicIDCreator();
        public string ID { get; private set; }
        public Card Original { get; private set; }
        public LuaTable Info { get; private set; }
        public CardW(Card original, LuaTable info) {
            ID = IDCreator.Next();
            Original = original;
            Info = info;

            Info["id"] = ID;
        }

        public string ShortStr() => Original.Name + " (" + ID + ")";

        public CardW GetCardWrapper() => this;

        public object[] ExecFunc(string fName, params object[] args) {
            var f = Utility.TableGet<LuaFunction>(Info, fName);
            return f.Call(args);
        }

        public bool ExecCheckerFunc(string fName, params object[] args) {
            var results = ExecFunc(fName, args);
            var result = Utility.GetReturnAsBool(results);
            return result;
        }

        public string InfoStr()
        {
            return "";
        }
    }


    abstract class HasMarkedDamage : IDamageable, IHasCardW
    {
        protected virtual string requiredCardType { get; }
        public CardW Card { get; private set; }

        public HasMarkedDamage(CardW card) {
            if (card.Original.Type != requiredCardType) throw new Exception("Tried to create a UnitW out of non-unit card " + card.Original.Name);
            Card = card;


            // check that has life
            var _ = Life;
        }

        public virtual long ProcessDamage(Match match, long damage)
        {
            var l = Life;
            if (damage > l) damage = l;
            Life -= damage;

            // TODO emit that card is destroyed
            return l - Life; 
        }

        public long Life
        {
            get =>  Utility.GetLong(Card.Info, "life");
            set {
                if (value < 0) value = 0;
                Card.Info["life"] = value;
            }
        }
        

        public CardW GetCardWrapper() => Card;

        public string InfoStr()
        {
            return "Life: " + Life;
        }
    }


    class UnitW : HasMarkedDamage
    {
        protected override string requiredCardType => "Unit";
        public UnitW(CardW card) : base(card)
        {
            // check that it has power
            GetPower();
        }

        public long GetPower() {
            return Utility.GetLong(Card.Info, "power");
        }

        public string ToShortStr() {
            return Card.Original.Name + ": " + GetPower() + "/" + Life;
        }
    }

    class TreasureW : HasMarkedDamage
    {
        protected override string requiredCardType => "Treasure";
        public TreasureW(CardW card) : base(card)
        {
        }

        public override long ProcessDamage(Match match, long damage)
        {
            var result = base.ProcessDamage(match, damage);
            if (Life == 0) {
                Destroy(match);
            }

            return result;
        }

        public void Destroy(Match match) {
            match.Emit("treasure_destroyed", new(){{"treasure", Card.Info}});

            var owner = match.OwnerOf(Card.ID);
            Logger.Instance.Log("TreasureW", "Treasure card " + Card.ShortStr() + " of player " + owner.ShortStr() + " was destroyed");

            owner.Treasures.Cards.Remove(this);
            owner.PlaceIntoDiscard(this);
        }
    }

}