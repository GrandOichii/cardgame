using NLua;

using game.core;
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
        var g = new Game("../cards");
        // Lua state = new();
        // state.DoFile("core.lua");       
    }
}