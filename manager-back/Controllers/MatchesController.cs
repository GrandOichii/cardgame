using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace manager_back.Controllers
{
    [Route("matches")]
    [ApiController]
    public class MatchesController : ControllerBase
    {
        [HttpPost]
        public MatchRequestBody Post([FromBody] MatchRequestBody body)
        {
            return body;
        }
    }
}
