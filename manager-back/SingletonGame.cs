namespace manager_back;

using game.cards;
using game.core;
using game.decks;
using Microsoft.OpenApi.Validations.Rules;

class SingletonGame : Game
{
    public static SingletonGame Instance { get; private set; } = new SingletonGame();

    private SingletonGame() : base("../cards")
    {
    }
}

class DeckLoader
{
    public static DeckLoader Instance { get; private set; } = new DeckLoader();

    public Dictionary<string, Deck> Decks { get; private set; }
    private DeckLoader()
    {
        Decks = new();
    }

    public void LoadDecksFrom(string path)
    {
        var dir = Directory.GetFiles(path);
        foreach (var file in dir)
        {
            var deckName = Path.GetFileNameWithoutExtension(file);
            var deck = Deck.FromText(SingletonGame.Instance.CardMaster, File.ReadAllText(file));
            Decks.Add(deckName, deck);
        }
    }
}

public class SDeck
{
    public string Name { get; private set; }
    public string Bond { get; private set; }
    public Dictionary<string, int> Cards { get; set; }

    public SDeck(string name, Deck template)
    {
        Name = name;
        Bond = template.Bond.Collection + "::" + template.Bond.Name;
        Cards = new();
        foreach (var pair in template.MainDeck)
        {
            var card = pair.Key;
            var amount = pair.Value;
            Cards.Add(card.Collection + "::" + card.Name, amount);
            //Cards.Add(new SDeckCard(pair.Key, pair.Value));
        }
    }
}