using NLua;

using game.core;
using game.deck;
using game.match;
using game.player;

class Program {
    static void Main(string[] args)
    {
        Game game = new Game("../cards");
        MatchConfig mConf = MatchConfig.FromText(File.ReadAllText("../match_configs/test_match.json"));

        var match = game.Matches.CreateMatch(mConf);

        // TODO set deck
        // TODO set deck
        var deck = Deck.FromText(game.Collections, File.ReadAllText("../decks/test.deck"));
        match.AddPlayer("Igor", new TerminalPlayerController(), deck);
        match.AddPlayer("Nastya", new TerminalPlayerController(), deck);
        
        match.Start();
    }
}