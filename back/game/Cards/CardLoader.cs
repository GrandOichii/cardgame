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
                System.Console.WriteLine(collection.Name);
                foreach (var cardPath in collection.Cards) {
                    var path = Path.Join(cP, cardPath, CARD_INFO_FILE);
                    var card = JShortCard.Load(path);
                    var fullCardName = FmtCard(card.Name, collection.Name);
                    Logger.Instance.Log("FileCardLoader", "Indexed card " + fullCardName + " to " + path);
                }
            }
            Logger.Instance.Log("FileCardLoader", "Finished indexing cards");
        }

        private string FmtCard(string cName, string colName) => colName + ":" + cName;

        public override Card Load(string cName, string colName)
        {
            
            // TODO not tested
            var name = FmtCard(cName, colName);
            if (!_cardIndex.ContainsKey(name)) throw new Exception("Card " + name + " is not present in card loader");
            return new Card();
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