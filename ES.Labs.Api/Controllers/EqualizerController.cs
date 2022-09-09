using System.Text;
using ES.Labs.Domain;
using ES.Labs.Domain.Events;
using EventStore.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace ES.Labs.Api.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class EqualizerController : ControllerBase
    {
        private readonly IHubContext<TestHub> _hubContext;

        private readonly ILogger<EqualizerController> _logger;

        public EqualizerController(
            IHubContext<TestHub> hubContext, 
            ILogger<EqualizerController> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpGet(Name = "GetEqualizer")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55)
            })
                .ToArray();
        }

        [HttpPost(Name = "SetChannelLevel")]
        public async Task<IActionResult> Set(ChannelLevelChanged data)
        {
            var settings = EventStoreClientSettings
                .Create("esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false");
            var client = new EventStoreClient(settings);

            const string metadata = "{}";

            var eventType = data.GetType().Name.ToLower();
            var eventData = new EventData(
                eventId: Uuid.NewUuid(),
                type: eventType,
                //isJson: true,
                data: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
                metadata: Encoding.UTF8.GetBytes(metadata)
            );
            
            var result = await client.AppendToStreamAsync(
                streamName: $"device-{data.DeviceName}",
                expectedState: StreamState.Any,
                eventData: new List<EventData>
                {
                    eventData
                });

            return Ok(result);
        }
    }
}