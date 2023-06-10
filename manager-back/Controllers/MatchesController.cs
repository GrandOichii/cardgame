using System.Net;
using System.Net.Sockets;
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

        private void RunMatch(MatchRecord record, MatchRequestBody config, Match match)
        {
            // player creation
            var decks = new Deck[2] {
                Deck.FromText(match.Game.CardMaster, config.DeckList1),
                Deck.FromText(match.Game.CardMaster, config.DeckList2)
            };

            var playerWaitTasks = new Task[2];

            // TODO don't know if works
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();

            string p = ((IPEndPoint)listener.LocalEndpoint).Port.ToString();
            var isBotList = new bool[2] {config.P1IsBot, config.P2IsBot};
            for (int i = 0; i < 2; i++) {
                var copyI = i;
                var t = new Task(() => {
                    var pName = "P" + (copyI+1);
                    PlayerController? controller;
                    if (isBotList[copyI]) {
                        // THIS (PROBABLY) THROWS EXCEPTION ON FILE READING
                        controller = new LuaBotController("../bots/test_bot.lua");
                        System.Console.WriteLine("BOT");
                    } else {
                        if (copyI == 0)
                            record.P1Port = p;
                        else record.P2Port = p;
                        

                        controller = new TCPPlayerController(listener, match.Config);
                        
                        if (copyI == 0)
                            record.P1Port = ".";
                        else record.P2Port = ".";
                    }
                    var player = new Player(match, pName, decks[copyI], controller);
                    match.AddPlayer(player);
                });
                playerWaitTasks[copyI] = t;
                t.Start();
            }

            Task.WaitAll(playerWaitTasks);
            System.Console.WriteLine("Players connected");

            try
            {
                record.Status = "IN PROGRESS";
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

            var result = new MatchRecord(match);

            RecordKeeper.Instance.Records.Add(result);
            // RunMatch(result, body, match);

            Task.Run(() => {
                RunMatch(result, body, match);
            });

            return result;
        }

        [HttpGet()]
        public IEnumerable<MatchRecord> Get()
        {
            return RecordKeeper.Instance.Records;
        }
    }
}
