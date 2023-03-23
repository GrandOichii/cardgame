using NLua;

using game.core;
using game.util;

namespace game.cards {

    class CardMaster {
        public void Load(string cName, string colName) {

        }

        public void Unload(string cName, string colName) {
            
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