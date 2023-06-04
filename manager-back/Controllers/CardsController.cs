using game.cards;
using Microsoft.AspNetCore.Mvc;

namespace manager_back.Controllers
{
    [ApiController]
    [Route("cards")]
    public class CardsController : ControllerBase
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet()]
        public IEnumerable<Card> GetAll()
        {
            return SingletonGame.Instance.CardLoader.All();
        }

        //[HttpGet()]
        //public IEnumerable<Card> GetFiltered([/FromQuery(Name = "type")] string type)
        //{
        //    HttpContext.Request.Query
        //    return from card in GetAll()
        //           where card.Type == type
        //           select card;
        //}
    }
}

/*
 * using Microsoft.AspNetCore.Mvc;

namespace manager_back.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
 */