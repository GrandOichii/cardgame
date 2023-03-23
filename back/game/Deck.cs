using NLua;

using game.core;
using game.util;
using game.cards;

namespace game.decks {
    class Deck {

        public Card Bond { get; private set; }
        public Dictionary<Card, int> MainDeck { get; private set; }

        public Deck(Card bond, Dictionary<Card, int> mainDeck) {
            Bond = bond;
            MainDeck = mainDeck;
        }

        public void UnloadCards(CardMaster cMaster) {
            cMaster.Unload(Bond.Name, Bond.Collection);
            foreach (var card in MainDeck.Keys)
                cMaster.Unload(card.Name, card.Collection);
        }

        static private Card ParseCard(CardMaster cMaster, string cName) {
            var split = cName.Split("::");
            if (split.Length != 2) throw new Exception("Failed to split card name " + cName);
            var colName = split[0];
            var cardName = split[1];
            cMaster.Load(cardName, colName);
            return cMaster.Get(cardName, colName);
        }

        static public Deck FromText(CardMaster cMaster, string text) {
            /*
            test_set::Basic Bond
            10 test_set::Source
            8 test_set::New Recruit
            */
            // first line is bond
            Logger.Instance.Log("Deck", "Requested load deck from text\n" + text);
            var lines = text.Split(Environment.NewLine);
            var bond = ParseCard(cMaster, lines[0]);
            int startIndex = 1;
            var nameMap = new Dictionary<string, Card>();
            var cards = new Dictionary<Card, int>();

            for (int i = startIndex; i < lines.Length; i++) {
                var line = lines[i];
                var s = line.Split(" ");
                if (s.Length < 2) throw new Exception("Failed to split line while loading deck: " + line);

                var amount = int.Parse(s[0]);
                var cName = string.Join(" ", s, 1, s.Length-1);
                if (cName is null) throw new Exception("Failed to join back card name: " + line);
                if (nameMap.ContainsKey(cName)) {
                    cards[nameMap[cName]] += amount;
                    continue;
                }
                var card = ParseCard(cMaster, cName);
                nameMap.Add(cName, card);
                cards.Add(card, amount);
            }
            Logger.Instance.Log("Deck", "Loaded!");
            return new Deck(bond, cards);
        }
    
        public CardW CreateBond(Lua lState) => Bond.ConstructWrapper(lState);

        public Zone<CardW> ToDeckZone(Lua lState) {
            var result = new Zone<CardW>(new());
            foreach (var pair in MainDeck) {
                for (int i = 0; i < pair.Value; i++)
                    result.AddToBack(pair.Key.ConstructWrapper(lState));
            }
            return result;
        }
    }

    // class DeckZone : Zone<CardW> {
    //     public CardDeck(List<CardW> cards) : base(cards)
    //     {
    //     }

    //     // static public CardDeck From(Match match, Deck deck) {
    //     //     var cards = new List<CardW>();
    //     //     foreach (var pair in deck.Cards)
    //     //         for (int i = 0; i < pair.Value; i++)
    //     //             cards.Add(new CardW(match, pair.Key));
    //     //     return new CardDeck(cards);
    //     // }

    // }
}