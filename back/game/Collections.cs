using System.Text.Json;
using System.Text.Json.Serialization;

using game.core;
using game.cards;

namespace game.collection {

    class JCollection {

    }

    class JCollectionInfo {
        [JsonPropertyName("name")]
        public string Name { get; set; }="";

        [JsonPropertyName("cards")]
        public List<string> CardPaths { get; set; }=new();
    }

    class Collection {

        static private string COLLECTION_INFO_FILE = "manifest.json";


        public string Name { get; }
        public List<Card> Cards { get; }

        public Card? this[string cardName]
        {
            get => Cards.Find(
                card => 
                    card.Name == cardName
            );
        }

        public Collection(string name, List<Card> cards) {
            Name = name;
            Cards = cards;
        }

        static public Collection Load(string path) {
            var infoText = File.ReadAllText(Path.Join(path, COLLECTION_INFO_FILE));
            var info = JsonSerializer.Deserialize<JCollectionInfo>(infoText);
            if (info is null) throw new Exception("Failed to load collection from path" + path);

            // var pDir = Directory.GetParent(path);
            // if (pDir is null) throw new Exception("Failed to get parent dir of " + path);
            // var pDirS = pDir.FullName;

            var cards = new List<Card>();
            foreach (var cPath in info.CardPaths)
                cards.Add(Card.Load(Path.Join(path, cPath)));
            
            var result = new Collection(info.Name, cards);
            return result;
        }
    }

    class JCollectionPoolInfo {
        [JsonPropertyName("collections")]
        public List<string> CollectionPaths { get; set; }=new();
    }

    class CollectionPool {
        static private string COLLECTION_POOL_INFO_FILE = "manifest.json";

        public List<Collection> Collections { get; }

        public CollectionPool(List<Collection> collections) {
            Collections = collections;
        }

        public Collection? this[string collectionName]
        {
            get => Collections.Find(
                collection => 
                    collection.Name == collectionName
            );
        }

        static public CollectionPool Load(string path) {
            var infoText = File.ReadAllText(Path.Join(path, COLLECTION_POOL_INFO_FILE));
            var info = JsonSerializer.Deserialize<JCollectionPoolInfo>(infoText);
            if (info is null) throw new Exception("Failed to load collection info from " + path);

            List<Collection> collections = new();
            foreach (var cPath in info.CollectionPaths)
                collections.Add(Collection.Load(Path.Join(path, cPath)));

            var result = new CollectionPool(collections);
            return result;
        }
    }

}