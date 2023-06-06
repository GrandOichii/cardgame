using game.controllers;
using game.decks;
using game.match;
using game.player;
using Microsoft.AspNetCore.Mvc;

namespace manager_back.Controllers
{
    class RecordKeeper {
        public static RecordKeeper Instance = new();

        public List<MatchRecord> Records { get; }
        private RecordKeeper() {
            Records = new();
        }
    }

    [Route("matches")]
    [ApiController]
    public class MatchesController : ControllerBase
    {

        private async void RunMatch(MatchRecord record, Match match)
        {
            // await Task.Delay(2000);
            try
            {
                match.Start();

                var winner = match.Winner;
                if (winner is null) throw new Exception("Match ended, yet no winner found");

                record.Winner = winner.Name;
                record.Status = "FINISHED";
            } catch (Exception ex)
            {
                record.ErrorMsg = ex.ToString();
                record.Status = "CRASHED";
            }

            record.TimeEnd = DateTime.Now.ToString();

            // end of match
            foreach (var player in match.Players)
            {
                player.OriginalDeck.UnloadCards(match.Game.CardMaster);
            }


        }

        [HttpPost()]
        public MatchRecord Post([FromBody] MatchRequestBody body)
        {
            var game = SingletonGame.Instance;
            var config = MatchConfig.FromText(System.IO.File.ReadAllText("../match_configs/normal.json"));
            config.Seed = body.Seed;
            var match = game.MatchPool.NewMatch(game, config);

            var deck1 = Deck.FromText(game.CardMaster, body.DeckList1);
            var deck2 = Deck.FromText(game.CardMaster, body.DeckList2);

            // TODO for now only bots
            var p1 = new Player(match, "P1", deck1, new LuaBotController("../bots/test_bot.lua"));
            var p2 = new Player(match, "P2", deck2, new LuaBotController("../bots/test_bot.lua"));

            match.AddPlayer(p1);
            match.AddPlayer(p2);

            var record = new MatchRecord(match);
            RecordKeeper.Instance.Records.Add(record);

            RunMatch(record, match);

            return record;
        }

        [HttpGet()]
        public IEnumerable<MatchRecord> Get()
        {
            return RecordKeeper.Instance.Records;
        }
    }
}
