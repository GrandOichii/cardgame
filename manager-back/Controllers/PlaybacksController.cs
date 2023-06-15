using Microsoft.AspNetCore.Mvc;

namespace manager_back.Controllers
{
    [Route("records")]
    [ApiController]
    public class PlaybacksController : ControllerBase
    {
        [HttpGet()]
        public Dictionary<string, Record> Get()
        {
            return PlaybackRecordKeeper.Instance.Records;
        }

        [HttpGet("{Id}")]
        public Record? GetByID(string Id) {
            foreach (var pair in PlaybackRecordKeeper.Instance.Records) {
                if (pair.Key != Id) continue;

                return pair.Value;
            }
            return null;
        }
    }
}
