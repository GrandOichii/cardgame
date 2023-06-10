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
    }
}
