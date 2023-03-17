

using game.cards;
using game.collection;
using game.core;

namespace game.deck {
    
    class Deck : ITextSerializable {
        
        public Card? Bond { get ; }
        public List<KeyValuePair<Card, int>> Cards { get; }

        
        public Deck(List<KeyValuePair<Card, int>> cards, Card? bond=null) {
            Cards = cards;
            Bond = bond;
        }

        public string ToText()
        {
            // TODO
            throw new NotImplementedException();
        }

        static private Card FromLine(CollectionPool collections, string line) {
            var s = line.Split("::");
            if (s.Length != 2) throw new Exception("Failed to split card line " + line);

            var colName = s[0];
            if (colName is null) throw new Exception("Failed to split card line " + line);
            var cardName = s[1];
            if (cardName is null) throw new Exception("Failed to split card line " + line);

            var collection = collections[colName];
            if (collection is null) throw new Exception("Failed to split card line " + line + ": no collection with name " + colName);
            var card = collection[cardName];
            if (card is null) throw new Exception("Failed to split card line " + line + ": no card with name " + cardName);

            return card;
        }

        static private KeyValuePair<Card, int> FromLineMult(CollectionPool collections, string line) {
            var s = line.Split(" ");
            if (s.Length < 2) throw new Exception("Failed to parse card line " + line);

            var countS = s[0];
            var count = int.Parse(countS);
            var cardName = s[1];
            for (int i = 2; i < s.Length; i++) {
                cardName += " " + s[i];
            }

            return new KeyValuePair<Card, int>(FromLine(collections, cardName), count);
        }

        static public Deck FromText(CollectionPool collections, string text) {
            var lines = text.Split(Environment.NewLine);
            Card? bond = null;
            int startI = 0;
            if (lines.Length > 2 && lines[1] == "") {
                bond = FromLine(collections, lines[1]);
                startI = 2;
            }
            List<KeyValuePair<Card, int>> cards=new();
            for (int i = startI; i < lines.Length; i++) {
                var line = lines[i];
                var card = FromLineMult(collections, line);
                cards.Add(card);
            }
            var result = new Deck(cards, bond);

            return result;
        }
    }

}