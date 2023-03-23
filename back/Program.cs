using NLua;

using game.match;
using game.core;
using game.player;
using game.decks;
// using game.deck;
// using game.match;
// using game.player;

// class Program {
//     static void Main()
//     {

//         Game game = new Game("../cards");        
//         // Game game = new Game("C:\\Users\\ihawk\\code\\cardgame\\cards");
//         MatchConfig mConf = MatchConfig.FromText(File.ReadAllText("../match_configs/test_match.json"));
//         // MatchConfig mConf = MatchConfig.FromText(File.ReadAllText("C:\\Users\\ihawk\\code\\cardgame\\match_configs\\test_match.json"));

//         var match = game.Matches.CreateMatch(mConf);

//         var deck = Deck.FromText(game.Collections, File.ReadAllText("../decks/test.deck"));
//         // var deck = Deck.FromText(game.Collections, File.ReadAllText("C:\\Users\\ihawk\\code\\cardgame\\decks\\test.deck"));

//         match.AddPlayer("Igor", new TerminalPlayerController(), deck);
//         match.AddPlayer("Nastya", new TerminalPlayerController(), deck);

//         match.Start();
//         // Laputa.Run();
//     }
// }

class Program {
    static void Main(string[] args)
    {
        #region Game Creation
        var g = new Game("../cards");

        #region Match Creation
        string configPath = "../match_configs/test_match.json";
        var m = g.MatchPool.NewMatch(MatchConfig.FromText(File.ReadAllText(configPath)));

        var deck1 = Deck.FromText(g.CardMaster, File.ReadAllText("../decks/test.deck"));
        var p1 = new Player(m, "Igor", deck1, new TerminalPlayerController());
        var deck2 = Deck.FromText(g.CardMaster, File.ReadAllText("../decks/test.deck"));
        var p2 = new Player(m, "Nastya", deck2, new TerminalPlayerController());
        g.CardMaster.LogContents();

        deck1.UnloadCards(g.CardMaster);
        deck2.UnloadCards(g.CardMaster);
        m.Start();
        #endregion

        bool clear = g.CardMaster.CheckEmpty();
        if (clear) return;

        g.CardMaster.LogContents();
        throw new Exception("Not all cards are unloaded from CardMaster");
        #endregion
    }
}