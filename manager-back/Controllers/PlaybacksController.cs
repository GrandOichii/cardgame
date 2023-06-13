using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace manager_back.Controllers
{
    [Route("records")]
    [ApiController]
    public class PlaybacksController : ControllerBase
    {
        [HttpGet()]
        public Dictionary<string, game.recording.MatchRecord> Get()
        {
            return PlaybackIndex.Instance.Playbacks;
        }

        [HttpGet("{Id}")]
        public game.recording.MatchRecord? GetByID(string Id) {
            foreach (var pair in PlaybackIndex.Instance.Playbacks) {
                if (pair.Key != Id) continue;

                return pair.Value;
            }
            return null;
        }
    }
}
