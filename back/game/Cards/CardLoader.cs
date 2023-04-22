using System.Text.Json;
using System.Text.Json.Serialization;

using game.util;

namespace game.cards.loaders {

    abstract class CardLoader {
        abstract public Card Load(string cName, string colName);
    } 


    #region File Card Loader
    class JShortCard {
        [JsonPropertyName("name")]
        public string Name { get; set; }="";
        static public JShortCard Load(string path) {
            var text = File.ReadAllText(path);
            var result = JsonSerializer.Deserialize<JShortCard>(text);
            if (result is null) throw new Exception("Failed to load shord card info from path " + path);
            return result;
        }
    }

    class JCard {
        [JsonPropertyName("name")]
        public string Name { get; set; }="";
        [JsonPropertyName("type")]
        public string Type { get; set; }="";
        [JsonPropertyName("text")]
        public string Text { get; set; }="";
        [JsonPropertyName("script")]
        public string ScriptPath { get; set; }="";
        [JsonPropertyName("summoned")]
        public bool Summoned { get; set; }=false;
        [JsonPropertyName("refCards")]
        public List<string> RefCards { get; set; }=new();

        static public JCard Load(string path) {
            var text = File.ReadAllText(path);
            var result = JsonSerializer.Deserialize<JCard>(text);
            if (result is null) throw new Exception("Failed to load shord card info from path " + path);
            return result;
        }
    }

    class JCollection {
        [JsonPropertyName("name")]
        public string Name { get; set; }="";
        [JsonPropertyName("cards")]
        public List<string> Cards { get; set; }=new();
        static public JCollection Load(string path) {
            var text = File.ReadAllText(path);
            var result = JsonSerializer.Deserialize<JCollection>(text);
            if (result is null) throw new Exception("Failed to load collection info from path " + path);
            return result;
        }
    }

    class JCollectionPool {
        [JsonPropertyName("collections")]
        public List<string> Collections { get; set; }=new();
        static public JCollectionPool Load(string path) {
            var text = File.ReadAllText(path);
            var result = JsonSerializer.Deserialize<JCollectionPool>(text);
            if (result is null) throw new Exception("Failed to load collection pool info from path " + path);
            return result;
        }
    }

    class FileCardLoader : CardLoader {
        static private string COLLECTION_POOL_INFO_FILE = "manifest.json";
        static private string COLLECTION_INFO_FILE = "manifest.json";
        static private string CARD_INFO_FILE = "card.json";


        private Dictionary<string, string> _cardIndex; // CardName -- file path
        public FileCardLoader(string colPath) {
            // preload all collections, find what cards are contained in them
            
            _cardIndex = new();

            // load collection manifest file
            var colPool = JCollectionPool.Load(Path.Join(colPath, COLLECTION_POOL_INFO_FILE));

            foreach (var cPath in colPool.Collections) {
                var cP = Path.Join(colPath, cPath);

                var collection = JCollection.Load(Path.Join(cP, COLLECTION_INFO_FILE));
                foreach (var cardPath in collection.Cards) {
                    var parentPath = Path.Join(cP, cardPath);
                    var path = Path.Join(parentPath, CARD_INFO_FILE);
                    var card = JShortCard.Load(path);
                    var fullCardName = FmtCard(card.Name, collection.Name);
                    Logger.Instance.Log("FileCardLoader", "Indexed card " + fullCardName + " to " + parentPath);
                    _cardIndex.Add(fullCardName, parentPath);
                }
            }
            Logger.Instance.Log("FileCardLoader", "Finished indexing cards");
        }

        private string FmtCard(string cName, string colName) => colName + "::" + cName;

        public override Card Load(string cName, string colName)
        {
            var name = FmtCard(cName, colName);
            if (!_cardIndex.ContainsKey(name)) throw new Exception("Card " + name + " is not present in card loader");
            var path = _cardIndex[name];
            var template = JCard.Load(Path.Join(path, CARD_INFO_FILE));
            return new Card(
                template.Name,
                template.Type,
                template.Text,
                colName,
                Path.Join(path, template.ScriptPath),
                template.Summoned,
                template.RefCards
            );
        }
    }
    #endregion


    class DBCardLoader : CardLoader
    {
        public override Card Load(string cName, string colName)
        {
            // TODO
            throw new NotImplementedException();
        }
    }

}