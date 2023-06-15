using System.Net;
using System.Net.Sockets;
using game.controllers;
using game.decks;
using game.match;
using game.player;
using Microsoft.AspNetCore.Mvc;
using game.recording;

namespace manager_back.Controllers
{
    class MatchCardIndexRecords {
        static public MatchCardIndexRecords Instance { get; }=new();
        private MatchCardIndexRecords() {
            CardIndicies = new();
        }

        public Dictionary<string, Dictionary<string, string>> CardIndicies { get; }
    }

    class RecordKeeper {
        public static RecordKeeper Instance = new();

        public List<MRecord> Records { get; }
        private RecordKeeper() {
            Records = new();
        }
    }

    public class Record {
        public MatchRecord MRecord { get; set; }
        public Dictionary<string, string> CardIndex { get; set; }
        public Record(MatchRecord record, Dictionary<string, string> cIndex) {
            MRecord = record;
            CardIndex = cIndex;
        }
    }

    class PlaybackRecordKeeper {
        static public PlaybackRecordKeeper Instance { get; } = new();
        private PlaybackRecordKeeper() {}

        public Dictionary<string, Record> Records { get; }=new();
    }

    [Route("matches")]
    [ApiController]
    public class MatchesController : ControllerBase
    {

        static private void RunMatch(MRecord record, MatchRequestBody config, Match match)
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

            // save match card index
            var ci = new Dictionary<string, string>();
            foreach (var pair in match.AllCards.Cards) {
                var card = pair.Value;
                ci.Add(pair.Key, card.Original.ToString());
            }
            // MatchCardIndexRecords.Instance.CardIndicies.Add(record.ID);

            // add playback to index
            PlaybackRecordKeeper.Instance.Records.Add(record.ID, new Record(match.Record, ci));
        }

        [HttpPost()]
        public MRecord Post([FromBody] MatchRequestBody body)
        {
            var game = SingletonGame.Instance;
            var config = MatchConfig.FromText(System.IO.File.ReadAllText("../match_configs/normal.json"));
            config.Seed = body.Seed;
            var match = game.MatchPool.NewMatch(game, config);

            var result = new MRecord(match);

            RecordKeeper.Instance.Records.Add(result);
            // RunMatch(result, body, match);

            Task.Run(() => {
                RunMatch(result, body, match);
            });

            return result;
        }

        [HttpGet()]
        public IEnumerable<MRecord> Get()
        {
            return RecordKeeper.Instance.Records;
        }
    }
}
