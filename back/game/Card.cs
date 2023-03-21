using System.Text.Json;
using System.Text.Json.Serialization;
using NLua;

using game.core;
using game.match;
using System.Net;


namespace game.cards {
    class CardWrapper : IDamageable {
        static public IIDCreator IDCreator = new IncrementIDCreator(); 

        public string ID { get; }
        public Card Card { get; }
        public LuaTable Table { get; }
        public CardWrapper(Match match, Card card) {
            ID = IDCreator.CreateID();

            Card = card;

            var state = match.LState;
            state.DoFile(card.ScriptPath);
            var creationFunc = state[Card.CREATION_FNAME] as LuaFunction;
            if (creationFunc is null) throw new Exception("No creation func in card " + Card.Name);
            var props = card.ToLuaProps(state);
            var t = creationFunc.Call(props)[0] as LuaTable;
            if (t is null) throw new Exception("Card creation script from " + card.ScriptPath + " did not create a card object");
            Table = t;
            Table["id"] = ID;
        }

        public int ProcessDamage(int damage)
        {
            var healthO = Table["health"];
            if (healthO is null) throw new Exception("Tried to damage " + Card.Name + " (" + ID + "), which is not damageable");
            int health = (int) healthO;
            int or = health;
            health -= damage;
            Table["health"] = health;
            return or - (int)Table["health"];
        }

        // public LuaTable ToLuaTable(Lua lState)
        // {
        //     var result = Card.ToLuaProps(lState);
        //     result["id"] = ID;
        //     return result;
        // }
    }

    class JCard {
        [JsonPropertyName("name")]
        public string Name { get; set; }="";

        [JsonPropertyName("text")]
        public string Text { get; set; }="";
        [JsonPropertyName("type")]
        public string Type { get; set; }="";

        [JsonPropertyName("script")]
        public string ScriptPath { get; set; }="";
    }

    class Card {
        static public string CAST_COST_FNAME = "cast_cost";
        static public string ON_CAST_FNAME = "on_cast";
        static public string CAN_CAST_FNAME = "can_cast";
        static public string CREATION_FNAME = "_CreateCard";

        static private string CARD_INFO_PATH = "card.json";

        public string Name { get; }
        public string Text { get; }
        public string Type { get; }
        public string ScriptPath { get; }

        protected Card(string name, string text, string type, string scriptPath) {
            Name = name;
            Text = text;
            Type = type;
            ScriptPath = scriptPath;
        }

        static public Card Load(string path) {
            var text = File.ReadAllText(Path.Join(path, CARD_INFO_PATH));
            var template = JsonSerializer.Deserialize<JCard>(text);

            if (template is null) 
                throw new Exception("Failed to load card from " + path);

            var result = new Card(
                template.Name,
                template.Text,
                template.Type,
                Path.Join(path, template.ScriptPath)
            );

            return result;
        }

        public LuaTable ToLuaProps(Lua lState)
        {
            lState.NewTable("result");
            var result = lState.GetTable("result");
            result["name"] = Name;
            result["type"] = Type;
            return result;
        }

        //public LuaTable ToLuaTable(Lua lState)
        //{
        //    lState.NewTable("result");
        //    var result = lState.GetTable("result");
        //    result["name"] = Name;
        //    return result;
        //}
    }
}

//namespace game.cards1
//{

//    #region Wrapper

//    class CardWrapper {
//        static public IIDCreator IDCreator = new IncrementIDCreator(); 

//        private string ID { get; }
//        private Card Card { get; }
//        public CardWrapper(Match match, Card card) {
//            ID = IDCreator.CreateID();

//            Card = card;

//            //var state = match.LState;
//            //state.DoFile(card.ScriptPath);
//        }
//    }
    
//    #endregion
    
//    #region Base
//    class JCard {
//        [JsonPropertyName("name")]
//        public string Name { get; set; }="";

//        [JsonPropertyName("text")]
//        public string Text { get; set; }="";

//        [JsonPropertyName("script")]
//        public string ScriptPath { get; set; }="";
//    }

//    class Card : ILuaSerializable {
//        static private string CARD_INFO_PATH = "card.json";

//        public string Name { get; }

//        protected Card(string name) {
//            Name = name;
//        }

//        static public Card Load(string path) {
//            var text = File.ReadAllText(Path.Join(path, CARD_INFO_PATH));
//            var template = JsonSerializer.Deserialize<JCard>(text);

//            if (template is null) 
//                throw new Exception("Failed to load card from " + path);

//            var result = new Card(
//                template.Name
//            );

//            return result;
//        }

//        public LuaTable ToLuaTable(Lua lState)
//        {
//            lState.NewTable("result");
//            var result = lState.GetTable("result");
//            result["name"] = Name;
//            return result;
//        }
//    }
//    #endregion

//    #region InPlay
//    abstract class InPlayCard : Card {
//        protected InPlayCard(string name) : base(name) {

//        }

//    }
//    #endregion

//    #region Damagable
//    abstract class DamagableCard : InPlayCard {
//        // TODO
//        protected DamagableCard(string name) : base(name) {

//        }
//    }
//    #endregion

//    #region Creature
//    class Creature : DamagableCard {
//        // TODO
//        protected Creature(string name) : base(name) {

//        }
//    }

//    #endregion

//    #region Spell 
//    class Spell : Card {
//        protected Spell(string name) : base(name) {

//        }
//    }
//    #endregion

//    #region Bond
//    class Bond : Card {
//        protected Bond(string name) : base(name) {

//        }
//    }
//    #endregion
//}