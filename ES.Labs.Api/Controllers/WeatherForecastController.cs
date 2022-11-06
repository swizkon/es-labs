using System.Text;
using ES.Labs.Domain;
using EventStore.Client;
//using EventStore.ClientAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
//using EventData = EventStore.ClientAPI.EventData;

namespace ES.Labs.Api.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly IHubContext<TestHub> _hubContext;

        private readonly EventStoreClient _client;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(
            IHubContext<TestHub> hubContext,
            EventStoreClient client,
            ILogger<WeatherForecastController> logger)
        {
            _client = client;
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55)
            })
                .ToArray();
        }

        [HttpPost(Name = "SetWeatherForecast")]
        public async Task<IActionResult> Set(WeatherForecast data)
        {
            //var settings = EventStoreClientSettings
            //    .Create("esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false");
            //var client = new EventStoreClient(settings);

            const string metadata = "{}";

            var eventType = data.GetType().Name.ToLower();
            var eventData = new EventData(
                eventId: Uuid.NewUuid(),
                type: eventType,
                //isJson: true,
                data: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
                metadata: Encoding.UTF8.GetBytes(metadata)
            );
            
            var result = await _client.AppendToStreamAsync(
                streamName: EventStoreConfiguration.StreamName,
                expectedState: StreamState.Any,
                eventData: new List<EventStore.Client.EventData>()
                {
                    eventData
                });

            return Ok(result);
        }
    }
}