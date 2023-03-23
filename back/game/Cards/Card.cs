using NLua;

using game.core;
using game.util;
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

        private string FmtCard(string cName, string colName) => cName + ":" + colName;

        public CardMaster(CardLoader loader) {
            _loader = loader;
            _cardIndex = new();
            _refCount = new();
        }
    }

    // card template class
    class Card {
        static public readonly string WRAPPER_CREATION_FNAME = "_CreateCard";
        
        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Text { get; private set; }
        public string Collection { get; private set; }
        public string Script { get; private set; }

        public Card(string name, string type, string text, string collection, string script) {
            Name = name;
            Type = type;
            Text = text;
            Collection = collection;
            Script = script;
        }

        public LuaTable GetProps(Lua lState) {
            var result = Utility.CreateTable(lState);
            result["name"] = Name;
            result["type"] = Type;
            return result;
        }

        public CardW ConstructWrapper(Lua lState) {
            var creationF = Utility.GetGlobalF(lState, WRAPPER_CREATION_FNAME);
            var props = GetProps(lState);
            var returned = creationF.Call(props);
            var table = Utility.GetReturnAsTable(returned);
            return new CardW(this, table);
        }
    }


    // card wrapper object
    class CardW {
        public Card Original { get; private set; }
        public LuaTable Info { get; private set; }
        public CardW(Card original, LuaTable info) {
            Original = original;
            Info = info;
        }
    }


    abstract class HasMarkedDamage : Damageable
    {
        protected virtual string requiredCardType { get; }
        public CardW Card { get; private set; }

        public long MarkedDamage { get; private set; }

        public HasMarkedDamage(CardW card) {
            if (card.Original.Type != requiredCardType) throw new Exception("Tried to create a UnitW out of non-unit card " + card.Original.Name);
            Card = card;
        }

        public long ProcessDamage(long damage)
        {
            long original = MarkedDamage;
            MarkedDamage += damage;
            var l = GetLife();
            if (MarkedDamage > l) MarkedDamage = l;

            // TODO emit that card is destroyed
            return MarkedDamage - original; 
        }

        public long GetLife() {
            var t = Card.Info;
            var life = Utility.GetLong(t, "life");
            return life;
        }
    }


    class UnitW : HasMarkedDamage
    {
        protected override string requiredCardType => "Unit";
        public UnitW(CardW card) : base(card)
        {
        }
    }

    class TreasureW : HasMarkedDamage
    {
        protected override string requiredCardType => "Tresure";
        public TreasureW(CardW card) : base(card)
        {
        }
    }

}