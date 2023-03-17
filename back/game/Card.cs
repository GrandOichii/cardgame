using System.Text.Json;
using System.Text.Json.Serialization;
using NLua;

using game.core;

/*
name: str
text: str
type: str
script: str


*/

namespace game.cards {
    class CardWrapper {
        static public IIDCreator IDCreator = new IncrementIDCreator(); 

        public string ID { get; }
        public Card Card { get; }
        public CardWrapper(Card card) {
            ID = IDCreator.CreateID();

            Card = card;
        }
    }

    class JCard {
        [JsonPropertyName("name")]
        public string Name { get; set; }="";

        [JsonPropertyName("text")]
        public string Text { get; set; }="";
        [JsonPropertyName("types")]
        public string Type { get; set; }="";

        [JsonPropertyName("script")]
        public string ScriptPath { get; set; }="";
    }

    class Card : ILuaSerializable {
        static private string CARD_INFO_PATH = "card.json";

        public string Name { get; }
        public string Text { get; }
        public string Type { get; }

        protected Card(string name, string text, string type) {
            Name = name;
            Text = text;
            Type = type;
        }

        static public Card Load(string path) {
            var text = File.ReadAllText(Path.Join(path, CARD_INFO_PATH));
            var template = JsonSerializer.Deserialize<JCard>(text);

            if (template is null) 
                throw new Exception("Failed to load card from " + path);

            var result = new Card(
                template.Name,
                template.Text,
                template.Type
            );

            return result;
        }

        public LuaTable ToLuaTable(Lua lState)
        {
            lState.NewTable("result");
            var result = lState.GetTable("result");
            result["name"] = Name;
            return result;
        }
    }
}

namespace game.cards1 {

    #region Wrapper

    class CardWrapper {
        static public IIDCreator IDCreator = new IncrementIDCreator(); 

        private string ID { get; }
        private Card Card { get; }
        public CardWrapper(Card card) {
            ID = IDCreator.CreateID();

            Card = card;
        }
    }
    
    #endregion
    
    #region Base
    class JCard {
        [JsonPropertyName("name")]
        public string Name { get; set; }="";

        [JsonPropertyName("text")]
        public string Text { get; set; }="";

        [JsonPropertyName("script")]
        public string ScriptPath { get; set; }="";
    }

    class Card : ILuaSerializable {
        static private string CARD_INFO_PATH = "card.json";

        public string Name { get; }

        protected Card(string name) {
            Name = name;
        }

        static public Card Load(string path) {
            var text = File.ReadAllText(Path.Join(path, CARD_INFO_PATH));
            var template = JsonSerializer.Deserialize<JCard>(text);

            if (template is null) 
                throw new Exception("Failed to load card from " + path);

            var result = new Card(
                template.Name
            );

            return result;
        }

        public LuaTable ToLuaTable(Lua lState)
        {
            lState.NewTable("result");
            var result = lState.GetTable("result");
            result["name"] = Name;
            return result;
        }
    }
    #endregion

    #region InPlay
    abstract class InPlayCard : Card {
        protected InPlayCard(string name) : base(name) {

        }

    }
    #endregion

    #region Damagable
    abstract class DamagableCard : InPlayCard {
        // TODO
        protected DamagableCard(string name) : base(name) {

        }
    }
    #endregion

    #region Creature
    class Creature : DamagableCard {
        // TODO
        protected Creature(string name) : base(name) {

        }
    }

    #endregion

    #region Spell 
    class Spell : Card {
        protected Spell(string name) : base(name) {

        }
    }
    #endregion

    #region Bond
    class Bond : Card {
        protected Bond(string name) : base(name) {

        }
    }
    #endregion
}